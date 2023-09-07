namespace Disco.Web.Models.Bot;

public class AddOrUpdateSpaceRequest
{
    public string? name { get; set; }
    public string? description { get; set; }
    public int memberCount { get; set; }
    public string? invite { get; set; }
    /// <summary>
    /// mxc:// url
    /// </summary>
    public string? avatar { get; set; }
    /// <summary>
    /// Array of matrix user IDs with ban permission
    /// </summary>
    public string[]? admins { get; set; }
}