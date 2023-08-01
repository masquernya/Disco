namespace Disco.Web.Services;

public interface ICaptchaService
{
    Task<bool> IsValid(string captchaResponse);
}