namespace Disco.Web.Services;

public class HttpRequestService : IHttpRequestService, IDiscoService, ITransientDiscoService<IHttpRequestService>
{
    private readonly IHttpContextAccessor _context;
    public HttpRequestService(IHttpContextAccessor context)
    {
        this._context = context;
    }
    
    public string? GetRequestHeader(string key)
    {
        return _context.HttpContext?.Request.Headers[key];
    }
}