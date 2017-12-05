using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using ChannelX.Email;
using ChannelX.Data;
using ChannelX.Models;
using ChannelX.Models.Channel;
using Microsoft.EntityFrameworkCore;

public class SendBulkEmail : IJob
{
    readonly DatabaseContext _db;
    readonly IEmailSender _emailSender;
    public SendBulkEmail(IEmailSender emailSender, DatabaseContext db)
    {
        _db = db;
        _emailSender = emailSender;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Trying to execute the job.");
        GenerateBulkEmail();
        // SMTP TEST START
        // await _emailSender.SendEmailAsync("asdfgh1453@mynet.com", "subject", "Enter email body here");
        // -----SMTP TEST END
    }

    public void GenerateBulkEmail()
    {
        var data = _db.Channels.Include(i => i.Users).ToList();
        // foreach (var channel in data)
        // {
        //     Console.WriteLine(channel.Title);

        //     // Console.WriteLine(channel.Users);
        //     foreach (var user in channel.Users)
        //     {
        //         Console.WriteLine(user);
        //     }
        // }
        // Console.WriteLine("Channels succesfully gotten!");
    }
}