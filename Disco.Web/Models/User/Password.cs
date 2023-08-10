namespace Disco.Web.Models.User;

public class UpdatePasswordRequest
{
    public string newPassword { get; set; }
    public string originalPassword { get; set; }
}

public class ResetPasswordRequest
{
    public string username { get; set; }
}

public class ResetPasswordWithMatrixRequest : ResetPasswordRequest
{
    public string matrixUserId { get; set; }
}

public class ResetPasswordResponse
{
    public string token { get; set; }
}

public class ResetPasswordSubmitRequest
{
    public string token { get; set; }
    public string newPassword { get; set; }
}