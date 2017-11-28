using System.Threading.Tasks;

namespace ChannelX.Email
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}