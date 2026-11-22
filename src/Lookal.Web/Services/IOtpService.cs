namespace Lookal.Web.Services;

public interface IOtpService
{
    Task SendCodeAsync(string phoneNumber);
    Task<bool> VerifyCodeAsync(string phoneNumber, string code);
}