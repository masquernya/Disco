namespace Disco.Web.Services;

public interface IHttpRequestService
{
    string? GetRequestHeader(string key);
}