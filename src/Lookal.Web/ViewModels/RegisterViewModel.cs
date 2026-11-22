using System.ComponentModel.DataAnnotations;
using Lookal.Web.Services;

namespace Lookal.Web.ViewModels;

public class RegisterViewModel : IValidatableObject
{
    private static readonly TimeZoneInfo ManilaZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

    [Required(ErrorMessage = ErrorMessages.E101)]
    [StringLength(100, ErrorMessage = "Your name can be up to 100 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = ErrorMessages.E101)]
    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = ErrorMessages.E101)]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = "Mobile number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = ErrorMessages.E101)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Your password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$",
        ErrorMessage = "Your password needs at least one letter and one number.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    // E-105: the applicant must be 18 or older, judged by Philippine time.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth is null)
            yield break;

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ManilaZone));
        if (DateOfBirth.Value.AddYears(18) > today)
            yield return new ValidationResult(ErrorMessages.E105, new[] { nameof(DateOfBirth) });
    }
}