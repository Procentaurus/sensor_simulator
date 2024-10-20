using MassTransit;
using OnboardSystem.Models;
using OnboardSystem.Services;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>();

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.Configure<OnboardDatabaseSettings>(builder.Configuration.GetSection("OnboardDatabase"));
builder.Services.AddSingleton<SensorService>();
builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SensorConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri($"rabbitmq://{rabbitMqSettings.Host}"), h =>
        {
            h.Username(rabbitMqSettings.Username);
            h.Password(rabbitMqSettings.Password);
        });

        cfg.ReceiveEndpoint(rabbitMqSettings.QueueName, ep =>
        {
            ep.ClearSerialization();
            ep.UseRawJsonSerializer();

            ep.ConfigureConsumer<SensorConsumer>(context);
        });
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseWebSockets();
/**
 * https://copilot.microsoft.com/
 * prompt: podaj przyk³ad handlera na nawi¹zywanie po³¹czenia webSocket, 
 * aby zapisaæ z jakimi sesjami jest nawi¹zane po³¹czenie
*/
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var webSocketConnectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
            string connectionId = webSocketConnectionManager.AddSocket(webSocket);
            await HandleWebSocket(context, webSocket, connectionId);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

/**
 * https://copilot.microsoft.com/
 * prompt: podaj przyk³ad handlera na nawi¹zywanie po³¹czenia webSocket, 
 * aby zapisaæ z jakimi sesjami jest nawi¹zane po³¹czenie
*/
async Task HandleWebSocket(HttpContext context, WebSocket webSocket, string connectionId)
{
    var buffer = new byte[1024 * 4];
    var webSocketConnectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();

    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    while (!result.CloseStatus.HasValue)
    {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    webSocketConnectionManager.RemoveSocket(connectionId);
    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}
