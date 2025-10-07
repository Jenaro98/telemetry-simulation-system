pipeline {
    agent any
    
    environment {
        DOCKER_COMPOSE_FILE = 'docker-compose.yml'
        DOTNET_VERSION = '8.0'
        GO_VERSION = '1.21'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo 'Code checked out successfully'
            }
        }
        
        stage('Build .NET API') {
            steps {
                script {
                    echo 'Building .NET 8 WebAPI...'
                    dir('TelemetryAPI') {
                        sh 'dotnet restore'
                        sh 'dotnet build --configuration Release --no-restore'
                    }
                }
            }
        }
        
        stage('Test .NET API') {
            steps {
                script {
                    echo 'Running .NET API tests...'
                    dir('TelemetryAPI.Tests') {
                        sh 'dotnet test --configuration Release --no-build --verbosity normal'
                    }
                }
            }
            post {
                always {
                    publishTestResults testResultsPattern: '**/TestResults/*.trx'
                }
            }
        }
        
        stage('Build Go Simulator') {
            steps {
                script {
                    echo 'Building Go telemetry simulator...'
                    dir('telemetry-simulator') {
                        sh 'go mod download'
                        sh 'go build -o telemetry-simulator main.go'
                    }
                }
            }
        }
        
        stage('Build Docker Images') {
            steps {
                script {
                    echo 'Building Docker images...'
                    sh 'docker-compose -f ${DOCKER_COMPOSE_FILE} build --no-cache'
                }
            }
        }
        
        stage('Integration Tests') {
            steps {
                script {
                    echo 'Starting services for integration tests...'
                    sh 'docker-compose -f ${DOCKER_COMPOSE_FILE} up -d'
                    
                    // Wait for services to be ready
                    sh 'sleep 30'
                    
                    // Run integration tests
                    sh '''
                        # Test API health endpoint
                        curl -f http://localhost:5000/api/health || exit 1
        
                        # Test telemetry ingestion
                        curl -X POST http://localhost:5000/api/telemetry/ingest \
                             -H "Content-Type: application/json" \
                             -d '{"deviceId":"test-device","deviceType":"temperature","temperature":25.5,"humidity":60.0,"pressure":1013.25,"batteryLevel":85.0,"location":"test-location","timestamp":"2024-01-01T00:00:00Z"}' || exit 1
        
                        # Test latest telemetry endpoint
                        curl -f http://localhost:5000/api/telemetry/latest || exit 1
                    '''
                }
            }
            post {
                always {
                    echo 'Stopping services...'
                    sh 'docker-compose -f ${DOCKER_COMPOSE_FILE} down'
                }
            }
        }
        
        stage('Security Scan') {
            steps {
                script {
                    echo 'Running security scan on Docker images...'
                    sh 'docker images | grep telemetry'
                    // Add Trivy or other security scanning tools here if needed
                }
            }
        }
    }
    
    post {
        always {
            echo 'Pipeline completed'
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            echo 'Pipeline failed!'
        }
        unstable {
            echo 'Pipeline unstable!'
        }
    }
}
