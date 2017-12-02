
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace ChannelX.Email
{
    public static class QuartzService
    {
        public static void AddQuartz(this IServiceCollection sc)
        {
            Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("Creating the scheduler.");

                    NameValueCollection cfg = new NameValueCollection();
                    cfg.Add("quartz.scheduler.instanceName", "BulkEmail scheduler");
                    cfg.Add("quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz");
                    cfg.Add("quartz.threadPool.threadCount", Environment.ProcessorCount.ToString());

                    StdSchedulerFactory factory = new StdSchedulerFactory();
                    factory.Initialize(cfg);
                    IScheduler scheduler = await factory.GetScheduler();
                    scheduler.JobFactory = new IntegrationJobFactory(sc);
                    
                    // and start it off
                    await scheduler.Start();

                    IJobDetail fc = JobBuilder.Create<SendBulkEmail>()
                        .WithIdentity("SendBulkEmail")
                        .Build();

                    ITrigger fct = TriggerBuilder.Create()
                        .WithIdentity("BulkEmailTrigger")
                        // .StartNow()
                        .StartAt(DateTime.Now.AddMinutes(1))
                        // .WithCronSchedule("0 22 * * 0") // “At 22:00 on Sunday.”
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(30)
                            .RepeatForever()
                        )
                        .Build();

                    Console.WriteLine(DateTime.Now);

                    // Tell quartz to schedule the job using our trigger
                    Console.WriteLine("Starting the scheduler");
                    await scheduler.ScheduleJob(fc, fct);
                    Console.WriteLine("Scheduler started!");
                }
                catch (SchedulerException se)
                {
                    Console.WriteLine(se);
                }
            });
        }

        internal sealed class IntegrationJobFactory : IJobFactory
        {
            private readonly IServiceProvider _container;
            public IntegrationJobFactory(IServiceCollection sc)
            {
                _container = sc.BuildServiceProvider();
            }
            public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            {
                var jobDetail = bundle.JobDetail;
                var job = (IJob)_container.GetRequiredService(jobDetail.JobType);
                return job;
            }
            public void ReturnJob(IJob job)
            {
            }
        }
    }
}