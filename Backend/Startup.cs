using System;
using System.IO;

using Helios.Extensions;
using Helios.Jobs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;

using Quartz;
using Microsoft.AspNetCore.Http;

namespace Helios
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //services.AddControllers();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "../Frontend/dist";
            });

            services.AddMqttClientServiceWithConfig((configBuilder) =>
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
                    );
            }, (heliosOptions) =>
            {
                heliosOptions.HeliosListenTopic = Configuration["Mqtt:Topics:Listen"];
                heliosOptions.DimmerOnOffStatusTopic = Configuration["Mqtt:Topics:DimmerOnOffStatus"];
                heliosOptions.DimmerOnOffCommandTopic = Configuration["Mqtt:Topics:DimmerOnOffCommand"];
                heliosOptions.DimmerPercentageStatusTopic = Configuration["Mqtt:Topics:DimmerPercentageStatus"];
                heliosOptions.DimmerPercentageCommandTopic = Configuration["Mqtt:Topics:DimmerPercentageCommand"];
                heliosOptions.DimmerMinimumPercentage = Configuration.GetValue<int>("Helios:DimmerMinPercentage", 20);
                heliosOptions.DimmerTime = Configuration.GetValue<int>("Helio:DimmerTime", 20 * 60 * 1000);
            });

            services.AddQuartz(q =>
            {
                q.SchedulerId = "JobScheduler";
                q.SchedulerName = "Job Scheduler";
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                q.AddJob<WakeUpJob>(j => j
                    .WithIdentity("wakeUp")
                );

                /*q.AddTrigger(t =>
                    t.WithIdentity("wakeUp")
                    .ForJob("wakeUp")
                    .StartAt(new DateTimeOffset(2021, 7, 5, 7, 40, 0, TimeSpan.Zero))
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(24)
                        .WithRepeatCount(5)
                    )
                );*/

                q.AddTrigger(t =>
                    t.WithIdentity("wakeUp")
                    .ForJob("wakeUp")
                    .StartNow()
                );
            });

            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                         name: "default",
                         pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "../Frontend";
                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
