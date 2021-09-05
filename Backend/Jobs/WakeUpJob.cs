
using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Quartz;

namespace Helios.Jobs
{
    public class WakeUpJob : IJob
    {
        private static async Task SendPercentage(IMqttClient mqttClient, string topic, int percentage) {
            await mqttClient.PublishAsync(new MqttApplicationMessage() {
                Topic = topic,
                Payload = Encoding.UTF8.GetBytes(percentage.ToString())
            });
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var dimmerPercentageCommandTopic = context.MergedJobDataMap.GetString("Helios:PercentageTopic");
                var dimmerOnOffCommandTopic = context.MergedJobDataMap.GetString("Helios:OnOffTopic");

                var mqttClient = new MqttFactory().CreateMqttClient();

                var mqttClientOptionsBuilder = new MqttClientOptionsBuilder();
                var mqttClientOptions = mqttClientOptionsBuilder
                    .WithClientId(context.MergedJobDataMap.GetString("Mqtt:ClientId"))
                    .WithTcpServer(
                        context.MergedJobDataMap.GetString("Mqtt:Server"),
                        context.MergedJobDataMap.GetInt("Mqtt:Port"))
                    .WithCredentials(
                        context.MergedJobDataMap.GetString("Mqtt:Username"),
                        context.MergedJobDataMap.GetString("Mqtt:Password"))
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions);

                await mqttClient.PublishAsync(new MqttApplicationMessage() {
                    Topic = dimmerOnOffCommandTopic,
                    Payload = Encoding.UTF8.GetBytes("ON")
                });

                await SendPercentage(mqttClient, dimmerPercentageCommandTopic, 0);

                var percentage = 20;
                while(percentage <= 100) {

                    await SendPercentage(mqttClient, dimmerPercentageCommandTopic, percentage);

                    percentage++;
                    await Task.Delay(15000);
                }

                await mqttClient.DisconnectAsync();
            }
            catch (Exception)
            {

            }
        }
    }
}