using Helios.Data;
using Helios.Extensions;
using Helios.Jobs;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;
using Quartz;
using Quartz.Impl.AdoJobStore;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var Configuration = builder.Configuration;

builder.Services.AddMqttClientServiceWithConfig((configBuilder) =>
{
    configBuilder
        .WithCredentials(
            Configuration["Mqtt:Username"],
            Configuration["Mqtt:Password"]
        )
        .WithClientId(Configuration.GetValue<string>("Mqtt:ClientId", "HELIOS-Daemon"))
        .WithTcpServer(
            Configuration["Mqtt:Server"],
            Configuration.GetValue<int>("Mqtt:Port", 1883)
        )
        .WithKeepAlivePeriod(TimeSpan.FromMinutes(10))
        ;
}, (heliosOptions) =>
{
    heliosOptions.HeliosListenTopic = Configuration["Mqtt:Topics:Listen"];
    heliosOptions.DimmerOnOffStatusTopic = Configuration["Mqtt:Topics:DimmerOnOffStatus"];
    heliosOptions.DimmerOnOffCommandTopic = Configuration["Mqtt:Topics:DimmerOnOffCommand"];
    heliosOptions.DimmerPercentageStatusTopic = Configuration["Mqtt:Topics:DimmerPercentageStatus"];
    heliosOptions.DimmerPercentageCommandTopic = Configuration["Mqtt:Topics:DimmerPercentageCommand"];
    heliosOptions.DimmerMinimumPercentage = Configuration.GetValue<int>("Helios:DimmerMinPercentage", 20);
    heliosOptions.DimmerTime = Configuration.GetValue<int>("Helios:DimmerTime", 20 * 60 * 1000);
});


builder.Services.AddQuartz(q =>
{
    q.SchedulerId = "JobScheduler";
    q.SchedulerName = "Job Scheduler";
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.AddJobAndTrigger<WakeUpJobV2>(Configuration);
    q.UsePersistentStore(store =>
    {
        store.UseProperties = true;
        store.RetryInterval = TimeSpan.FromSeconds(15);
        store.UseJsonSerializer();
        store.UsePostgres(postgresOptions =>
        {
            postgresOptions.UseDriverDelegate<PostgreSQLDelegate>();
            postgresOptions.ConnectionString = Configuration.GetConnectionString("HeliosDb");
            postgresOptions.TablePrefix = "QRTZ_";
        });
        store.PerformSchemaValidation = true;
    });
});

builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

builder.Services.AddSingleton<ScheduledEventService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
