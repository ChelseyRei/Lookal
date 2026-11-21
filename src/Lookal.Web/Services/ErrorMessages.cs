namespace Lookal.Web.Services;

// Exact wording from the proposal (Section VIII).
public static class ErrorMessages
{
    public const string E101 = "Please fill in all required fields.";
    public const string E102 = "Please enter a valid mobile number.";
    public const string E103 = "This number is already registered. Try logging in.";
    public const string E104 = "The code is incorrect or has expired. Request a new one.";
    public const string E105 = "You must be 18 or older to use this app.";

    // One generic message, so login doesn't reveal which part was wrong.
    public const string LoginFailed = "The mobile number or password is incorrect.";
}