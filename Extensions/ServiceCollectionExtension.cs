using System;
using Helios.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet.Client.Options;

namespace Helios.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddMqttClientServiceWithConfig(
            this IServiceCollection services,
            Action<MqttClientOptionsBuilder> configure,
            Action<HeliosOptions> configureOptions
        )
        {
            services.AddSingleton<IMqttClientOptions>(_ =>
            {
                var optionsBuilder = new MqttClientOptionsBuilder();

                configure(optionsBuilder);

                return optionsBuilder.Build();
            });
            services.AddSingleton<HeliosOptions>(_ =>
                {
                    var options = new HeliosOptions();

                    configureOptions(options);

                    return options;
                }
            );
            services.AddSingleton<MqttClientService>();
            services.AddSingleton<IHostedService>(serviceProvider =>
            {
                return serviceProvider.GetService<MqttClientService>();
            });
            services.AddSingleton<MqttClientServiceProvider>(serviceProvider =>
            {
                var mqttClientService = serviceProvider.GetService<MqttClientService>();
                var mqttClientServiceProvider = new MqttClientServiceProvider(mqttClientService);
                return mqttClientServiceProvider;
            });
            return services;
        }
    }
}