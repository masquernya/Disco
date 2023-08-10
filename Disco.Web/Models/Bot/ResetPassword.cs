namespace Disco.Web.Models.Bot;

public class ResetPasswordVerification
{
    public string? token { get; set; }
    public string? userId { get; set; }
    public string? imageUrl { get; set; }
}

public class ResetPasswordVerificationResponse
{
    public string redirectUrl { get; set; }
}