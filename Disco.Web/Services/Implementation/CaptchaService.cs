using System.Net;
using System.Text.Json;

namespace Disco.Web.Services;

internal class HCaptchaJsonResponse
{
    public bool success { get; set; }
}

public class CaptchaService : ICaptchaService
{
    private ILogger logger { get; }

    public CaptchaService(ILogger logger)
    {
        this.logger = logger;
    }
    
    private HttpClient client { get; } = new();
    public async Task<bool> IsValid(string captchaResponse)
    {
        var body = new FormUrlEncodedContent(new Dictionary<string,string>
        {
            {"response", captchaResponse},
            {"secret", Config.hcaptchaPrivate},
            {"sitekey", Config.hcatpchaPublic},
        });
        
        try
        {
            var result = await client.PostAsync("https://hcaptcha.com/siteverify", body);
            var str = await result.Content.ReadAsStringAsync();
            if (result.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Failed to verify captcha. token={token} status={status} response={response}", captchaResponse, result.StatusCode, str);
                return false;
            }

            var decoded = JsonSerializer.Deserialize<HCaptchaJsonResponse>(str);
            if (decoded is not {success: true})
            {
                logger.LogError("Failed to verify captcha. token={token} response={response}", captchaResponse, str);
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to verify captcha. token={token}", captchaResponse);
            return false;
        }
    }
}