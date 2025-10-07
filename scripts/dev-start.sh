#!/bin/bash

# Bash script to start the development environment
echo "Starting Telemetry Simulation System..."

# Check if Docker is running
if ! docker version > /dev/null 2>&1; then
    echo "Docker is not running. Please start Docker."
    exit 1
fi

echo "Docker is running"

# Start services with docker-compose
echo "Starting services with docker-compose..."
docker-compose up -d

# Wait for services to be ready
echo "Waiting for services to be ready..."
sleep 30

# Check if API is responding
echo "Checking API health..."
if curl -f http://localhost:5000/api/health > /dev/null 2>&1; then
    echo "API is healthy"
else
    echo "API is not responding yet. Please wait a moment and try again."
fi

echo ""
echo "Services started successfully!"
echo "API: http://localhost:5000"
echo "Swagger UI: http://localhost:5000/swagger"
echo "RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "MongoDB: localhost:27017"

echo ""
echo "To view logs: docker-compose logs -f"
echo "To stop services: docker-compose down"
