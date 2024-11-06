using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NotificationService.Model;
using System.Threading.Tasks;

namespace NotificationService.Service
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(Notification notification)
        {
            var email = new MimeMessage();

            try
            {
                email.From.Add(MailboxAddress.Parse(_configuration["Email:From"]));
                email.To.Add(MailboxAddress.Parse(notification.To));
                email.Subject = notification.Subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = notification.Message
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();

                // Подключаемся к SMTP-серверу
                await smtp.ConnectAsync(_configuration["Email:SmtpServer"], int.Parse(_configuration["Email:Port"]), SecureSocketOptions.SslOnConnect);

                // Аутентифицируемся
                await smtp.AuthenticateAsync(_configuration["Email:Username"], _configuration["Email:Password"]);

                // Отправляем письмо
                await smtp.SendAsync(email);

                Console.WriteLine("Email sent successfully!");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Invalid email format: {ex.Message}");
            }
            catch (SmtpCommandException ex)
            {
                Console.WriteLine($"SMTP Command Error: {ex.Message} (StatusCode: {ex.StatusCode})");
            }
            catch (SmtpProtocolException ex)
            {
                Console.WriteLine($"SMTP Protocol Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending email: {ex.Message}");
            }
            finally
            {
                // Попробуем отключиться от SMTP-сервера
                try
                {
                    using var smtp = new SmtpClient();
                    await smtp.DisconnectAsync(true);
                }
                catch
                {
                    Console.WriteLine("Failed to disconnect from SMTP server.");
                }
            }
        }
    }
}
