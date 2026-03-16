using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
// using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuizHub.Data;
// using QuizHub.HealthChecks;
using QuizHub.Middleware;
using QuizHub.Models.Entities;
using QuizHub.Services;
using QuizHub.Services.BackgroundServices;
using QuizHub.Services.Implementations;
using QuizHub.Services.Interfaces;
using QuizHub.Swagger;
using QuizHub.Validators.Auth;


var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. DATABASE CONNECTION
// ============================================
builder.Services.AddDbContext<QuizHubDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// ============================================
// 1.5. MEMORY CACHE (REQUIRED FOR BACKGROUND SERVICES)
// ============================================
builder.Services.AddMemoryCache();

// ============================================
// 1.6. HTTP CLIENT FACTORY (REQUIRED FOR HEALTH CHECKS)
// ============================================
builder.Services.AddHttpClient();

// ============================================
// 1.7. FLUENT VALIDATION
// ============================================
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

// ============================================
// 1.8. HEALTH CHECKS
// ============================================
// builder.Services.AddHealthChecks()
//     .AddSqlServer(
//         builder.Configuration.GetConnectionString("DefaultConnection")!,
//         name: "sqlserver",
//         tags: new[] { "db", "sql", "sqlserver" })
//     .AddCheck<SignalRHealthCheck>("signalr", tags: new[] { "signalr", "realtime" })
//     .AddCheck<GeminiAIHealthCheck>("gemini-ai", tags: new[] { "ai", "external" })
//     .AddCheck<EmailServiceHealthCheck>("email", tags: new[] { "email", "smtp" });

// ============================================
// 1.9. RATE LIMITING - Production-Optimized Multi-Strategy
// ============================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // --------------------------------------------------------
    // GLOBAL LIMITER: Sliding Window
    // Mượt hơn Fixed Window, tránh burst traffic tại ranh giới cửa sổ
    // 100 requests/phút, chia thành 6 segments (mỗi 10 giây)
    // --------------------------------------------------------
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6, // Chia nhỏ để phân bố đều hơn
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            }));

    // --------------------------------------------------------
    // AUTH POLICY: Sliding Window (Relaxed for Testing)
    // Bảo vệ chống brute-force login/register
    // 20 requests/1 phút per IP - nới lỏng để test (Production nên đặt 5/5phút)
    // --------------------------------------------------------
    options.AddSlidingWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;          // 5 requests
        opt.Window = TimeSpan.FromMinutes(1); // Giảm từ 5 phút xuống 1 phút
        opt.SegmentsPerWindow = 6; // 10 giây/segment
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;           // Không cho phép queue để phản hồi nhanh với brute-force attempts
    });

    // --------------------------------------------------------
    // AI POLICY: Token Bucket
    // Phù hợp cho AI vì cho phép burst nhỏ nhưng giới hạn tổng thể
    // 10 tokens, nạp thêm 5 tokens/phút
    // --------------------------------------------------------
    options.AddTokenBucketLimiter("ai", opt =>
    {
        opt.TokenLimit = 10;           // Tối đa 10 tokens trong bucket
        opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
        opt.TokensPerPeriod = 5;       // Nạp 5 tokens mỗi phút
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 3;
        opt.AutoReplenishment = true;
    });

    // --------------------------------------------------------
    // QUIZ POLICY: Concurrency Limiter
    // Giới hạn số lượng làm bài đồng thời để bảo vệ server
    // Max 5 concurrent, queue 10 pending
    // --------------------------------------------------------
    options.AddConcurrencyLimiter("quiz", opt =>
    {
        opt.PermitLimit = 10;           // Max 10 concurrent requests
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;           // Hàng đợi 10 requests
    });

    // --------------------------------------------------------
    // PUBLIC POLICY: Fixed Window (Lenient)
    // Cho các endpoint công khai như danh sách câu hỏi, danh mục
    // 200 requests/phút - khá rộng rãi
    // --------------------------------------------------------
    options.AddFixedWindowLimiter("public", opt =>
    {
        opt.PermitLimit = 200;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 20;
    });

    // --------------------------------------------------------
    // Custom rejection response với thông tin retry
    // --------------------------------------------------------
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
            ? (int)Math.Ceiling(retryAfter.TotalSeconds)
            : 60;
        
        // Thêm header Retry-After chuẩn HTTP
        context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
        
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Success = false,
            Message = "Quá nhiều yêu cầu. Vui lòng thử lại sau.",
            RetryAfterSeconds = retryAfterSeconds,
            PolicyHit = context.Lease.TryGetMetadata(MetadataName.ReasonPhrase, out var reason) 
                ? reason 
                : "Rate limit exceeded"
        }, token);
    };
});

// ============================================
// 2. IDENTITY CONFIGURATION
// ============================================
builder.Services.AddIdentity<NguoiDung, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Đổi thành true nếu dùng email confirmation
})
.AddEntityFrameworkStores<QuizHubDbContext>()
.AddDefaultTokenProviders();

// Cấu hình thời gian sống của token (ví dụ: 10 phút cho email confirmation, password reset)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(10);
});

