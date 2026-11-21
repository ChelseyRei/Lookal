using System.ComponentModel.DataAnnotations;
using Lookal.Web.Services;

namespace Lookal.Web.ViewModels;

public class VerifyOtpViewModel
{
    [Required(ErrorMessage = ErrorMessages.E101)]
    [RegularExpression(@"^\d{6}$", ErrorMessage = ErrorMessages.E104)]
    [Display(Name = "Verification code")]
    public string Code { get; set; } = string.Empty;
}