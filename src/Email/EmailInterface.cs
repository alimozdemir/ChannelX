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
                Console.WriteLine("Warning: emailsetting.json is not read! You need a "
                   + "working SMTP information.");
                return;
            }

            MailMessage mail = new MailMessage()
            {
                From = new MailAddress(_emailSettings.UsernameEmail, "ChannelX Staff")
            };

            mail.To.Add(new MailAddress(toEmail));

            Console.WriteLine("keeeeeeeeeeeeeeeeeeek1");

            // mail.CC.Add(new MailAddressCollection(_emailSettings.CcEmail));

            mail.Subject = "ChannelX Mail System - " + subject;

            // Construct the message body
            var finalized_message = "";
            // Add upper part of the mail
            finalized_message+= System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\heml_upper.txt");
            // Add message body part of the mail 
            finalized_message+= message;
            // Add lower part of the mail
            finalized_message+= System.IO.File.ReadAllText(_env.ContentRootPath + "\\Email\\heml_lower.txt");

            mail.Body = finalized_message;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;

            using (SmtpClient smtp = new SmtpClient(_emailSettings.SecondayDomain, _emailSettings.SecondaryPort))
            {
                smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
                Console.WriteLine("keeeeeeeeeeeeeeeeeeek2");
            }            

             //do something here
            Console.WriteLine("HAAAAAAAAAAAAAAA");

          
        }

        Task IEmailSender.SendEmailAsync(string email, string subject, string message)
        {
            return SendEmailAsync(email, subject, message);
        }
    }
}