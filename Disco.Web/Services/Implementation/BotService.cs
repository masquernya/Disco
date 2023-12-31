namespace Disco.Web.Services;

public class BotService : IBotService, IDiscoService
{
    private readonly string _authorizationKey;

    public BotService(string authorizationKey)
    {
        this._authorizationKey = authorizationKey;
    }
    
    public string GetAuthorizationKey()
    {
        return _authorizationKey;
    }
}