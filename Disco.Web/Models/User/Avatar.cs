using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class SetAvatarRequest
{
    public AvatarSource? source { get; set; }
}