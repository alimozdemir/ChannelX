using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Web;

using Quartz;
using ChannelX.Email;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Hubs;
using ChannelX.Models.Chat;
using ChannelX.Models.Channel;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;

using Newtonsoft.Json;
using ChannelX.Models.Trackers;
using ChannelX.Redis;
using StackExchange.Redis;

public class SendBulkEmail : IJob
{
    readonly DatabaseContext _db;
    readonly IEmailSender _emailSender;
    readonly UserTracker _tracker;
    readonly RedisConnection _redis_db;
    readonly IRedisConnectionFactory _fact;
    readonly IHostingEnvironment _env;

    public SendBulkEmail(IEmailSender emailSender, DatabaseContext db, UserTracker tracker,  IRedisConnectionFactory fact, IHostingEnvironment env)
    {
        _db = db;
        _emailSender = emailSender;
        _tracker = tracker;
        _fact = fact;
        _env = env;
        _redis_db = fact.Connection();
        
    }
    public async Task Execute(IJobExecutionContext context)
    {
        System.Diagnostics.Debug.WriteLine("Trying to execute the job.");
        GenerateBulkEmailAsync();
    }

    public async Task GenerateBulkEmailAsync()
    {
        // var user = _tracker.Remove(Context.Connection);

        // Fetch all channels
        var channel_list = _db.Channels.Include(i => i.Owner).Include(i => i.Users).ThenInclude(i => i.User).ToList();
        foreach (var channel in channel_list)
        {
            if(channel.Id == 4)
                continue;
                
            // First check the channel owner
            CheckTheUserForMessage(channel.Owner, channel.Id);
            // For all users in the currently fetched channel
            foreach (var user in channel.Users)
            {
                CheckTheUserForMessage(user.User, user.ChannelId);
            }
        }
        System.Diagnostics.Debug.WriteLine("----------------------------");
    }

    public async void CheckTheUserForMessage(ApplicationUser user, int ChannelId)
    {
        if(user.UserName == "haha" || user.UserName == "haha2")
            return;

        //Console.WriteLine(user.UserId);
        var sent_message_count = 0;
        var sent_email_body = "";
        var last_seen_time = DateTime.Now;
        List<TextModel> message_list = new List<TextModel>();
        
        // last seen is updated ondisconnectedasync
        RedisValue test_data;
        try{
            test_data = _redis_db.HashGet("LastSeen" + ChannelId, user.Id);
        }catch
        {
            return;
        }
        // Console.WriteLine("Plz: " + test_data);
        last_seen_time = Convert.ToDateTime(test_data);

        // Get all the messages in that channel
        // Compare them with last seen, and append them to list
        var messages = _redis_db.ListRange(ChannelId.ToString(),0,-1);
        foreach(var message in messages)
        {
            TextModel text = JsonConvert.DeserializeObject<TextModel>(message);

            var sent_time = Convert.ToDateTime(text.SentTime);
            if( sent_time > last_seen_time )
            {
                // Add message to bulk email content
                sent_email_body+=text.Content;
                // Add message to list
                message_list.Add(text);
                // Increment the counter
                sent_message_count++;
            }
        }
        // If no message is fetched no need to send a mail
        if (sent_message_count == 0)
        {
            System.Diagnostics.Debug.WriteLine("No need to send a mail.");
        }
        else
        {
            var finalized_mail_body = GetFormattedMessage(message_list, user);
            // Send the email to user
            await _emailSender.SendEmailAsync(user.Email, "Channel Bulk Mail Feed", finalized_mail_body);
            // Set last seen for this person in this channel to this message
            HashEntry entry = new HashEntry(user.Id.ToString(), DateTime.Now.ToString());
            HashEntry[] arr = new HashEntry[1];
            arr[0] = entry;
            _redis_db.HashSet("LastSeen" + ChannelId.ToString(), arr);
        }
    }

    public string GetFormattedMessage(List<TextModel> message_list, ApplicationUser current_user)
    {
        var finalized_message = "";
        finalized_message+=System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\latest_feed_header.txt");;
        foreach(var mes in message_list)
        {
            if (mes.User.UserId == current_user.Id)
            {
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\self_msg_template\\div_row_self_upper.txt");
                finalized_message += mes.User.Name;
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\self_msg_template\\div_row_self_middle.txt");
                finalized_message += mes.Content;
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\self_msg_template\\div_row_self_lower.txt");
            }
            else
            {
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\other_ppl_msg_template\\div_row_other_upper.txt");
                finalized_message += mes.User.Name;
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\other_ppl_msg_template\\div_row_other_middle.txt");
                finalized_message += mes.Content;
                finalized_message += System.IO.File.ReadAllText(_env.ContentRootPath + "\\wwwroot\\email_templates\\bulk_templates\\other_ppl_msg_template\\div_row_other_lower.txt");
            }
        }
        return finalized_message;
    }
}