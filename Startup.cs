using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Helios.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddGrpc();
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
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
