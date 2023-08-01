using System.Text.Json.Serialization;

namespace Disco.Web.Models.User;

public class LinkDiscordUrlResponse
{
    public string redirectUrl { get; set; }
}

public class DiscordOauthResponse
{
    [JsonPropertyName("access_token")]
    public string accessToken { get; set; }
}

public class DiscordStateResponse
{
    public string redirectUrl { get; set; }
    public string state { get; set; }
}

public class DiscordUser
{
    public string id { get; set; }
    public string username { get; set; }
    public string discriminator { get; set; }
    public string? avatar { get; set; }
    public bool bot { get; set; }
}