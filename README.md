# SMS Notification Microservice

Enterprise-grade SMS & Notification microservice built using ASP.NET Core 8, Clean Architecture, RabbitMQ, Redis, Docker, and Kubernetes.

---

## Features

- OTP Generation & Validation
- SMS Notification Processing
- Queue-based asynchronous processing using RabbitMQ
- Redis caching for OTP throttling and performance optimization
- Polly retry and resilience policies
- Background worker services
- Clean Architecture implementation
- Swagger API documentation
- Docker containerization
- Kubernetes deployment support
- Structured logging and exception handling

---

## Tech Stack

- ASP.NET Core 8 Web API
- C#
- RabbitMQ
- Redis
- PostgreSQL
- Dapper
- Polly
- Docker
- Kubernetes
- Swagger / OpenAPI

---

## Architecture

```text
Client Request
      |
      v
API Layer
      |
Application Layer
      |
Infrastructure Layer
      |
RabbitMQ Queue
      |
Background Worker
      |
SMS Provider
