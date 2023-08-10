using System.Net;

namespace Disco.Web.Exceptions.User;

public class UserNotFoundException : BaseWebException
{
    public UserNotFoundException() : base(HttpStatusCode.NotFound, "User does not exist")
    {
        
    }
}

public class InvalidUsernameOrPasswordException : BaseWebException
{
    public InvalidUsernameOrPasswordException() : base(HttpStatusCode.Unauthorized, "Username or password is invalid")
    {
        
    }
}

public class UnauthorizedException : BaseWebException
{
    public UnauthorizedException() : base(HttpStatusCode.Unauthorized, "Must be logged in to access this resource")
    {

    }
}

public class TagNotFoundException : BaseWebException
{
    public TagNotFoundException() : base(HttpStatusCode.BadRequest, "Tag not found")
    {
        
    }
}

public class InvalidTagException : BaseWebException
{
    public InvalidTagException() : base(HttpStatusCode.BadRequest, "Invalid tag format")
    {
        
    }
}

public class TooManyTagsException : BaseWebException
{
    public TooManyTagsException() : base(HttpStatusCode.BadRequest, "Too many tags")
    {
        
    }
}

public class InvalidDisplayNameException : BaseWebException
{
    public InvalidDisplayNameException() : base(HttpStatusCode.BadRequest, "Invalid display name")
    {
        
    }
}

public class InvalidUsernameException : BaseWebException
{
    public InvalidUsernameException() : base(HttpStatusCode.BadRequest, "Invalid username")
    {
        
    }
}

public class UsernameTakenException : BaseWebException
{
    public UsernameTakenException() : base(HttpStatusCode.Conflict, "Username is taken")
    {
        
    }
}

public class InvalidAvatarSourceException : BaseWebException
{
    public InvalidAvatarSourceException() : base(HttpStatusCode.BadRequest, "Invalid avatar source") {}
}

public class AvatarNotFoundException : BaseWebException
{
    public AvatarNotFoundException() : base(HttpStatusCode.BadRequest, "Avatar not found or does not exist") {}
}

public class InvalidGenderLengthException : BaseWebException
{
    public InvalidGenderLengthException() : base(HttpStatusCode.BadRequest, "Invalid gender length")
    {
        
    }
}

public class InvalidAgeException : BaseWebException
{
    public InvalidAgeException() : base(HttpStatusCode.BadRequest, "Invalid age")
    {
        
    }
}

public class InvalidPronounsException : BaseWebException
{
    public InvalidPronounsException() : base(HttpStatusCode.BadRequest, "Invalid pronouns")
    {
        
    }
}

public class InvalidRelationStatusException : BaseWebException
{
    public InvalidRelationStatusException() : base(HttpStatusCode.BadRequest, "Invalid relation status")
    {
        
    }
}

public class AccountBannedException : BaseWebException
{
    public AccountBannedException(string reason) : base(HttpStatusCode.BadRequest, "Account has been banned. Reason: " + reason)
    {
        
    }
}

public class InvalidPasswordResetTokenException : BaseWebException
{
    public InvalidPasswordResetTokenException() : base(HttpStatusCode.BadRequest, "Invalid password reset token")
    {
        
    }
}