pipeline {
    agent any
    
    stages {
        stage('Test System') {
            steps {
                script {
                    echo 'Testing telemetry system...'
                    
                    // Check if Docker is available
                    sh 'docker --version'
                    
                    // Check if our services are running
                    sh 'docker ps'
                    
                    // Test if we can access the API
                    sh 'curl -f http://localhost:5000/api/health || echo "API not accessible"'
                }
            }
        }
    }
    
    post {
        always {
            echo 'Test completed'
        }
    }
}