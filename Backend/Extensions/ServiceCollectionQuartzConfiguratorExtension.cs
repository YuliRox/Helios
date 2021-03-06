using System;
using Quartz;
using Microsoft.Extensions.Configuration;

namespace Helios.Extensions
{
    public static class ServiceCollectionQuartzConfiguratorExtensions
    {
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            IConfiguration config)
            where T : IJob
        {
            var quartzConfig = config.GetSection("Quartz");

            var jobName = typeof(T).Name;
            // register the job in Quartz
            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            foreach (var jobConfig in quartzConfig.GetChildren())
            {
                var cronSchedule = jobConfig.Value;
                var triggerName = jobConfig.Key;

                // minor validation
                if (string.IsNullOrEmpty(cronSchedule))
                {
                    throw new Exception($"No Quartz.NET Cron schedule found for job in configuration {jobName}");
                }

                quartz.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity(jobName + "-" + triggerName + "-trigger")
                    .WithCronSchedule(cronSchedule)
                    );
            }
        }
    }
}