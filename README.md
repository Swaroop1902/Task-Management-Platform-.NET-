# Task Management Platform

This is a full-stack Task Management Platform built with .NET 8 Microservices and an Angular 18 Frontend.

## Architecture

The system consists of three backend services and a frontend:

1. **User Service (Port 5001)**: Manages users and authentication. Exposes a simple login endpoint `/auth/login` to retrieve a stubbed JWT token.
2. **Task Service (Port 5002)**: Manages tasks and activity logs. Applies business logic around Task statuses, deadlines, and SLA breaches.
3. **Reporting Service (Port 5003)**: Connects to the User and Task services synchronously (using `HttpClient` and a simple `IMemoryCache` for performance) and aggregates tasks counts by user, by status, and lists SLA breaches.
4. **Angular Frontend (Port 4200)**: A responsive Single Page Application built with Angular 18 and Bootstrap 5. Provides Login, Dashboard, Task List, Task Details, and Reports views.

## Technical Choices

- **EF Core InMemory Database:** Used for rapid prototyping and container simplifications. Data resets on container restart.
- **Docker Compose:** Orchestrates all backend services alongside the Angular Nginx static file server.
- **Swagger / OpenAPI:** Built-in to all .NET Services explicitly in development mode.

## Running Locally (without Docker)

Prerequisites:
- .NET 8 SDK
- Node.js & npm

1. **Run User Service**: `cd TaskManagement.UserService && dotnet run`
2. **Run Task Service**: `cd TaskManagement.TaskService && dotnet run`
3. **Run Reporting Service**: `cd TaskManagement.ReportingService && dotnet run`
4. **Run Angular App**: `cd TaskManagement.Frontend && npm start`

## Running via Docker Compose (Recommended)

Make sure Docker Desktop is running.

```bash
cd TaskManagement
docker-compose up --build
```

Access the application at: **http://localhost:4200**

## Authentication

When navigating to the frontend or hitting the API, use the simple stub accounts:
- **Username:** `admin`, `manager`, or `engineer`
- **Password:** Same as username

### Testing Endpoints via Swagger

While the backend services are running, you can access the Swagger UI directly:
- User Service API: http://localhost:5001/swagger
- Task Service API: http://localhost:5002/swagger
- Reporting Service API: http://localhost:5003/swagger

To test authorized endpoints via Swagger:
1. Hit the `/auth/login` endpoint on `UserService` with `{ "username": "admin", "password": "admin" }`
2. Copy the resulting `token`.
3. In any other Swagger UI, click **Authorize** and input: `Bearer <token>`

## Tests

To run the boilerplate standard xUnit tests (or create more):
```bash
cd TaskManagement
dotnet test
```

## Known Limitations
- Data is stored in memory, so it is non-persistent across restarts.
- The Reporting service fetch logic synchronously calls other HTTP services. In a production environment with massive datasets, an asynchronous event-driven system (like RabbitMQ) propagating read-models to a distinct Read Database would be significantly more resilient and performant.
- Front-end HTTP interceptor handles token placement, but does not do sophisticated refresh-token flows as it is a stub backend.
