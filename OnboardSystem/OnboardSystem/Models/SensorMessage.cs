using MassTransit;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using OnboardSystem.Services;
using System.Diagnostics;

namespace OnboardSystem.Models;

public class SensorMessage
{
    public int Id { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
}

public class SensorConsumer : IConsumer<SensorMessage>
{
    private readonly WebSocketConnectionManager _webSocketConnectionManager;

    private readonly SensorService _sensorService;

    private static readonly Stopwatch stopwatch = new Stopwatch();

    public SensorConsumer(
        WebSocketConnectionManager webSocketConnectionManager, 
        SensorService sensorService)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
        _sensorService = sensorService;
        stopwatch.Start();
    }

    public async Task Consume(ConsumeContext<SensorMessage> context)
    {
        Console.WriteLine($"Received message: {context.Message.Id} - {context.Message.Value} - {context.Message.Unit}");

        Sensor receivedData = new Sensor(
                Guid.NewGuid(),
                context.Message.Id,
                context.Message.Value,
                context.Message.Unit,
                (stopwatch.ElapsedMilliseconds / 1000));

        await _sensorService.CreateAsync(receivedData);

        var messageJson = JsonSerializer.Serialize(receivedData);
        var buffer = Encoding.UTF8.GetBytes(messageJson);

        foreach (var socket in _webSocketConnectionManager.GetAllSockets())
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
