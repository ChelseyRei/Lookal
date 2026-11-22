using System.Text.Json;
using Lookal.Web.Models;
using Lookal.Web.Services;
using Lookal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Lookal.Web.Controllers;

public class AccountController : Controller
{
    private const string PendingKey = "PendingRegistration";

    private readonly AccountService _accounts;
    private readonly IOtpService _otp;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        AccountService accounts,
        IOtpService otp,
        SignInManager<ApplicationUser> signInManager)
    {
        _accounts = accounts;
        _otp = otp;
        _signInManager = signInManager;
    }

    // ---------- Registration ----------

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return RedisplayRegister(model);

        var start = await _accounts.StartRegistrationAsync(model);
        if (start.ErrorMessage is not null)
        {
            ModelState.AddModelError(start.ErrorField ?? string.Empty, start.ErrorMessage);
            return RedisplayRegister(model);
        }

        // Nothing is saved to the database yet. The pending details wait in TempData for the code.
        TempData[PendingKey] = JsonSerializer.Serialize(start.Pending);
        return RedirectToAction(nameof(VerifyOtp));
    }

    // ---------- OTP verification ----------

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        if (PeekPending() is null)
            return RedirectToAction(nameof(Register));

        return View(new VerifyOtpViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        var pending = PeekPending();
        if (pending is null)
            return RedirectToAction(nameof(Register));

        if (!ModelState.IsValid)
            return View(model);

        // E-104: wrong or expired code. The pending details stay, so the user can try again.
        if (!await _otp.VerifyCodeAsync(pending.PhoneNumber, model.Code))
        {
            ModelState.AddModelError(nameof(VerifyOtpViewModel.Code), ErrorMessages.E104);
            return View(model);
        }

        var (user, result) = await _accounts.CreateAccountAsync(pending);
        if (!result.Succeeded)
        {
            // Another request can take the same number between the check and this save.
            var isDuplicate = result.Errors.Any(e => e.Code == "DuplicateUserName");
            ModelState.AddModelError(string.Empty,
                isDuplicate ? ErrorMessages.E103 : "We couldn't create your account. Try again.");
            return View(model);
        }

        TempData.Remove(PendingKey);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(JoinCommunity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCode()
    {
        var pending = PeekPending();
        if (pending is null)
            return RedirectToAction(nameof(Register));

        await _otp.SendCodeAsync(pending.PhoneNumber);
        TempData["Status"] = "We sent you a new code.";
        return RedirectToAction(nameof(VerifyOtp));
    }

    // ---------- Login and logout ----------

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return RedisplayLogin(model);

        var phone = AccountService.NormalizePhone(model.PhoneNumber);
        if (phone is null)
        {
            ModelState.AddModelError(string.Empty, ErrorMessages.LoginFailed);
            return RedisplayLogin(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            phone, model.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
            return RedirectToAction("Index", "Home");

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Too many failed attempts. Wait a few minutes and try again."
            : ErrorMessages.LoginFailed);

        return RedisplayLogin(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    // ---------- Pages after login ----------

    [Authorize]
    public IActionResult JoinCommunity() => View();
    
    [Authorize]
    public IActionResult VerifyId() => View();

    [Authorize]
    public IActionResult Profile()
    {
        ViewData["NavActive"] = "profile";
        return View();
    }

    // ---------- Helpers ----------

    // Reads the pending registration without removing it, so it survives the next request.
    private PendingRegistration? PeekPending()
    {
        TempData.Keep(PendingKey);
        return TempData.Peek(PendingKey) is string json
            ? JsonSerializer.Deserialize<PendingRegistration>(json)
            : null;
    }

    // Clear the password before redisplaying a form, so it isn't echoed back into the page.
    private IActionResult RedisplayRegister(RegisterViewModel model)
    {
        model.Password = string.Empty;
        return View("Register", model);
    }

    private IActionResult RedisplayLogin(LoginViewModel model)
    {
        model.Password = string.Empty;
        return View("Login", model);
    }
}