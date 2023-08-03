using System.Text.Json.Serialization;

namespace Disco.Web.Models.User;

public class MatrixUserInfoResponse
{
    [JsonPropertyName("displayname")]
    public string displayName { get; set; }
    [JsonPropertyName("avatar_url")]
    public string avatarUrl { get; set; }
}

public class SetMatrixAccountResponse
{
    public string username { get; set; }
}