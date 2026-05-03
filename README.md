# SMS Notification Microservice

Enterprise-grade SMS & Notification microservice built using ASP.NET Core 8, Clean Architecture, RabbitMQ, Redis, Docker, and Kubernetes.

---

## Features

-OTP Generation & Validation
-SMS Notification Processing
-Queue-based asynchronous processing using RabbitMQ
-Redis caching for OTP throttling and performance optimization
-Polly retry and resilience policies
-Background worker services
-Clean Architecture implementation
-Swagger API documentation
-Docker containerization
-Kubernetes deployment support
-Structured logging and exception handling
-Decorator pattern implementation
-Dead-letter queue (DLQ) support

---

## Tech Stack

| Technology     | Usage                   |
| -------------- | ----------------------- |
| ASP.NET Core 8 | REST API                |
| C#             | Backend Development     |
| RabbitMQ       | Message Queue           |
| Redis          | Distributed Cache       |
| PostgreSQL     | Database                |
| Dapper         | Data Access             |
| Polly          | Retry Policies          |
| Docker         | Containerization        |
| Kubernetes     | Container Orchestration |
| Swagger        | API Documentation       |


---

## Architecture

                +------------------+
                |   Client / UI    |
                +------------------+
                          |
                          v
                +------------------+
                |  SMS API Layer   |
                | ASP.NET Core API |
                +------------------+
                          |
                          v
                +----------------------+
                | Application Layer    |
                | Validation Decorator |
                | Logging Decorator    |
                | Cache Decorator      |
                +----------------------+
                          |
                          v
                +----------------------+
                | Infrastructure Layer |
                | Dapper / Redis       |
                | RabbitMQ Publisher   |
                +----------------------+
                          |
             +------------+------------+
             |                         |
             v                         v
     +---------------+       +----------------+
     | RabbitMQ Queue|       | Redis Cache    |
     +---------------+       +----------------+
             |
             v
     +----------------------+
     | Background Workers   |
     | SMS Worker Service   |
     +----------------------+
             |
             v
     +----------------------+
     | Notification Gateway |
     +----------------------+

---

  ## Project Structure

SmsNotification.API              --> API Layer
SmsNotification.APPLICATION      --> Business Logic
SmsNotification.DOMAIN           --> Domain Entities
SmsNotification.INFRASTRUCTURE   --> External Services & Repositories
SmsNotification.SMSWORKER        --> Background Queue Consumers
k8s                              --> Kubernetes Deployment Files
docs                             --> Architecture & Documentation

---

## Clean Architecture

Implemented layered architecture for:

-Separation of concerns
-Maintainability
-Scalability
-Testability

---

## Decorator Pattern

Implemented decorators for:

Request validation
Structured logging
Redis caching

---

## Queue-Based Processing

RabbitMQ used for:

Asynchronous processing
Message reliability
Queue retry handling
Dead-letter queue support

---

## Distributed Caching

Redis caching implemented for:

OTP throttling
Performance optimization
Request validation

---

## Retry & Resilience

Implemented Polly retry policies for:

External API calls
RabbitMQ retry handling
Fault tolerance

---

## Docker Support
Build Docker Image
docker build -t smsapi:v1 .
docker build -t smsworker:v1 .

---

## Kubernetes Deployment

Kubernetes manifests available inside:
/k8s
kubectl apply -f k8s/

---

## Security Note

Sensitive credentials, production URLs, and confidential configurations are excluded from this repository.

---

## Author
Nawab Sajjad Ali

Senior .NET Developer | Backend Engineer | Microservices Enthusiast

GitHub:
https://github.com/NawabSajjad
LinkedIn:
https://linkedin.com/in/nawab-sajjad-ali/

---

## License

This project is licensed under the MIT License.

