using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using TelemetryAPI.Models;
using TelemetryAPI.Repositories;
using TelemetryAPI.Services;
using FluentAssertions;
using Moq;

namespace TelemetryAPI.Tests.Controllers;

public class TelemetryControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ITelemetryRepository> _mockRepository;
    private readonly Mock<IRabbitMQService> _mockRabbitMQService;

    public TelemetryControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _mockRepository = new Mock<ITelemetryRepository>();
        _mockRabbitMQService = new Mock<IRabbitMQService>();
    }

    [Fact]
    public async Task IngestTelemetry_WithValidData_ReturnsOk()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockRepository.Object);
                services.AddSingleton(_mockRabbitMQService.Object);
            });
        }).CreateClient();

        var telemetryData = new TelemetryData
        {
            DeviceId = "test-device-001",
            DeviceType = "temperature",
            Temperature = 25.5,
            Humidity = 60.0,
            Pressure = 1013.25,
            BatteryLevel = 85.0,
            Location = "test-location",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(telemetryData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _mockRepository.Setup(x => x.CreateAsync(It.IsAny<TelemetryData>()))
                      .ReturnsAsync("test-id-123");

        // Act
        var response = await client.PostAsync("/api/telemetry/ingest", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<TelemetryData>()), Times.Once);
        _mockRabbitMQService.Verify(x => x.PublishMessage(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetLatestTelemetry_WithDeviceId_ReturnsLatestData()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockRepository.Object);
                services.AddSingleton(_mockRabbitMQService.Object);
            });
        }).CreateClient();

        var expectedData = new TelemetryData
        {
            Id = "test-id-123",
            DeviceId = "test-device-001",
            DeviceType = "temperature",
            Temperature = 25.5,
            Humidity = 60.0,
            Pressure = 1013.25,
            BatteryLevel = 85.0,
            Location = "test-location",
            Timestamp = DateTime.UtcNow
        };

        _mockRepository.Setup(x => x.GetLatestAsync("test-device-001"))
                      .ReturnsAsync(expectedData);

        // Act
        var response = await client.GetAsync("/api/telemetry/latest?deviceId=test-device-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var actualData = JsonSerializer.Deserialize<TelemetryData>(responseContent);
        actualData.Should().BeEquivalentTo(expectedData);
    }

    [Fact]
    public async Task GetLatestTelemetry_WithoutDeviceId_ReturnsAllLatest()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockRepository.Object);
                services.AddSingleton(_mockRabbitMQService.Object);
            });
        }).CreateClient();

        var expectedData = new List<TelemetryData>
        {
            new TelemetryData
            {
                Id = "test-id-1",
                DeviceId = "device-1",
                DeviceType = "temperature",
                Temperature = 25.5,
                Timestamp = DateTime.UtcNow
            },
            new TelemetryData
            {
                Id = "test-id-2",
                DeviceId = "device-2",
                DeviceType = "humidity",
                Humidity = 60.0,
                Timestamp = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(x => x.GetAllLatestAsync())
                      .ReturnsAsync(expectedData);

        // Act
        var response = await client.GetAsync("/api/telemetry/latest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var actualData = JsonSerializer.Deserialize<List<TelemetryData>>(responseContent);
        actualData.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestTelemetry_WithNonExistentDeviceId_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_mockRepository.Object);
                services.AddSingleton(_mockRabbitMQService.Object);
            });
        }).CreateClient();

        _mockRepository.Setup(x => x.GetLatestAsync("non-existent-device"))
                      .ReturnsAsync((TelemetryData?)null);

        // Act
        var response = await client.GetAsync("/api/telemetry/latest?deviceId=non-existent-device");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
