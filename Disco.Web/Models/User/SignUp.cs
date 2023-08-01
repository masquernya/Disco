using System.Diagnostics.CodeAnalysis;

namespace Disco.Web.Models.User;

public class SignUpResponse
{
    public long id { get; set; }
    public string username { get; set; }
}

public class SignUpRequest
{
    /// <summary>
    /// Desired username
    /// </summary>
    public string username { get; set; }
    /// <summary>
    /// Desired account password
    /// </summary>
    public string password { get; set; }
    /// <summary>
    /// The completed captcha
    /// </summary>
    public string? captcha { get; set; }
}