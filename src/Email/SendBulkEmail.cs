using System;
using System.Threading.Tasks;
using Quartz;
using ChannelX.Email;
public class SendBulkEmail : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Trying to execute the job.");
        throw new System.NotImplementedException();
    }
}