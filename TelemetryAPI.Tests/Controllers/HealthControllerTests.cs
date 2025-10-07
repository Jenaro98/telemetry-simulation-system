using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace TelemetryAPI.Tests.Controllers;

public class HealthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var healthData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        healthData.GetProperty("status").GetString().Should().Be("Healthy");
        healthData.GetProperty("version").GetString().Should().Be("1.0.0");
        healthData.TryGetProperty("timestamp", out _).Should().BeTrue();
        healthData.TryGetProperty("environment", out _).Should().BeTrue();
    }
}
