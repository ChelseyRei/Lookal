namespace Lookal.Web.Services;

public static class ErrorMessages
{
    public const string E101 = "Please fill in all required fields.";
    public const string E102 = "Please enter a valid mobile number.";
    public const string E103 = "This number is already registered. Try logging in.";
    public const string E104 = "The code is incorrect or has expired. Request a new one.";
    public const string E105 = "You must be 18 or older to use this app.";

    public const string LoginFailed = "The mobile number or password is incorrect.";

    /*
        OTHER  ERRORS

        public const string E-201 = “Please choose a deadline at least 30 minutes from now.”;
    
        public const string E-202 = “The minimum fee is ₱20.”;

        public const string E-203 = “This item cannot be requested on Lookal.”;


        public const string E-301 = “Verify your ID to accept requests.”;

        public const string E-302 = “Finish a current task before accepting another.”;

        public const string E-303 = “This request is no longer available.”;


        public const string E-401 = “Please upload a JPG or PNG under 5 MB.”;
        

        public const string E-501 = “Your account is suspended. Contact support.”;
    */
}