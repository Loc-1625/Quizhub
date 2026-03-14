using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.EntityFrameworkCore;
using QuizHub.Data;
using QuizHub.Models.DTOs.AI;
using QuizHub.Models.Entities;
using QuizHub.Services.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mscc.GenerativeAI;

namespace QuizHub.Services.Implementations
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly QuizHubDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly string _uploadPath;
        private readonly string _geminiApiKey;
        private readonly string _modelName;
        private readonly int _maxOutputTokens;
        private readonly float _temperature;

        public GeminiAIService(
            QuizHubDbContext context, 
            IConfiguration configuration, 
            IWebHostEnvironment env,
            ILogger<GeminiAIService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _uploadPath = Path.Combine(env.WebRootPath ?? env.ContentRootPath, "uploads", "ai-extractions");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);

            // Load Gemini configuration
            _geminiApiKey = _configuration["GeminiAI:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key not configured");
            _modelName = _configuration["GeminiAI:Model"] ?? "gemini-2.0-flash-exp";
            _maxOutputTokens = _configuration.GetValue<int>("GeminiAI:MaxTokens", 8192);
            _temperature = _configuration.GetValue<float>("GeminiAI:Temperature", 1.0f);
        }

        public async Task<ExtractionResultDto> ExtractQuestionsFromFileAsync(IFormFile file, string userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var maPhien = Guid.NewGuid();

            try
            {
                // Kiểm tra tệp tin
                var allowedExtensions = new[] { ".docx", ".pdf", ".txt" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException("Chỉ hỗ trợ file .docx, .pdf, hoặc .txt");
                }

                if (file.Length > 10 * 1024 * 1024) // 10MB
                {
                    throw new InvalidOperationException("Kích thước file không được vượt quá 10MB");
                }

                // Trích xuất văn bản từ tệp tin
                string extractedText = extension switch
                {
                    ".docx" => await ExtractTextFromDocxAsync(file),
                    ".pdf" => await ExtractTextFromPdfAsync(file),
                    ".txt" => await ExtractTextFromTxtAsync(file),
                    _ => throw new InvalidOperationException("Định dạng file không được hỗ trợ")
                };

                // Log văn bản đã trích xuất để gỡ lỗi
                _logger.LogDebug("Extracted text length: {Length}", extractedText.Length);
                if (extractedText.Length > 0)
                {
                    _logger.LogDebug("First 500 chars: {Text}", extractedText.Substring(0, Math.Min(500, extractedText.Length)));
                }

                // Sử dụng GEMINI AI để phân tích câu hỏi
                var questions = await ExtractQuestionsUsingGeminiAsync(extractedText);

                // Log kết quả AI
                _logger.LogDebug("AI extracted {Count} questions", questions.Count);

                // Lưu phiên vào database
                var phien = new PhienNhapDuLieu
                {
                    MaPhien = maPhien,
                    NguoiDungId = userId,
                    TenTepTin = file.FileName,
                    LoaiTepTin = extension,
                    KichThuocTepTin = file.Length,
                    TrangThai = questions.Any() ? "ThanhCong" : "ThatBai",
                    SoCauHoiTrichXuat = questions.Count,
                    MoHinhAI = _modelName,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };

                _context.PhienNhapDuLieu.Add(phien);

                // Lưu các câu hỏi đã trích xuất dưới dạng JSON
                var jsonPath = Path.Combine(_uploadPath, $"{maPhien}.json");
                var jsonData = JsonSerializer.Serialize(questions, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(jsonPath, jsonData);

                await _context.SaveChangesAsync();

                stopwatch.Stop();

                return new ExtractionResultDto
                {
                    MaPhien = maPhien,
                    TenTepTin = file.FileName,
                    TrangThai = phien.TrangThai,
                    SoCauHoiTrichXuat = questions.Count,
                    CacCauHoi = questions,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Lưu phiên thất bại
                var failedPhien = new PhienNhapDuLieu
                {
                    MaPhien = maPhien,
                    NguoiDungId = userId,
                    TenTepTin = file.FileName,
                    LoaiTepTin = Path.GetExtension(file.FileName),
                    KichThuocTepTin = file.Length,
                    TrangThai = "ThatBai",
                    ThongBaoLoi = ex.Message,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };

                _context.PhienNhapDuLieu.Add(failedPhien);
                await _context.SaveChangesAsync();

                return new ExtractionResultDto
                {
                    MaPhien = maPhien,
                    TenTepTin = file.FileName,
                    TrangThai = "ThatBai",
                    ThongBaoLoi = ex.Message,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        private async Task<List<ExtractedQuestionDto>> ExtractQuestionsUsingGeminiAsync(string text)
        {
            try
            {
                // khởi tạo Gemini model với API key và config
                var googleAI = new GoogleAI(_geminiApiKey);
                var generationConfig = new GenerationConfig
                {
                    MaxOutputTokens = _maxOutputTokens,
                    Temperature = _temperature
                };
                var model = googleAI.GenerativeModel(model: _modelName, generationConfig: generationConfig);

                var prompt = $@"Bạn là một chuyên gia phân tích câu hỏi trắc nghiệm. Hãy trích xuất TẤT CẢ các câu hỏi trắc nghiệm từ văn bản sau và trả về dưới dạng JSON array.

YÊU CẦU:
- Mỗi câu hỏi PHẢI có:
  + noiDungCauHoi: Nội dung câu hỏi (bắt buộc)
  + cacLuaChon: Mảng 4 đáp án (bắt buộc)
  + dapAnDung: Index của đáp án đúng từ 0-3 (bắt buộc)
  + giaiThich: Giải thích ngắn gọn lý do đáp án đúng (bắt buộc)
  + mucDo: Độ khó của câu hỏi (bắt buộc) - phải là một trong 3 giá trị: De, TrungBinh, Kho
    - De: Câu hỏi cơ bản, dễ nhớ, phù hợp người mới
    - TrungBinh: Câu hỏi đòi hỏi hiểu biết nhất định
    - Kho: Câu hỏi phức tạp, đòi hỏi hiểu biết sâu hoặc suy luận

- Hãy ĐÁNH GIÁ ĐỘ KHÓ PHÙ HỢP cho từng câu dựa trên độ phức tạp của nội dung
- Chỉ trả về JSON array, KHÔNG có text giải thích thêm
- Nếu không tìm thấy câu hỏi nào, trả về mảng rỗng []

VĂN BẢN:
{text}

JSON OUTPUT (chỉ JSON array, không có markdown):";

                var response = await model.GenerateContent(prompt);
                var responseText = response?.Text?.Trim() ?? "";

                // Loại bỏ khối mã markdown nếu có
                responseText = Regex.Replace(responseText, @"^```json\s*", "", RegexOptions.Multiline);
                responseText = Regex.Replace(responseText, @"```\s*$", "", RegexOptions.Multiline);
                responseText = responseText.Trim();

                if (string.IsNullOrEmpty(responseText) || responseText == "[]")
                {
                    return new List<ExtractedQuestionDto>();
                }

                // Phân tích phản hồi JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var questions = JsonSerializer.Deserialize<List<ExtractedQuestionDto>>(responseText, options);
                return questions ?? new List<ExtractedQuestionDto>();
            }
            catch (Exception ex)
            {
                // Dự phòng bằng cách sử dụng pattern matching nếu AI thất bại
                _logger.LogWarning(ex, "Gemini AI Error. Falling back to pattern matching.");
                return ParseQuestionsFromText(text);
            }
        }


        public async Task<ExtractionResultDto?> GetExtractionResultAsync(Guid maPhien, string userId)
        {
            var phien = await _context.PhienNhapDuLieu
                        .FirstOrDefaultAsync(p => p.MaPhien == maPhien && p.NguoiDungId == userId);

            if (phien == null)
                return null;

            var result = new ExtractionResultDto
            {
                MaPhien = phien.MaPhien,
                TenTepTin = phien.TenTepTin,
                TrangThai = phien.TrangThai,
                SoCauHoiTrichXuat = phien.SoCauHoiTrichXuat,
                ThongBaoLoi = phien.ThongBaoLoi,
                ThoiGianXuLy = phien.ThoiGianXuLy
            };

            // Lấy câu hỏi từ JSON nếu thành công
            if (phien.TrangThai == "ThanhCong")
            {
                var jsonPath = Path.Combine(_uploadPath, $"{maPhien}.json");
                if (File.Exists(jsonPath))
                {
                    var jsonData = await File.ReadAllTextAsync(jsonPath);
                    result.CacCauHoi = JsonSerializer.Deserialize<List<ExtractedQuestionDto>>(jsonData) ?? new();
                }
            }

            return result;
        }

        public async Task<int> ImportQuestionsAsync(ImportQuestionDto dto, string userId)
        {
            var phien = await _context.PhienNhapDuLieu
           .FirstOrDefaultAsync(p => p.MaPhien == dto.MaPhien && p.NguoiDungId == userId);

            if (phien == null || phien.TrangThai != "ThanhCong")
                throw new InvalidOperationException("Phiên import không hợp lệ");

            // Lấy câu hỏi từ JSON
            var jsonPath = Path.Combine(_uploadPath, $"{dto.MaPhien}.json");
            if (!File.Exists(jsonPath))
                throw new InvalidOperationException("Không tìm thấy dữ liệu câu hỏi");

            var jsonData = await File.ReadAllTextAsync(jsonPath);
            var allQuestions = JsonSerializer.Deserialize<List<ExtractedQuestionDto>>(jsonData) ?? new();

            // Lọc các câu hỏi được chọn
            var selectedQuestions = dto.IndexCauHoi
           .Where(i => i >= 0 && i < allQuestions.Count)
           .Select(i => allQuestions[i])
                .ToList();

            int importCount = 0;

            foreach (var q in selectedQuestions)
            {
                var cauHoi = new CauHoi
                {
                    NguoiTaoId = userId,
                    MaDanhMuc = dto.MaDanhMuc,
                    NoiDungCauHoi = q.NoiDungCauHoi,
                    GiaiThich = q.GiaiThich,
                    LoaiCauHoi = "TracNghiem",
                    MucDo = q.MucDo ?? "TrungBinh",
                    CongKhai = dto.CongKhai,
                    NguonNhap = "AI-Import"
                };

                _context.CauHoi.Add(cauHoi);
                await _context.SaveChangesAsync(); // Save to get MaCauHoi

                // Add choices
                for (int i = 0; i < q.CacLuaChon.Count && i < 4; i++)
                {
                    var luaChon = new LuaChonDapAn
                    {
                        MaCauHoi = cauHoi.MaCauHoi,
                        NoiDungDapAn = q.CacLuaChon[i],
                        LaDapAnDung = i == q.DapAnDung,
                        ThuTu = i + 1
                    };
                    _context.LuaChonDapAn.Add(luaChon);
                }

                importCount++;
            }

            // Cập nhật số câu hỏi đã nhập
            phien.SoCauHoiNhap = importCount;
            await _context.SaveChangesAsync();

            return importCount;
        }

        public async Task<(IEnumerable<PhienNhapDuLieuDto> Items, int TotalCount)> GetImportHistoryAsync(
 string userId,
            int pageNumber = 1,
   int pageSize = 20)
        {
            var query = _context.PhienNhapDuLieu
        .Where(p => p.NguoiDungId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
              .OrderByDescending(p => p.NgayTao)
                        .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .Select(p => new PhienNhapDuLieuDto
          {
              MaPhien = p.MaPhien,
              TenTepTin = p.TenTepTin,
              LoaiTepTin = p.LoaiTepTin,
              KichThuocTepTin = p.KichThuocTepTin,
              TrangThai = p.TrangThai,
              SoCauHoiTrichXuat = p.SoCauHoiTrichXuat,
              SoCauHoiNhap = p.SoCauHoiNhap,
              ThongBaoLoi = p.ThongBaoLoi,
              MoHinhAI = p.MoHinhAI,
              ThoiGianXuLy = p.ThoiGianXuLy,
              NgayTao = p.NgayTao
          })
                 .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> DeleteSessionAsync(Guid maPhien, string userId)
        {
            var phien = await _context.PhienNhapDuLieu
    .FirstOrDefaultAsync(p => p.MaPhien == maPhien && p.NguoiDungId == userId);

            if (phien == null)
                return false;

            // Delete JSON file
            var jsonPath = Path.Combine(_uploadPath, $"{maPhien}.json");
            if (File.Exists(jsonPath))
                File.Delete(jsonPath);

            _context.PhienNhapDuLieu.Remove(phien);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GenerateResultDto> GenerateQuestionsFromTopicAsync(string topic, int numberOfQuestions, string difficulty, string userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var maPhien = Guid.NewGuid();

            try
            {
                var questions = await GenerateQuestionsUsingGeminiAsync(topic, numberOfQuestions, difficulty);

                var phien = new PhienNhapDuLieu
                {
                    MaPhien = maPhien,
                    NguoiDungId = userId,
                    TenTepTin = $"AI-Generate: {topic.Substring(0, Math.Min(50, topic.Length))}",
                    LoaiTepTin = "AI-Generate",
                    KichThuocTepTin = 0,
                    TrangThai = questions.Any() ? "ThanhCong" : "ThatBai",
                    SoCauHoiTrichXuat = questions.Count,
                    MoHinhAI = _modelName,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };

                _context.PhienNhapDuLieu.Add(phien);

                var jsonPath = Path.Combine(_uploadPath, $"{maPhien}.json");
                var jsonData = JsonSerializer.Serialize(questions, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(jsonPath, jsonData);

                await _context.SaveChangesAsync();
                stopwatch.Stop();

                return new GenerateResultDto
                {
                    MaPhien = maPhien,
                    ChuDe = topic,
                    TrangThai = phien.TrangThai,
                    SoCauHoiTao = questions.Count,
                    CacCauHoi = questions,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Generate questions from topic failed");

                return new GenerateResultDto
                {
                    MaPhien = maPhien,
                    ChuDe = topic,
                    TrangThai = "ThatBai",
                    ThongBaoLoi = ex.Message,
                    ThoiGianXuLy = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        private async Task<List<ExtractedQuestionDto>> GenerateQuestionsUsingGeminiAsync(string topic, int numberOfQuestions, string difficulty)
        {
            try
            {
                var googleAI = new GoogleAI(_geminiApiKey);
                var generationConfig = new GenerationConfig
                {
                    MaxOutputTokens = _maxOutputTokens,
                    Temperature = _temperature
                };
                var model = googleAI.GenerativeModel(model: _modelName, generationConfig: generationConfig);

                var prompt = $@"Bạn là chuyên gia tạo câu hỏi trắc nghiệm. Hãy tạo {numberOfQuestions} câu hỏi trắc nghiệm về chủ đề sau:

CHỦ ĐỀ: {topic}

YÊU CẦU:
- Tạo câu hỏi với ĐỘ KHÓ ĐA DẠNG (có cả dễ, trung bình và khó)
- Mỗi câu hỏi PHẢI có đúng 4 đáp án (A, B, C, D)
- Chỉ có 1 đáp án đúng cho mỗi câu
- Câu hỏi phải rõ ràng, chính xác
- Các đáp án sai phải hợp lý (không quá dễ đoán)
- HÃY TỰ ĐÁNH GIÁ VÀ GÁN ĐỘ KHÓ PHÙ HỢP cho từng câu:
  - De: Câu hỏi cơ bản, dễ nhớ, phù hợp người mới
  - TrungBinh: Câu hỏi đòi hỏi hiểu biết nhất định
  - Kho: Câu hỏi phức tạp, đòi hỏi hiểu biết sâu hoặc suy luận

ĐỊNH DẠNG OUTPUT (JSON array, KHÔNG có markdown):
[
  {{
    ""noiDungCauHoi"": ""Nội dung câu hỏi"",
    ""cacLuaChon"": [""Đáp án A"", ""Đáp án B"", ""Đáp án C"", ""Đáp án D""],
    ""dapAnDung"": 0,
    ""giaiThich"": ""Giải thích ngắn gọn lý do đáp án đúng"",
    ""mucDo"": ""De hoặc TrungBinh hoặc Kho""
  }}
]

Chỉ trả về JSON array, không có text giải thích thêm.";

                var response = await model.GenerateContent(prompt);
                var responseText = response?.Text?.Trim() ?? "";

                // Debug logging
                _logger.LogInformation("Gemini response length: {Length}", responseText.Length);
                if (responseText.Length < 500)
                {
                    _logger.LogInformation("Gemini response: {Response}", responseText);
                }
                else
                {
                    _logger.LogInformation("Gemini response (first 500 chars): {Response}", responseText.Substring(0, 500));
                }

                // Loại bỏ khối mã markdown nếu có
                responseText = Regex.Replace(responseText, @"^```json\s*", "", RegexOptions.Multiline);
                responseText = Regex.Replace(responseText, @"```\s*$", "", RegexOptions.Multiline);
                responseText = responseText.Trim();

                if (string.IsNullOrEmpty(responseText) || responseText == "[]")
                {
                    _logger.LogWarning("Gemini returned empty response for topic: {Topic}, numberOfQuestions: {Count}", topic, numberOfQuestions);
                    throw new InvalidOperationException("AI không tạo được câu hỏi. Vui lòng thử lại với mô tả chi tiết hơn.");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<ExtractedQuestionDto>>(responseText, options);
                
                return questions ?? new List<ExtractedQuestionDto>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse AI response as JSON");
                throw new InvalidOperationException("Không thể xử lý phản hồi từ AI. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini AI generation failed");
                throw;
            }
        }

        // PRIVATE HELPER METHODS
        private Task<string> ExtractTextFromDocxAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var doc = WordprocessingDocument.Open(stream, false);

            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null)
                return Task.FromResult(string.Empty);

            return Task.FromResult(body.InnerText);
        }

        private Task<string> ExtractTextFromPdfAsync(IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                using var pdfReader = new PdfReader(stream);
                using var pdfDoc = new PdfDocument(pdfReader);

                var text = new System.Text.StringBuilder();
                int successPages = 0;
                int failedPages = 0;

                // Trích xuất văn bản từ tất cả các trang với xử lý lỗi
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    try
                    {
                        var page = pdfDoc.GetPage(i);
                        var pageText = PdfTextExtractor.GetTextFromPage(page);

                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            text.AppendLine(pageText);
                            text.AppendLine(); // Dòng trống giữa các trang
                            successPages++;
                        }
                    }
                    catch (Exception pageEx)
                    {
                        failedPages++;
                        _logger.LogWarning(pageEx, "PDF page {PageNumber} extraction failed", i);
                        // Bỏ qua trang có vấn đề, tiếp tục với các trang khác
                        continue;
                    }
                }

                var extractedText = text.ToString();

                // Thêm khoảng trắng giữa chữ thường và chữ hoa
                // "networkadministrator" → "network administrator"
                extractedText = Regex.Replace(extractedText, @"([a-z])([A-Z])", "$1 $2");

                // Thêm dấu xuống dòng sau dấu chấm trước chữ cái viết hoa
                // "server.What" → "server.\nWhat"
                extractedText = Regex.Replace(extractedText, @"\.([A-Z])", ".\n$1");

                // Thêm khoảng trắng sau dấu phẩy nếu thiếu
                extractedText = Regex.Replace(extractedText, @",([^\s])", ", $1");

                // Thêm dấu xuống dòng sau dấu hỏi/chấm than trước chữ cái viết hoa
                extractedText = Regex.Replace(extractedText, @"([?!])([A-Z])", "$1\n$2");

                // Thêm khoảng trắng chuẩn hóa nhiều khoảng trắng liên tiếp
                extractedText = Regex.Replace(extractedText, @"[ ]{2,}", " ");

                // Thêm dấu xuống dòng chuẩn hóa nhiều dấu xuống dòng liên tiếp
                extractedText = Regex.Replace(extractedText, @"[\r\n]{3,}", "\n\n");

                _logger.LogInformation("PDF extraction: {Length} chars from {Success}/{Total} pages{Failed}",
                    extractedText.Length, successPages, pdfDoc.GetNumberOfPages(),
                    failedPages > 0 ? $" ({failedPages} failed)" : "");

                if (extractedText.Length == 0)
                {
                    throw new InvalidOperationException(
                        "PDF không chứa text có thể đọc được. " +
                        "Có thể đây là PDF scan/ảnh. Hãy thử: " +
                        "1) Sử dụng OCR để convert PDF, " +
                        "2) Copy text từ PDF và paste vào file .txt, " +
                        "3) Convert PDF sang .docx"
                    );
                }

                return Task.FromResult(extractedText.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF extraction failed");
                throw new InvalidOperationException(
                    $"Không thể đọc file PDF: {ex.Message}. " +
                    "Giải pháp: Convert PDF sang .txt hoặc .docx rồi thử lại.",
                    ex
                );
            }
        }

        private async Task<string> ExtractTextFromTxtAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            // Kiểm tra encoding của file txt
            // Thử đọc một vài byte đầu để xác định BOM
            var bufferSize = (int)Math.Min(4, stream.Length);
            var buffer = new byte[bufferSize];
            _ = await stream.ReadAsync(buffer.AsMemory(0, bufferSize));
            stream.Position = 0; // Reset stream position
            
            System.Text.Encoding encoding;
            
            // Kiểm tra BOM (Byte Order Mark) để xác định encoding
            if (buffer.Length >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                encoding = System.Text.Encoding.UTF8;
            }
            else if (buffer.Length >= 2 && buffer[0] == 0xFF && buffer[1] == 0xFE)
            {
                encoding = System.Text.Encoding.Unicode; // UTF-16 LE
            }
            else if (buffer.Length >= 2 && buffer[0] == 0xFE && buffer[1] == 0xFF)
            {
                encoding = System.Text.Encoding.BigEndianUnicode; // UTF-16 BE
            }
            else
            {
                // Mặc định sử dụng UTF-8 cho các file không có BOM
                encoding = System.Text.Encoding.UTF8;
            }
            
            using var reader = new StreamReader(stream, encoding);
            return await reader.ReadToEndAsync();
        }

        // Khi không thể sử dụng AI, dùng regex để tìm kiếm các câu hỏi
        private List<ExtractedQuestionDto> ParseQuestionsFromText(string text)
        {
            var questions = new List<ExtractedQuestionDto>();

            // Pattern: Câu 1: ... hoặc 1. ... hoặc Question 1: ...
            var questionPattern = @"(?:Câu|Question|Bài)\s*(\d+)[:\.\)]?\s*(.+?)(?=(?:Câu|Question|Bài)\s*\d+[:\.\)]?|$)";
            var questionMatches = Regex.Matches(text, questionPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in questionMatches)
            {
                try
                {
                    var questionText = match.Groups[2].Value.Trim();
                    var question = ParseSingleQuestion(questionText);

                    if (question != null && question.CacLuaChon.Count >= 2)
                    {
                        questions.Add(question);
                    }
                }
                catch
                {
                    // Bỏ qua câu hỏi nếu có lỗi
                    continue;
                }
            }

            return questions;
        }

        private ExtractedQuestionDto? ParseSingleQuestion(string questionText)
        {
            // Phân chia đáp án và câu hỏi dựa trên dòng mới
            var lines = questionText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
        .Where(l => !string.IsNullOrWhiteSpace(l))
           .ToList();

            if (lines.Count < 3) // Cần ít nhất: câu hỏi + 2 đáp án
                return null;

            var question = new ExtractedQuestionDto();
            var choices = new List<string>();
            int correctIndex = -1;

            // Dòng đầu tiên là câu hỏi
            question.NoiDungCauHoi = lines[0];

            // Hỗ trợ cả chữ cái và số
            var choicePattern = @"^[A-Da-d\d][\.\)]\s*(.+)$";
            // Dấu hiệu đáp án đúng có thể nằm trong hoặc ngoài phần nội dung lựa chọn
            var correctPattern = @"\*|✓|✔|correct|đúng";

            for (int i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                var match = Regex.Match(line, choicePattern);

                if (match.Success)
                {
                    var choiceText = match.Groups[1].Value.Trim();

                    // Kiểm tra dấu hiệu đáp án đúng
                    if (Regex.IsMatch(line, correctPattern, RegexOptions.IgnoreCase))
                    {
                        correctIndex = choices.Count;
                        choiceText = Regex.Replace(choiceText, correctPattern, "").Trim();
                    }

                    choices.Add(choiceText);

                    if (choices.Count >= 4)
                        break;
                }
            }

            if (choices.Count < 2)
                return null;

            question.CacLuaChon = choices;
            question.DapAnDung = correctIndex >= 0 ? correctIndex : 0;

            return question;
        }
    }
}
