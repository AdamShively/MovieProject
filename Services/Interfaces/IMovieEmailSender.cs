using Microsoft.AspNetCore.Identity.UI.Services;

namespace MovieProject.Services
{
    public interface IMovieEmailSender : IEmailSender
    {
        Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage);
    }
}
