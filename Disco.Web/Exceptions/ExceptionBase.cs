using System.Net;

namespace Disco.Web.Exceptions;

public class BaseWebException : System.Exception
{
    public HttpStatusCode status { get; set; } = HttpStatusCode.InternalServerError;
    public string message { get; set; }
    public string code => this.GetType().Name;

    public BaseWebException(HttpStatusCode code, string message)
    {
        this.status = code;
        this.message = message;
    }
}