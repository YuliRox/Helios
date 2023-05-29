using System;
using Quartz;
using Microsoft.Extensions.Configuration;
using Helios.Jobs;

namespace Helios.Extensions;

public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static void AddJobAndTrigger<T>(
        this IServiceCollectionQuartzConfigurator quartz,
        IConfiguration config)
        where T : IJob
    {
        var quartzConfig = config.GetSection("Quartz:Jobs").GetChildren();

        if (!quartzConfig.Any())
        {
            return;
        }

        var jobName = typeof(T).Name;
        // register the job in Quartz
        var jobKey = new JobKey(jobName);
        quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

        var jobNameOff = "LightsOffJob";
        var jobKeyOff = new JobKey(jobNameOff);
        quartz.AddJob<LightsOffJob>(opts => opts.WithIdentity(jobKeyOff));

        foreach (var jobConfig in quartzConfig)
        {
            var triggerName = jobConfig.GetValue("Name", "Default");
            var cronSchedule = jobConfig.GetValue<string>("On");
            var cronScheduleOff = jobConfig.GetValue<string>("Off");

            // minor validation
            if (string.IsNullOrEmpty(cronSchedule))
            {
                throw new Exception($"No Quartz.NET Cron schedule found for job in configuration {jobName}");
            }

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-" + triggerName + "-trigger")
                .WithCronSchedule(cronSchedule,
                    x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
                )));

            if (!string.IsNullOrEmpty(cronScheduleOff))
            {
                quartz.AddTrigger(opts => opts.ForJob(jobKeyOff)
                    .WithIdentity(jobNameOff + "-" + triggerName + "-trigger")
                    .WithCronSchedule(cronScheduleOff,
                        x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
                    )));
            }
        }
    }
}