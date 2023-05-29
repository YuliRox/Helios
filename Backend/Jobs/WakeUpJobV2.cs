
using System;
using System.Threading.Tasks;
using Helios.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Helios.Jobs;

public class WakeUpJobV2 : DimmerJob
{
    public WakeUpJobV2(ILogger<DimmerJob> logger, IManagedDimmer managedDimmer, HeliosOptions heliosOptions)
        : base(logger, managedDimmer, heliosOptions)
    {
    }

    protected override async Task ExecuteInternal(IJobExecutionContext context)
    {
        Logger.LogInformation("Trigger for Job {0}", context.Trigger.Key.Name);

        await ManagedDimmer.SetPercentage(0);
        await ManagedDimmer.TurnOn();

        var percentage = HeliosOptions.DimmerMinimumPercentage;
        var percentageDiff = 100.0 - (double)percentage;
        var timeFrame = (double)HeliosOptions.DimmerTime;
        var timeIntervall = (int)Math.Ceiling(timeFrame / percentageDiff);

        // let mqtt settle
        await Task.Delay(5000);

        using var session = new OperationSession();

        while (percentage <= 100)
        {
            var percentageMessage = await ManagedDimmer.SetPercentage(percentage);
            percentage++;
            await Task.Delay(timeIntervall);

            if (session.IsInterrupted(Logger, HeliosOptions.DimmerPercentageStatusTopic, percentageMessage))
            {
                Logger.LogCritical("Job got interrupted by User");
                return;
            }
        }

        /*await this.ManagedDimmer.SetPercentage(100);
        await this.ManagedDimmer.TurnOn();
        for(var flashCount = 0; flashCount < 30; flashCount++) {
            await this.ManagedDimmer.SetPercentage(0);
            await Task.Delay(500);
            await this.ManagedDimmer.SetPercentage(100);
            await Task.Delay(500);
        }*/
    }
}