namespace TelemetryAPI.Services;

public interface IRabbitMQService
{
    void PublishMessage(string message);
    void Dispose();
}
