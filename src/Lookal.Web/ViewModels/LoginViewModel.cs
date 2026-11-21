using System.ComponentModel.DataAnnotations;
using Lookal.Web.Services;

namespace Lookal.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = ErrorMessages.E101)]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Mobile number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = ErrorMessages.E101)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}