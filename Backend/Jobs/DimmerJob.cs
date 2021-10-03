using System;
using System.Threading.Tasks;
using Helios.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Helios.Jobs
{
    public abstract class DimmerJob : IJob
    {
        protected ILogger<DimmerJob> Logger { get; }
        protected IManagedDimmer ManagedDimmer { get; }
        protected HeliosOptions HeliosOptions { get; }

        protected DimmerJob(
            ILogger<DimmerJob> logger,
            IManagedDimmer managedDimmer,
            HeliosOptions heliosOptions
        )
        {
            Logger = logger;
            ManagedDimmer = managedDimmer;
            HeliosOptions = heliosOptions;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ExecuteInternal(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing dimmer job");
            }
        }

        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}