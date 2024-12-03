# riat  
# UserManagementService Documentation

## Overview
**UserManagementService** is a microservice responsible for user authentication, registration, and profile management within a freelance platform. It provides APIs for client and freelancer profile operations and integrates with RabbitMQ for messaging and notifications.

---

## Features

### Authentication
- **Login**: Validates user credentials and returns details.
- **Registration**: Creates new user accounts for clients or freelancers.

### Profile Management
- Retrieve and update client and freelancer profiles.
- Fetch lists of clients or freelancers excluding specific users.

### Messaging Integration
- Uses RabbitMQ for asynchronous communication.
  - Queues: `UserNotificationQueue`, `NotificationUserQueue`.

---

## API Endpoints

### Authentication Endpoints

#### `POST /api/auth/login`
- **Description**: Authenticates users by email and password.
- **Request Body**:
  ```json
  {
    "email": "string",
    "passwordHash": "string"
  }
  ```
- **Responses**:
  - `200 OK`: User authenticated successfully.
  - `401 Unauthorized`: Invalid credentials.
  - `500 Internal Server Error`: Server error.

#### `POST /api/auth/register`
- **Description**: Registers a new user.
- **Request Body**:
  ```json
  {
    "username": "string",
    "email": "string",
    "passwordHash": "string",
    "role": "Client | Freelancer",
    "additionalInfo": "object"
  }
  ```
- **Responses**:
  - `200 OK`: User registered successfully.
  - `400 Bad Request`: Invalid input or user already exists.
  - `500 Internal Server Error`: Server error.

### Profile Endpoints

#### `GET /api/profile/freelancer/{userId}`
- **Description**: Retrieves a freelancer’s profile by `userId`.
- **Responses**:
  - `200 OK`: Profile retrieved.
  - `404 Not Found`: Profile does not exist.

#### `GET /api/profile/client/{userId}`
- **Description**: Retrieves a client’s profile by `userId`.
- **Responses**:
  - `200 OK`: Profile retrieved.
  - `404 Not Found`: Profile does not exist.

#### `GET /api/profile/freelancers`
- **Description**: Retrieves a list of freelancers, excluding a specific `userId`.
- **Query Parameters**:
  - `userId` (required): ID of the user to exclude.
- **Responses**:
  - `200 OK`: List of freelancers.

#### `GET /api/profile/clients`
- **Description**: Retrieves a list of clients, excluding a specific `userId`.
- **Query Parameters**:
  - `userId` (required): ID of the user to exclude.
- **Responses**:
  - `200 OK`: List of clients.

---

## Database Schema
- **Users**
  - `Id`: Primary Key
  - `Email`: Unique user email
  - `PasswordHash`: Encrypted password
  - `Role`: `Client` or `Freelancer`
- **FreelancerProfile**
  - `UserId`: Foreign Key to Users
  - `Skills`: String array
  - `Bio`: String
- **ClientProfile**
  - `UserId`: Foreign Key to Users
  - `CompanyName`: String
  - `Description`: String

---

## Messaging

### RabbitMQ Queues
- **`UserNotificationQueue`**
  - Publishes notifications to users.
- **`NotificationUserQueue`**
  - Receives requests for user-related actions.

### Sample Message Formats
#### Request to `NotificationUserQueue`:
```json
{
  "Action": "GetFreelancerMail",
  "FreelancerId": 123,
  "CorrelationId": "unique-id"
}
```

#### Response from `UserNotificationQueue`:
```json
{
  "Action": "GetFreelancerMail",
  "Mail": "freelancer@example.com",
  "CorrelationId": "unique-id"
}
```


## Configuration

### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=userdb;port=3306;database=userdb;user=root;password=root;"
  },
  "Jwt": {
    "Key": "supersecretkey",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

---

## Deployment

### Prerequisites
- Docker
- RabbitMQ Server
- MySQL Database

### Docker Compose
Define services in `docker-compose.yml`:
```yaml
version: '3.8'
services:
  user-management:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
```

Run the service:
```bash
docker-compose up --build
```

---

## Testing

### Unit Tests
Use `xUnit` to test service logic and controllers:
- Test login and registration scenarios.
- Validate message handling logic for RabbitMQ.

### Integration Tests
- Use `TestServer` for HTTP API testing.
- Mock RabbitMQ messages to verify asynchronous workflows.

---

## Logging

### Log Levels
- Default: `Information`
- For development: Increase to `Debug` or `Trace`.

Example Log Entry:
```
[INFO] User with email user@example.com authenticated successfully.
```

# ProjectManagementService Documentation

## Overview
**ProjectManagementService** is responsible for managing projects within the freelance platform. This includes creating, updating, retrieving, and deleting projects. It also integrates with RabbitMQ for asynchronous messaging to support communication with other microservices.

---

## Features

### Project Operations
- **Create**: Add new projects to the system.
- **Update**: Modify project details.
- **Retrieve**: Fetch details of projects by various criteria.
- **Delete**: Remove projects from the system.

### Messaging Integration
- Uses RabbitMQ for event-driven interactions.
  - Queues: `ProjectQueue`, `ResponseToProjectQueue`, `ProjectResponseQueue`.

---

## API Endpoints

### Project Management Endpoints

#### `POST /api/project/create`
- **Description**: Creates a new project.
- **Request Body**:
  ```json
  {
    "title": "string",
    "description": "string",
    "clientId": "integer",
    "freelancerId": "integer | null",
    "budget": "number"
  }
  ```
- **Responses**:
  - `201 Created`: Project created successfully.
  - `400 Bad Request`: Invalid input.
  - `500 Internal Server Error`: Server error.

#### `PUT /api/project/update`
- **Description**: Updates an existing project.
- **Request Body**:
  ```json
  {
    "projectId": "integer",
    "title": "string | null",
    "description": "string | null",
    "freelancerId": "integer | null",
    "budget": "number | null",
    "status": "string | null"
  }
  ```
- **Responses**:
  - `200 OK`: Project updated successfully.
  - `404 Not Found`: Project not found.
  - `500 Internal Server Error`: Server error.

#### `GET /api/project/{projectId}`
- **Description**: Retrieves details of a specific project by its `projectId`.
- **Responses**:
  - `200 OK`: Project details retrieved.
  - `404 Not Found`: Project not found.

#### `DELETE /api/project/{projectId}`
- **Description**: Deletes a project by its `projectId`.
- **Responses**:
  - `204 No Content`: Project deleted successfully.
  - `404 Not Found`: Project not found.
  - `500 Internal Server Error`: Server error.

### Listing Endpoints

#### `GET /api/project/client/{clientId}`
- **Description**: Retrieves all projects for a specific client.
- **Responses**:
  - `200 OK`: List of projects.
  - `404 Not Found`: No projects found.

#### `GET /api/project/freelancer/{freelancerId}`
- **Description**: Retrieves all projects assigned to a specific freelancer.
- **Responses**:
  - `200 OK`: List of projects.
  - `404 Not Found`: No projects found.

---

## Database Schema

### Projects Table
- **Id**: Primary Key
- **Title**: Title of the project
- **Description**: Detailed project description
- **ClientId**: Foreign Key to Users (Client role)
- **FreelancerId**: Foreign Key to Users (Freelancer role, nullable)
- **Budget**: Project budget
- **Status**: Status of the project (e.g., Open, Assigned, Completed)
- **CreatedAt**: Timestamp of creation
- **UpdatedAt**: Timestamp of last update

---

## Messaging

### RabbitMQ Queues
- **`ProjectQueue`**
  - Listens for project-related actions (Create, Update, Delete).
- **`ResponseToProjectQueue`**
  - Receives messages from other services regarding projects.
- **`ProjectResponseQueue`**
  - Publishes responses for project-related queries.

### Sample Message Formats

#### Message to `ProjectQueue`:
```json
{
  "Action": "Create",
  "CorrelationId": "unique-id",
  "ProjectDetails": {
    "title": "New Project",
    "clientId": 123,
    "budget": 5000
  }
}
```

#### Response from `ProjectResponseQueue`:
```json
{
  "Action": "Create",
  "Status": "Success",
  "ProjectId": 456,
  "CorrelationId": "unique-id"
}
```

## Configuration

### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=proj_db;port=3306;database=project_management;user=root;password=root"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
```

## Deployment

### Prerequisites
- Docker
- RabbitMQ Server
- MySQL Database

### Docker Compose
Define services in `docker-compose.yml`:
```yaml
version: '3.8'
services:
  project-management:
    build: .
    ports:
      - "5001:5001"
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
```

Run the service:
```bash
docker-compose up --build

---


