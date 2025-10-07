# Telemetry Simulation System

A comprehensive telemetry simulation system built with .NET 8 WebAPI, MongoDB, RabbitMQ, and Go simulator.

## Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Go Simulator  │───▶│   .NET WebAPI   │───▶│     MongoDB     │
│                 │    │                 │    │                 │
│ Generates random│    │ /ingest         │    │ Stores telemetry│
│ telemetry data  │    │ /latest         │    │ data            │
│ every 5 seconds │    │ /health         │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                │
                                ▼
                       ┌─────────────────┐
                       │    RabbitMQ     │
                       │                 │
                       │ Message broker  │
                       │ for async proc. │
                       └─────────────────┘
```

## Components

### 1. .NET 8 WebAPI (`TelemetryAPI/`)
- **Endpoints:**
  - `POST /api/telemetry/ingest` - Receives telemetry data
  - `GET /api/telemetry/latest` - Gets latest telemetry data
  - `GET /api/health` - Health check endpoint
- **Features:**
  - MongoDB integration for data storage
  - RabbitMQ integration for message queuing
  - Swagger documentation
  - CORS enabled

### 2. Go Simulator (`telemetry-simulator/`)
- Generates random telemetry data every 5 seconds
- Simulates multiple device types (temperature, humidity, pressure, multi-sensor)
- Posts data to the API `/ingest` endpoint
- Configurable via environment variables

### 3. MongoDB
- Stores telemetry data
- Default database: `telemetry_db`
- Collection: `telemetry`

### 4. RabbitMQ
- Message broker for async processing
- Management UI available at `http://localhost:15672`
- Default credentials: `guest/guest`

## Quick Start

### Prerequisites
- Docker and Docker Compose
- .NET 8 SDK (for local development)
- Go 1.21+ (for local development)

### Using Docker Compose (Recommended)

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **Check services:**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - RabbitMQ Management: http://localhost:15672 (guest/guest)
   - MongoDB: localhost:27017

3. **View logs:**
   ```bash
   docker-compose logs -f
   ```

4. **Stop services:**
   ```bash
   docker-compose down
   ```

### Local Development

1. **Start MongoDB and RabbitMQ:**
   ```bash
   docker-compose up -d mongodb rabbitmq
   ```

2. **Run the .NET API:**
   ```bash
   cd TelemetryAPI
   dotnet run
   ```

3. **Run the Go simulator:**
   ```bash
   cd telemetry-simulator
   go run main.go
   ```

## API Usage

### Ingest Telemetry Data
```bash
curl -X POST http://localhost:5000/api/telemetry/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "sensor-001",
    "deviceType": "temperature",
    "temperature": 25.5,
    "humidity": 60.0,
    "pressure": 1013.25,
    "batteryLevel": 85.0,
    "location": "building-a-floor-1",
    "timestamp": "2024-01-01T12:00:00Z"
  }'
```

### Get Latest Telemetry
```bash
# Get latest for specific device
curl http://localhost:5000/api/telemetry/latest?deviceId=sensor-001

# Get latest for all devices
curl http://localhost:5000/api/telemetry/latest
```

### Health Check
```bash
curl http://localhost:5000/api/health
```

## Testing

### Run .NET Tests
```bash
cd TelemetryAPI.Tests
dotnet test
```

### Run Integration Tests
```bash
# Start services
docker-compose up -d

# Wait for services to be ready
sleep 30

# Run tests
curl -f http://localhost:5000/api/health
curl -X POST http://localhost:5000/api/telemetry/ingest -H "Content-Type: application/json" -d '{"deviceId":"test","deviceType":"temperature","temperature":25.5,"humidity":60.0,"pressure":1013.25,"batteryLevel":85.0,"location":"test","timestamp":"2024-01-01T00:00:00Z"}'
curl -f http://localhost:5000/api/telemetry/latest

# Stop services
docker-compose down
```

## CI/CD

The project includes a Jenkinsfile for CI/CD pipeline with the following stages:
1. Checkout code
2. Build .NET API
3. Run .NET tests
4. Build Go simulator
5. Build Docker images
6. Integration tests
7. Security scan

## Configuration

### Environment Variables

#### .NET API
- `MongoDb__ConnectionString` - MongoDB connection string
- `MongoDb__DatabaseName` - MongoDB database name
- `RabbitMQ__HostName` - RabbitMQ host
- `RabbitMQ__UserName` - RabbitMQ username
- `RabbitMQ__Password` - RabbitMQ password

#### Go Simulator
- `API_URL` - API endpoint URL (default: http://localhost:5000/api/telemetry/ingest)
- `INTERVAL` - Data generation interval (default: 5s)

## Data Model

```json
{
  "deviceId": "string",
  "deviceType": "string",
  "temperature": "number",
  "humidity": "number", 
  "pressure": "number",
  "batteryLevel": "number",
  "location": "string",
  "timestamp": "datetime",
  "additionalData": {
    "signal_strength": "number",
    "uptime_hours": "number",
    "error_count": "number",
    "firmware_version": "string"
  }
}
```

## Troubleshooting

### Common Issues

1. **Port conflicts:** Make sure ports 5000, 27017, 5672, 15672 are available
2. **MongoDB connection:** Check if MongoDB is running and accessible
3. **RabbitMQ connection:** Verify RabbitMQ is running and credentials are correct
4. **API not responding:** Check if all dependencies are running

### Logs
```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs telemetry-api
docker-compose logs telemetry-simulator
docker-compose logs mongodb
docker-compose logs rabbitmq
```

## Development Notes

- The system is designed for development and testing purposes
- For production deployment, consider adding authentication, rate limiting, and monitoring
- The Go simulator generates realistic telemetry data for testing
- All services are containerized for easy deployment and scaling
