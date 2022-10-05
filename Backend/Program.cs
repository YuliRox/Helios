using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Helios
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Current Time:{DateTime.Now}|{DateTime.UtcNow}");
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) =>
                    config.AddEnvironmentVariables(prefix: "HELIOS_"))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    /*webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(5200);
                    });*/
                    webBuilder.UseStartup<Startup>();
                });
    }
}
