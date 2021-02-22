namespace Enjin.SDK.PusherClient
{
    public interface IAuthorizer
    {
        string Authorize(string channelName, string socketId);
    }
}