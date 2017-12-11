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
using System.Net.Http;

namespace ChannelX.Email
{
public class AuthMessageSender : IEmailSender
    {
        public IHostingEnvironment _env;
        private static HttpClient client; //singleton
        public AuthMessageSender(IOptions<EmailSettings> emailSettings, IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
            client = new HttpClient();
            if(emailSettings.Value.Key != null)
            {
                var val = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "api", emailSettings.Value.Key)));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", val);
            }
        }

        public EmailSettings _emailSettings { get; }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute (email, subject, message);
        }
        private async Task Execute(string email, string subject, string message)
        {

            if(_emailSettings.Key == null)
                return;
            // Construct the message body
            var finalized_message = "";
            // Add upper part of the mail
            finalized_message+= System.IO.File.ReadAllText(Path.Combine(_env.ContentRootPath, "wwwroot", "email_templates", "heml_upper.txt"));
            // Add message body part of the mail 
            finalized_message+= message;
            // Add lower part of the mail
            finalized_message+= System.IO.File.ReadAllText(Path.Combine(_env.ContentRootPath, "wwwroot", "email_templates", "heml_lower.txt"));


            Dictionary<string, string> values = new Dictionary<string, string>();

            values.Add("from", $"ChannelX Staff <{_emailSettings.From}>");
            values.Add("to", email);
            values.Add("subject",  "ChannelX Mail System - " + subject);
            values.Add("text", finalized_message);
            values.Add("html", finalized_message);

            FormUrlEncodedContent content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://api.mailgun.net/v3/mubis.net/messages", content);

            string responseString = string.Empty;

            if(response.StatusCode == HttpStatusCode.OK)
                responseString = await response.Content.ReadAsStringAsync();

        }        


        Task IEmailSender.SendEmailAsync(string email, string subject, string message)
        {
            return SendEmailAsync(email, subject, message);
        }
    }
}