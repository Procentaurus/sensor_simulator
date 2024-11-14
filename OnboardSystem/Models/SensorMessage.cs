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

    private readonly BlockchainService _blockchainService;

    private static readonly Stopwatch stopwatch = new Stopwatch();

    public SensorConsumer(
        WebSocketConnectionManager webSocketConnectionManager, 
        SensorService sensorService,
        BlockchainService blockchainService)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
        _sensorService = sensorService;
        _blockchainService = blockchainService;
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
        await _blockchainService.RewardSensor(context.Message.Id);

        List<Sensor> result = _sensorService.GetAllAsync().Result;

        List<Sensor> filteredResult = new List<Sensor>();

        foreach (Sensor sensor in result)
        {
            if (sensor.id == receivedData.id)
            {
                filteredResult.Add(sensor);
            }
        }
        result = filteredResult;

        result.Sort((sensor1, sensor2) => (int)(sensor2.timestamp - sensor1.timestamp));

        double avgValue = result.Take(100).ToList().Average(sensor => sensor.value);
        receivedData.avgValue = avgValue;

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
