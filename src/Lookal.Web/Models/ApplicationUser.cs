using Microsoft.AspNetCore.Identity;

namespace Lookal.Web.Models;

public enum VerificationStatus { None, Pending, Verified, Rejected }

public enum AccountStatus { Active, Suspended }

// Id, UserName, PhoneNumber, and PasswordHash come from IdentityUser<Guid>.
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.None;
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    public Guid? CommunityId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}