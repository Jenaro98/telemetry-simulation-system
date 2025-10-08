pipeline {
    agent any
    
    stages {
        stage('Hello') {
            steps {
                echo 'Hello World!'
                echo 'Pipeline is working!'
            }
        }
        
        stage('Check Git') {
            steps {
                script {
                    sh 'pwd'
                    sh 'ls -la'
                    sh 'git --version'
                }
            }
        }
    }
    
    post {
        always {
            echo 'Pipeline completed'
        }
    }
}