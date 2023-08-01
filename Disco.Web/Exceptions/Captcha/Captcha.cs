using System.Net;

namespace Disco.Web.Exceptions.Captcha;

public class CaptchaFailedException : BaseWebException
{
    public CaptchaFailedException() : base(HttpStatusCode.BadRequest, "Captcha verification failed")
    {
        
    }
}