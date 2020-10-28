using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using WebApi.Models;
using Microsoft.Extensions.Options;

namespace WebApi.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(IList<string> email, IList<string> cc, string subject, string htmlMessage, IList<Attachment> attachments);
        Task SendEmailAsync(IList<string> email, IList<string> cc, string subject, string htmlMessage, AlternateView altview );
        Task SendEmailAsync(IList<string> email, IList<string> cc, string subject, string message);
    }
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public Task SendEmailAsync(IList<string> email, IList<string> cc, string subject, string message, IList<Attachment> attachments)
        {
            try
            {
                // Credentials
                var credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password);

                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

           
                if (email != null)
                {
                    foreach (string stremail in email)
                    {
                        mail.To.Add(new MailAddress(stremail));
                    }

                }

                if (cc != null)
                {
                    foreach (string strcc in cc)
                    {
                        mail.CC.Add(new MailAddress(strcc));
                    }

                }
                if (attachments != null)
                {
                    foreach (Attachment attfile in attachments)
                    {
                        mail.Attachments.Add(attfile);
                    }
                }
                // Smtp client
                var client = new SmtpClient()
                {
                    Port = _emailSettings.MailPort,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = _emailSettings.MailServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);
            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }

            return Task.CompletedTask;
        }

        public Task SendEmailAsync(IList<string> email,IList<string> cc, string subject, string message, AlternateView alterView)
        {
            try
            {
                // Credentials
                var credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password);

                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,                    
                };
           
                if (email != null)
                {
                    foreach (string stemail in email)
                    {
                        mail.To.Add(new MailAddress(stemail));
                    }

                }

                if (cc != null)
                {
                    foreach (string strcc in cc)
                    {
                        mail.CC.Add(new MailAddress(strcc));
                    }
                   
                }
                mail.AlternateViews.Add(alterView);

                // Smtp client
                var client = new SmtpClient()
                {
                    Port = _emailSettings.MailPort,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = _emailSettings.MailServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);
            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }

            return Task.CompletedTask;
        }

        public Task SendEmailAsync(IList<string> email, IList<string> cc, string subject, string message)
        {
            try
            {
                // Credentials
                var credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password);

                // Mail message
                var mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                if (email != null)
                {
                    foreach (string stemail in email)
                    {
                        mail.To.Add(new MailAddress(stemail));
                    }

                }

                if (cc != null)
                {
                    foreach (string strcc in cc)
                    {
                        mail.CC.Add(new MailAddress(strcc));
                    }

                }

                // Smtp client
                var client = new SmtpClient()
                {
                    Port = _emailSettings.MailPort,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Host = _emailSettings.MailServer,
                    EnableSsl = true,
                    Credentials = credentials
                };

                // Send it...         
                client.Send(mail);
            }
            catch (Exception ex)
            {
                // TODO: handle exception
                throw new InvalidOperationException(ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
