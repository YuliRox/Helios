using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Helios.Services
{
    public class MqttClientService : IMqttClientService
    {
        private readonly ILogger<MqttClientService> logger;
        private readonly HeliosOptions config;
        private IMqttClient mqttClient;
        private IMqttClientOptions options;

        public MqttClientService(HeliosOptions config, IMqttClientOptions options, ILogger<MqttClientService> logger)
        {
            this.logger = logger;
            this.options = options;
            this.config = config;
            mqttClient = new MqttFactory()
                .CreateMqttClient();

            ConfigureMqttClient();
        }

        private void ConfigureMqttClient()
        {
            mqttClient.ConnectedHandler = this;
            mqttClient.DisconnectedHandler = this;
            mqttClient.ApplicationMessageReceivedHandler = this;
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
            this.logger.LogInformation($"Message received: {payload}");
            return Task.CompletedTask;
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            this.logger.LogInformation("Connected to Mqtt Server");
            // TODO subscribe to topics

            foreach (var topic in config.ListenTopics){
                await this.mqttClient.SubscribeAsync(topic);
            }
        }

        public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
        {
            switch (eventArgs.Reason)
            {
                case MqttClientDisconnectReason.NotAuthorized:
                    {
                        throw new Exception("Disconnected due to invalid authentication", eventArgs.Exception);
                    }
                case MqttClientDisconnectReason.NormalDisconnection:
                    {
                        return;
                    }
            }
            // wait 5 seconds and then reconnect
            await Task.Delay(5000);

            await mqttClient.ConnectAsync(options);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await mqttClient.ConnectAsync(options);
            if (!mqttClient.IsConnected)
            {
                await mqttClient.ReconnectAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                var disconnectOption = new MqttClientDisconnectOptions
                {
                    ReasonCode = MqttClientDisconnectReason.NormalDisconnection,
                    ReasonString = "Normal Disconnect"
                };
                await mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
            }
            await mqttClient.DisconnectAsync();
        }
    }
}