using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using ChannelX.Models.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace ChannelX.Email
{
public class AuthMessageSender : IEmailSender
    {
        public IHostingEnvironment _env;
        public AuthMessageSender(IOptions<EmailSettings> emailSettings, IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public EmailSettings _emailSettings { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute (email, subject, message);
        }

        public async Task Execute(string email, string subject, string message)
        {
            string toEmail = string.IsNullOrEmpty(email) 
                             ? _emailSettings.ToEmail 
                             : email;

            // If emailsettings.json read failed, code should not process further
            if (_emailSettings.UsernameEmail == null)
            {
                System.Diagnostics.Debug.WriteLine("emailsettings.json is not loaded!");
                return;
            }

            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(_emailSettings.UsernameEmail, "ChannelX Staff")
            };

            mail.To.Add(new MailAddress(toEmail));
            mail.Subject = "ChannelX Mail System - " + subject;
            // Construct the message body
            var finalized_message = "";
            // Add upper part of the mail
            finalized_message+= System.IO.File.ReadAllText(Path.Combine(_env.ContentRootPath, "wwwroot", "email_templates", "heml_upper.txt"));
            // Add message body part of the mail 
            finalized_message+= message;
            // Add lower part of the mail
            finalized_message+= System.IO.File.ReadAllText(Path.Combine(_env.ContentRootPath, "wwwroot", "email_templates", "heml_lower.txt"));

            mail.Body = finalized_message;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            using (SmtpClient smtp = new SmtpClient(_emailSettings.SecondayDomain, _emailSettings.SecondaryPort))
            {
                smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
                System.Diagnostics.Debug.WriteLine("Sending Email.");
            }            
            System.Diagnostics.Debug.WriteLine("Process completed.");
        }

        Task IEmailSender.SendEmailAsync(string email, string subject, string message)
        {
            return SendEmailAsync(email, subject, message);
        }
    }
}