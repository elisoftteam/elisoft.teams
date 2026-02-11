namespace Elisoft.Teams.Services
{
    public interface ITeamsNotificator
    {
        Task<bool> SendMessageAsync(string webhookUrl, string messageText);
    }
}