//ĐĂNG KÝ SERVICE

// Đăng ký Argon2PasswordHasher thay vì PBKDF2 mặc định
//builder.Services.AddScoped<IPasswordHasher<NguoiDung>, Argon2PasswordHasher>();

// Thêm sau phần AddIdentity
// Đang ký Services cho Người Dùng
builder.Services.AddScoped<IJwtService, JwtService>();
// Đăng ký Services cho Email & Profile
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
// Đăng ký Services cho Danh Mục
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
// Đăng ký Services cho Câu Hỏi
builder.Services.AddScoped<ICauHoiService, CauHoiService>();
// Đăng ký Services cho Bài Thi
builder.Services.AddScoped<IBaiThiService, BaiThiService>();
// Đăng ký Services cho Lượt Làm Bài
builder.Services.AddScoped<ILuotLamBaiService, LuotLamBaiService>();
// Đăng ký Services cho Đánh Giá
builder.Services.AddScoped<IDanhGiaService, DanhGiaService>();
// Đăng ký Services cho Báo Cáo
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
// Đăng ký Services cho Admin
builder.Services.AddScoped<IAdminService, AdminService>();
// Đăng ký Services cho AI (Gemini)
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();

// Background Services

// ============================================
// SIGNALR CONFIGURATION
// ============================================
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});
// Đăng ký Background Service để tự động nộp bài thi khi hết giờ
builder.Services.AddHostedService<AutoSubmitQuizBackgroundService>();
// Đăng ký Background Service để gửi countdown mỗi giây
builder.Services.AddHostedService<QuizCountdownBackgroundService>();

// ============================================
// 3. JWT AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // HTTPS required in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

// ============================================
// 4. CORS CONFIGURATION
// ============================================
builder.Services.AddCors(options =>
{
    // Policy cho Development
    options.AddPolicy("AllowVueApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:8080",
                    "http://localhost:3000"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            // Production - chỉ cho phép các domain cụ thể
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] {
                    "https://quizhub.me",
                    "https://www.quizhub.me",
                    "https://quizhub-ui.vercel.app"
                };
            
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// ============================================
// 5. CONTROLLERS & API EXPLORER
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();

// ============================================
// 6. SWAGGER CONFIGURATION (FIX LỖI)
// ============================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "QuizHub API",
        Description = "API cho hệ thống tạo và chia sẻ đề thi trắc nghiệm thông minh",
        Contact = new OpenApiContact
        {
            Name = "QuizHub Support",
            Email = "support@quizhub.com"
        }
    });

    //   FIX: Cấu hình JWT cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào ô bên dưới. Ví dụ: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    //   Custom schema IDs để tránh conflict
    options.CustomSchemaIds(type => type.FullName);
    
    //   Bỏ qua lỗi trong mô tả API
    options.IgnoreObsoleteActions();
    options.IgnoreObsoleteProperties();
    
    //  Xử lý file upload
 options.OperationFilter<FileUploadOperationFilter>();
});

// ============================================
// 7. BUILD APP
// ============================================
var app = builder.Build();

// ============================================
// 8. MIDDLEWARE PIPELINE
// ============================================
//   Global Exception Handler - đặt đầu tiên để bắt tất cả exceptions
app.UseGlobalExceptionHandler();

//   Security Headers
app.UseSecurityHeaders();

//   Request Logging
app.UseRequestLogging();

//   Developer Exception Page (chỉ trong Development)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizHub API V1");
        c.RoutePrefix = "swagger"; // Truy cập tại: /swagger
    });
}
else
{
    //   HSTS cho Production
    app.UseHsts();
}

app.UseHttpsRedirection();

//   Rate Limiting
app.UseRateLimiter();

app.UseCors("AllowVueApp");

app.UseAuthentication(); // Phải đặt trước UseAuthorization
app.UseAuthorization();

//   Static Files (cho avatars, uploads)
app.UseStaticFiles();

app.MapControllers();

// //   Health Check Endpoints
// app.MapHealthChecks("/health", new HealthCheckOptions
// {
//     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
// });

// app.MapHealthChecks("/health/ready", new HealthCheckOptions
// {
//     Predicate = check => check.Tags.Contains("db"),
//     ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
// });

// app.MapHealthChecks("/health/live", new HealthCheckOptions
// {
//     Predicate = _ => false // Chỉ check app có sống không
// });

// Auto migrate database and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<QuizHubDbContext>();

        const int maxRetries = 15;
        var delay = TimeSpan.FromSeconds(4);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
                break;
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    throw;
                }

                logger.LogWarning(ex,
                    "Database migration attempt {Attempt}/{MaxRetries} failed. Retrying in {DelaySeconds} seconds...",
                    attempt,
                    maxRetries,
                    delay.TotalSeconds);

                await Task.Delay(delay);
            }
        }

        await QuizHub.Data.DbInitializer.SeedData(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating/seeding the database.");
    }
}

app.MapHub<QuizHub.Hubs.QuizHub>("/hubs/quiz"); //DÒNG NÀY trước app.Run()

app.Run();
