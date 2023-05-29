using Helios.Services;
using MQTTnet.Client;

namespace Helios.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMqttClientServiceWithConfig(
        this IServiceCollection services,
        Action<MqttClientOptionsBuilder> configure,
        Action<HeliosOptions> configureOptions
    )
    {
        services.AddSingleton<MqttClientOptions>(_ =>
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

        services.AddSingleton<IManagedDimmer>(serviceProvider => serviceProvider.GetRequiredService<MqttClientService>());

        services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetRequiredService<MqttClientService>());

        services.AddSingleton<MqttClientServiceProvider>(serviceProvider =>
        {
            var mqttClientService = serviceProvider.GetRequiredService<MqttClientService>();
            var mqttClientServiceProvider = new MqttClientServiceProvider(mqttClientService);
            return mqttClientServiceProvider;
        });
        return services;
    }
}