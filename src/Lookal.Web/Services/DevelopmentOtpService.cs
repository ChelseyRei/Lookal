using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lookal.Web.Services;

// TODO: replace with Firebase Authentication before release.
// This stub sends no SMS. It accepts one fixed test code, and only in the Development environment.
// Outside Development, every code is rejected.
public sealed class DevelopmentOtpService : IOtpService
{
    private const string TestCode = "123456";

    private readonly IHostEnvironment _environment;
    private readonly ILogger<DevelopmentOtpService> _logger;

    public DevelopmentOtpService(IHostEnvironment environment, ILogger<DevelopmentOtpService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public Task SendCodeAsync(string phoneNumber)
    {
        if (_environment.IsDevelopment())
            _logger.LogInformation("Development OTP stub: use code {Code}.", TestCode);

        return Task.CompletedTask;
    }

    public Task<bool> VerifyCodeAsync(string phoneNumber, string code)
    {
        var isValid = _environment.IsDevelopment() && string.Equals(code, TestCode, StringComparison.Ordinal);
        return Task.FromResult(isValid);
    }
}