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
    public interface IManagedDimmer
    {
        Task TurnOn();
        Task TurnOff();
        Task<string> SetPercentage(int percentage);
    }

    public class MqttClientService : IMqttClientService, IManagedDimmer
    {
        private readonly ILogger<MqttClientService> _logger;
        private readonly HeliosOptions _heliosOptions;
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;
        private bool _shouldReconnect = true;

        public MqttClientService(HeliosOptions config, IMqttClientOptions options, ILogger<MqttClientService> logger)
        {
            this._logger = logger;
            this._mqttOptions = options;
            this._heliosOptions = config;
            _mqttClient = new MqttFactory()
                .CreateMqttClient();

            ConfigureMqttClient();
        }

        private void ConfigureMqttClient()
        {
            _mqttClient.ConnectedHandler = this;
            _mqttClient.DisconnectedHandler = this;
            _mqttClient.ApplicationMessageReceivedHandler = this;
        }

        public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
        {
            var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
            this._logger.LogInformation($"Message received: {payload} on Topic: {eventArgs.ApplicationMessage.Topic}");

            if (OperationSession.Current != null)
            {
                OperationSession.Current.MessageBuffer.Add((eventArgs.ApplicationMessage.Topic, payload));
            }

            return Task.CompletedTask;
        }

        public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
        {
            switch (eventArgs.AuthenticateResult.ResultCode)
            {
                case MqttClientConnectResultCode.Success:
                    break;
                default:
                    this._logger.LogError("Connection fauled: {0}", eventArgs.AuthenticateResult.ResultCode);
                    return;
            }
            this._logger.LogInformation("Connected to Mqtt Server");

            foreach (var topic in _heliosOptions.ListenTopics)
            {
                await this._mqttClient.SubscribeAsync(topic);
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
            }
            if (!this._shouldReconnect)
                return;

            // wait 5 seconds and then reconnect
            await Task.Delay(5000);

            await _mqttClient.ConnectAsync(_mqttOptions);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this._shouldReconnect = true;
            await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ReconnectAsync();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this._shouldReconnect = false;
            if (cancellationToken.IsCancellationRequested)
            {
                var disconnectOption = new MqttClientDisconnectOptions
                {
                    ReasonCode = MqttClientDisconnectReason.NormalDisconnection,
                    ReasonString = "Normal Disconnect"
                };
                await _mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
                return;
            }
            await _mqttClient.DisconnectAsync(cancellationToken);
        }

        protected async Task PublishMessage(string topic, string message)
        {
            if (!_mqttClient.IsConnected)
                await _mqttClient.ReconnectAsync();

            await _mqttClient.PublishAsync(new MqttApplicationMessage()
            {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(message)
            });

            if (OperationSession.Current != null)
            {
                OperationSession.Current.LastSendMessage = message;
            }
        }

        public async Task<string> SetPercentage(int percentage)
        {
            percentage = percentage <= 0 ? 0 : percentage;
            percentage = percentage == 0 ? 0 : Math.Min(100, Math.Max(percentage, _heliosOptions.DimmerMinimumPercentage));

            await PublishMessage(_heliosOptions.DimmerPercentageCommandTopic, percentage.ToString());

            return $"{{\"POWER\":\"ON\",\"Dimmer\":{percentage}}}";
        }

        public async Task TurnOn()
        {
            await PublishMessage(_heliosOptions.DimmerOnOffCommandTopic, "{\"POWER\":\"ON\"}");
        }

        public async Task TurnOff()
        {
            await PublishMessage(_heliosOptions.DimmerOnOffCommandTopic, "{\"POWER\":\"OFF\"}");
        }
    }
}