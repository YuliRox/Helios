using MQTTnet;
using MQTTnet.Client;
using System.Text;

namespace Helios.Services;

public interface IManagedDimmer
{
    Task TurnOn();
    Task TurnOff();
    Task<string> SetPercentage(int percentage);
}

public class MqttClientService : IMqttClientService, IManagedDimmer, IDisposable
{
    private readonly ILogger<MqttClientService> _logger;
    private readonly HeliosOptions _heliosOptions;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _mqttOptions;
    private bool _shouldReconnect = true;

    public MqttClientService(HeliosOptions config, MqttClientOptions options, ILogger<MqttClientService> logger)
    {
        _logger = logger;
        _mqttOptions = options;
        _heliosOptions = config;
        _mqttClient = new MqttFactory()
            .CreateMqttClient();

        ConfigureMqttClient();
    }

    private void ConfigureMqttClient()
    {
        _mqttClient.ConnectedAsync += HandleConnectedAsync;
        _mqttClient.DisconnectedAsync += HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += HandleApplicationMessageReceivedAsync;
    }

    public void Dispose()
    {
        _mqttClient.ConnectedAsync -= HandleConnectedAsync;
        _mqttClient.DisconnectedAsync -= HandleDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync -= HandleApplicationMessageReceivedAsync;
    }

    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
        var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment);
        _logger.LogInformation("Message received: {Payload} on Topic: {Topic}", payload, eventArgs.ApplicationMessage.Topic);

        if (OperationSession.Current != null)
        {
            OperationSession.Current.MessageBuffer.Add((eventArgs.ApplicationMessage.Topic, payload));
        }

        return Task.CompletedTask;
    }

    public async Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
    {
        switch (eventArgs.ConnectResult.ResultCode)
        {
            case MqttClientConnectResultCode.Success:
                break;
            default:
                _logger.LogError("Connection failed: {ResultCode}", eventArgs.ConnectResult.ResultCode);
                return;
        }
        _logger.LogInformation("Connected to Mqtt Server");

        foreach (var topic in _heliosOptions.ListenTopics)
        {
            await _mqttClient.SubscribeAsync(topic);
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
        if (!_shouldReconnect)
            return;

        // wait 5 seconds and then reconnect
        await Task.Delay(5000);

        await _mqttClient.ConnectAsync(_mqttOptions);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _shouldReconnect = true;
        await _mqttClient.ConnectAsync(_mqttOptions, cancellationToken);
        if (!_mqttClient.IsConnected)
        {
            await _mqttClient.ReconnectAsync();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _shouldReconnect = false;

        var disconnectOption = new MqttClientDisconnectOptions
        {
            ReasonString = "Normal Disconnect"
        };
        await _mqttClient.DisconnectAsync(disconnectOption, cancellationToken);
    }

    protected async Task PublishMessage(string topic, string message)
    {
        if (!_mqttClient.IsConnected)
            await _mqttClient.ReconnectAsync();

        await _mqttClient.PublishAsync(new MqttApplicationMessage()
        {
            Topic = topic,
            PayloadSegment = Encoding.UTF8.GetBytes(message)
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