
using System;
using System.Threading.Tasks;
using Helios.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Helios.Jobs;

public class LightsOffJob : DimmerJob
{
    public LightsOffJob(ILogger<DimmerJob> logger, IManagedDimmer managedDimmer, HeliosOptions heliosOptions)
        : base(logger, managedDimmer, heliosOptions)
    {
    }

    protected override async Task ExecuteInternal(IJobExecutionContext context)
    {
        Logger.LogInformation("Trigger for Job {0}", context.Trigger.Key.Name);

        await ManagedDimmer.SetPercentage(0);
        await ManagedDimmer.TurnOff();


        using var session = new OperationSession();
    }
}