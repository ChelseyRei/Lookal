using Lookal.Web.Models;
using Lookal.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lookal.Web.Services;

// Details a visitor has entered but not yet verified. Kept in TempData as JSON until the code is confirmed.
// The password is stored only as a hash.
public sealed record PendingRegistration(
    string FullName,
    DateOnly DateOfBirth,
    string PhoneNumber,
    string PasswordHash);

// Result of the checks before the OTP step. ErrorMessage is null when the details pass.
public sealed record RegistrationStart(
    PendingRegistration? Pending,
    string? ErrorField = null,
    string? ErrorMessage = null);

public sealed class AccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IOtpService _otp;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IOtpService otp)
    {
        _userManager = userManager;
        _passwordHasher = passwordHasher;
        _otp = otp;
    }

    // Registration steps 2 to 3: format check, duplicate check, then send the code.
    // Nothing is written to the database here.
    public async Task<RegistrationStart> StartRegistrationAsync(RegisterViewModel model)
    {
        // E-102: the number must be a valid Philippine mobile number.
        var phone = NormalizePhone(model.PhoneNumber);
        if (phone is null)
            return new RegistrationStart(null, nameof(RegisterViewModel.PhoneNumber), ErrorMessages.E102);

        // E-103: one account per mobile number. Checked before any code is sent.
        var alreadyRegistered = await _userManager.Users.AnyAsync(u => u.PhoneNumber == phone);
        if (alreadyRegistered)
            return new RegistrationStart(null, nameof(RegisterViewModel.PhoneNumber), ErrorMessages.E103);

        // Hash the password now, so the plain text is never stored, even temporarily.
        var passwordHash = _passwordHasher.HashPassword(new ApplicationUser(), model.Password);

        var pending = new PendingRegistration(
            model.FullName.Trim(),
            model.DateOfBirth!.Value,
            phone,
            passwordHash);

        await _otp.SendCodeAsync(phone);
        return new RegistrationStart(pending);
    }

    // Registration step 7: runs after the code is verified.
    public async Task<(ApplicationUser User, IdentityResult Result)> CreateAccountAsync(PendingRegistration pending)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = pending.PhoneNumber,
            PhoneNumber = pending.PhoneNumber,
            PhoneNumberConfirmed = true,   // confirmed by the OTP step
            FullName = pending.FullName,
            DateOfBirth = pending.DateOfBirth,
            PasswordHash = pending.PasswordHash,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user);
        return (user, result);
    }

    // E-102 format rule. Accepts 09XX XXX XXXX or +639XX XXX XXXX, with spaces or dashes allowed.
    // Returns the stored form (+639XXXXXXXXX), or null if the input is not valid.
    public static string? NormalizePhone(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var text = raw.Trim();
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var allowed = IsAsciiDigit(c) || c == ' ' || c == '-' || (c == '+' && i == 0);
            if (!allowed)
                return null;
        }

        var digits = new string(text.Where(IsAsciiDigit).ToArray());

        if (digits.Length == 11 && digits.StartsWith("09", StringComparison.Ordinal))
            return "+63" + digits[1..];

        if (digits.Length == 12 && digits.StartsWith("639", StringComparison.Ordinal))
            return "+" + digits;

        return null;
    }

    private static bool IsAsciiDigit(char c) => c is >= '0' and <= '9';
}