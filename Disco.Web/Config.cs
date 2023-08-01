namespace Disco.Web;

public static class Config
{
    public static string frontendUrl { get; set; }
    public static long adminUserId { get; set; }
    public static string hcaptchaPrivate { get; set; }
    public static string hcatpchaPublic { get; set; }

    public static bool IsAdmin(long accountId)
    {
        return adminUserId == accountId;
    }
}