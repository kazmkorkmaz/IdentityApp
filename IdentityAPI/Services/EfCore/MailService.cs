using System.Net;
using System.Net.Mail;
using IdentityAPI.Services.Abstract;

namespace IdentityAPI.Services.EfCore
{
    public class MailService : IEmailService
    {
        private string? _host;
        private int _port;
        private bool _enableSSL;
        private string? _username;
        private string? _password;
        public MailService(string? host, int port, bool enableSSL, string? username, string? password)
        {
            _host = host;
            _port = port;
            _enableSSL = enableSSL;
            _username = username;
            _password = password;
        }
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = _enableSSL
            };

            return client.SendMailAsync(new MailMessage(_username ?? "", email, subject, message) { IsBodyHtml = true });
        }
    }
}