using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using nodemonitor.Models;
using System.ComponentModel.DataAnnotations;

namespace nodemonitor.Services;

public class MqttListener(ILogger<MqttListener> logger, IHubContext<NodeHub> hubContext) : IHostedService
{
    private readonly IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
            .WithClientOptions(new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer("mqtt")
                .Build())
        .Build();

        var topics = new[] {
            // kissproxy/gb7rdg-node/platform-3f980000.usb-usb-0:1.3:1.0/toModem/decoded/port0
            new MqttTopicFilterBuilder().WithTopic("kissproxy/+/+/+/+/#").Build(),
        };

        await _mqttClient.SubscribeAsync(topics);
        await _mqttClient.StartAsync(options);

        _mqttClient.ApplicationMessageReceivedAsync += ApplicationMessageReceived;

        logger.LogInformation("Listening for MQTT messages");
    }

    private async Task ApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
    {
        // kissproxy/gb7rdg-node/platform-3f980000.usb-usb-0:1.3:1.0/toModem/decoded/port0

        var parts = arg.ApplicationMessage.Topic.Split('/');

        if (parts.Length < 5)
        {
            return;
        }

        if (parts[4] != "decoded")
        {
            return;
        }

        var node = parts[1];
        var hardwarePort = parts[2];
        var directionS = parts[3];
        var tncPortS = parts[5];

        if (tncPortS.Length < 5 || !int.TryParse(tncPortS[4..], out var tncPortIndex))
        {
            return;
        }

        if (directionS != "toModem" && directionS != "fromModem")
        {
            return;
        }

        var direction = directionS == "toModem" ? ">" : "<";

        var msg = arg.ApplicationMessage.ConvertPayloadToString();

        logger.LogInformation($"{node} {hardwarePort}:{tncPortIndex} {direction} {msg}");

        Decode decode = new()
        {
            Timestamp = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.ff}Z",
            Node = node,
            ModemId = GetModemId(hardwarePort),
            ModemPort = tncPortIndex,
            Direction = direction,
            Data = msg.Replace("\r\n", "\n").Replace("\n", " ")
        };

        await hubContext.Clients.All.SendAsync("ReceiveMessage", decode);
    }

    private static string GetModemId(string hardwarePort) => hardwarePortMap.TryGetValue(hardwarePort, out var value) ? value : hardwarePort;

    private static readonly Dictionary<string, string> hardwarePortMap = new() {
        { "platform-3f980000.usb-usb-0:1.2:1.0", "2m  " },
        { "platform-3f980000.usb-usb-0:1.3:1.0", "70cm" },
    };

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _mqttClient.StopAsync();
    }
}
