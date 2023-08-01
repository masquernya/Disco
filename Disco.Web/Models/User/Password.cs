namespace Disco.Web.Models.User;

public class UpdatePasswordRequest
{
    public string newPassword { get; set; }
    public string originalPassword { get; set; }
}