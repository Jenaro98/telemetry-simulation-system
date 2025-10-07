package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"os"
	"time"
)

type TelemetryData struct {
	DeviceID      string                 `json:"deviceId"`
	DeviceType    string                 `json:"deviceType"`
	Temperature   float64                `json:"temperature"`
	Humidity      float64                `json:"humidity"`
	Pressure      float64                `json:"pressure"`
	BatteryLevel  float64                `json:"batteryLevel"`
	Location      string                 `json:"location"`
	Timestamp     time.Time              `json:"timestamp"`
	AdditionalData map[string]interface{} `json:"additionalData"`
}

type DeviceConfig struct {
	DeviceID   string
	DeviceType string
	Location   string
}

var deviceConfigs = []DeviceConfig{
	{"sensor-001", "temperature", "building-a-floor-1"},
	{"sensor-002", "humidity", "building-a-floor-2"},
	{"sensor-003", "pressure", "building-b-floor-1"},
	{"sensor-004", "multi-sensor", "building-b-floor-2"},
	{"sensor-005", "temperature", "building-c-floor-1"},
}

func main() {
	apiURL := getEnv("API_URL", "http://localhost:5000/api/telemetry/ingest")
	interval := getEnv("INTERVAL", "5s")
	
	duration, err := time.ParseDuration(interval)
	if err != nil {
		log.Fatalf("Invalid interval format: %v", err)
	}

	log.Printf("Starting telemetry simulator...")
	log.Printf("API URL: %s", apiURL)
	log.Printf("Interval: %s", interval)

	ticker := time.NewTicker(duration)
	defer ticker.Stop()

	// Send initial data
	sendTelemetryData(apiURL)

	for {
		select {
		case <-ticker.C:
			sendTelemetryData(apiURL)
		}
	}
}

func sendTelemetryData(apiURL string) {
	config := deviceConfigs[rand.Intn(len(deviceConfigs))]
	
	telemetryData := TelemetryData{
		DeviceID:      config.DeviceID,
		DeviceType:    config.DeviceType,
		Temperature:   generateTemperature(),
		Humidity:      generateHumidity(),
		Pressure:      generatePressure(),
		BatteryLevel:  generateBatteryLevel(),
		Location:      config.Location,
		Timestamp:     time.Now().UTC(),
		AdditionalData: generateAdditionalData(),
	}

	jsonData, err := json.Marshal(telemetryData)
	if err != nil {
		log.Printf("Error marshaling telemetry data: %v", err)
		return
	}

	resp, err := http.Post(apiURL, "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		log.Printf("Error sending telemetry data: %v", err)
		return
	}
	defer resp.Body.Close()

	if resp.StatusCode == http.StatusOK {
		log.Printf("Successfully sent telemetry data for device %s", telemetryData.DeviceID)
	} else {
		log.Printf("Failed to send telemetry data. Status: %d", resp.StatusCode)
	}
}

func generateTemperature() float64 {
	// Generate temperature between 15-35°C
	return 15 + rand.Float64()*20
}

func generateHumidity() float64 {
	// Generate humidity between 30-80%
	return 30 + rand.Float64()*50
}

func generatePressure() float64 {
	// Generate pressure between 980-1020 hPa
	return 980 + rand.Float64()*40
}

func generateBatteryLevel() float64 {
	// Generate battery level between 20-100%
	return 20 + rand.Float64()*80
}

func generateAdditionalData() map[string]interface{} {
	return map[string]interface{}{
		"signal_strength": rand.Intn(100),
		"uptime_hours":    rand.Intn(720), // 0-30 days
		"error_count":     rand.Intn(10),
		"firmware_version": fmt.Sprintf("v%d.%d.%d", rand.Intn(3), rand.Intn(10), rand.Intn(10)),
	}
}

func getEnv(key, defaultValue string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return defaultValue
}