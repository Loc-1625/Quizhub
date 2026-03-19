namespace QuizHub.Services.Interfaces
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyTokenAsync(
            string? token,
            string? remoteIp,
            string? expectedAction = null,
            CancellationToken cancellationToken = default);
    }
}