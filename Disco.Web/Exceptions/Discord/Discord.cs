using System.Net;
using System.Net.Http.Headers;

namespace Disco.Web.Exceptions.Discord;

public class DiscordException : BaseWebException
{
    public DiscordException(HttpStatusCode code, string message) : base(code, message)
    {
        
    }
}

public class IdOutOfRangeException : DiscordException
{
    public IdOutOfRangeException() : base(HttpStatusCode.BadRequest, "ID out of range")
    {
        
    }
}

public class MissingTagException : DiscordException
{
    public MissingTagException() : base(HttpStatusCode.BadRequest, "Missing tag in discord name")
    {
        
    }
}

public class BadLengthAfterTagSplit : DiscordException
{
    public BadLengthAfterTagSplit() : base(HttpStatusCode.BadRequest, "Bad length after discord tag name split")
    {
        
    }
}

public class MalformedNameException : DiscordException
{
    public MalformedNameException() : base(HttpStatusCode.BadRequest, "Discord name is malformed")
    {
        
    }
}

public class DiscordApiException : Exception
{
    private HttpStatusCode code { get; set; }
    private string content { get; }
    private HttpHeaders headers { get; }

    public DiscordApiException(HttpStatusCode code, HttpHeaders headers, string content, string message) : base(message)
    {
        // these are for tracking if we decide to add sentry (or something similar - e.g. logging)
        this.code = code;
        this.headers = headers;
        this.content = content;
    }
}

public class DiscordApiBadStatusException : DiscordApiException
{
    public DiscordApiBadStatusException(HttpStatusCode code, string content, HttpHeaders headers) : base(code, headers, content, "Bad status code from discord API: " + code)
    {
        
    }
}

public class DiscordApiBadBodyException : DiscordApiException
{
    public DiscordApiBadBodyException(string reason, HttpStatusCode code, string content, HttpHeaders headers) : base(code, headers, content, "Bad body from discord API: " + reason)
    {
        
    }
}
