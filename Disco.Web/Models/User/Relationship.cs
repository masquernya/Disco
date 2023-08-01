using Disco.Web.Data;

namespace Disco.Web.Models.User;

public class UpdateRelationshipRequest
{
    public long targetAccountId { get; set; }
    public RelationStatus status { get; set; }
}

public class AccountRelationshipUpdateResponse
{
    public RelationStatus status { get; set; }
    public bool isMutualLike { get; set; }
}