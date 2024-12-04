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

### Run the service:
```bash
docker-compose up --build

---
```
### NotificationService Documentation
## Overview
NotificationService is a microservice responsible for handling email notifications and message-based communication within a project management platform. It facilitates sending emails to clients and freelancers and integrates with RabbitMQ for asynchronous messaging.

### Features
- **`Email Notifications`**
- **`Sends customized email notifications to users`*
Supports HTML-formatted email content.
Messaging Integration
Uses RabbitMQ for inter-service communication.
Queues:
UserNotificationQueue: Processes user-related notification requests.
NotificationUserQueue: Handles inter-service communication requests.
ResponseToNotificationQueue: Handles responses for notification actions.
API Endpoints
The NotificationService operates primarily through RabbitMQ queues and internal triggers. API endpoints are not directly exposed to external users.

### Messaging and Queues
RabbitMQ Queues
UserNotificationQueue:
Receives requests for sending notifications.
NotificationUserQueue:
Handles inter-service communication, such as retrieving user details for notifications.
ResponseToNotificationQueue:
Processes responses to notification-related actions.
Sample Message Formats
Request to NotificationUserQueue:
{
  "Action": "GetClientMail",
  "ClientId": 123,
  "CorrelationId": "unique-id"
}
Response from ResponseToNotificationQueue:
{
  "Action": "GetClientMail",
  "Mail": "client@example.com",
  "CorrelationId": "unique-id"
}
Service Components
1. EmailService
Handles the process of sending email notifications using SMTP.

### Key Method
SendEmailAsync(Notification notification)
Sends an email based on the provided Notification object.
Parameters:
Notification:
To: Recipient email address.
Subject: Email subject.
Message: Email body (supports HTML).
Handles common exceptions like invalid email formats and SMTP errors.
2. NotifyService
Orchestrates messaging between RabbitMQ and EmailService. It listens for messages and triggers the appropriate email notifications.

### Key Methods
StartListeningForMessages()

Listens for messages from RabbitMQ queues and processes them.
HandleResponseMessage(string message)

Processes incoming messages from ResponseToNotificationQueue based on the action type (e.g., Accept, CreateResponse).
SendEmailAsync(string mail, string message)

Sends an email using EmailService.
3. RabbitMqService
Manages interaction with RabbitMQ, including publishing and consuming messages.

### Key Methods
PublishAsync(string queueName, string message)
Publishes messages to the specified queue.
ListenForMessages(string queueName, Func<string, Task> onMessageReceived)
Consumes messages from the specified queue and processes them using the provided callback.
Configuration
appsettings.json
Stores configuration settings for database connections, RabbitMQ, and email services.

{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=project_management;user=root;password=root"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  },
  "Email": {
    "From": "example@domain.com",
    "Username": "username",
    "Password": "password",
    "SmtpServer": "smtp.server.com",
    "Port": "465"
  }
}
### Deployment
Prerequisites
Docker
RabbitMQ Server
MySQL Database
Docker Compose
Docker Compose
Define services in docker-compose.yml:

version: '3.8'
services:
  notification-service:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
Run the service:
docker-compose up --build
Database Schema
This service does not directly manage its database schema but integrates with user data through RabbitMQ messaging. External services provide user-related details when requested.


# RatingService Documentation
## Overview
The RatingService is responsible for handling project and freelancer reviews and responses in a freelance platform. It provides API endpoints for creating, retrieving, updating, and deleting reviews and responses. Additionally, it integrates with RabbitMQ for asynchronous messaging.

### Features
Response Management
Retrieve responses for clients, freelancers, and projects.
Create, update, and delete responses.
Accept responses with messaging integration.
Review Management
Retrieve reviews for freelancers and projects.
Create, update, and delete reviews.
Messaging Integration
Uses RabbitMQ for asynchronous communication.
Sends messages on response creation and acceptance.
## API Endpoints
### Response Endpoints
Get Client Response

GET /api/response/client/{clientId}
Retrieves responses associated with a client.
Responses:
200 OK: Returns client responses.
400 Bad Request: Invalid client ID.
404 Not Found: No responses found.
500 Internal Server Error: Server error.
Get Freelancer Response

GET /api/response/freelancer/{freelancerId}
Retrieves responses associated with a freelancer.
Responses:
200 OK: Returns freelancer responses.
400 Bad Request: Invalid freelancer ID.
404 Not Found: No responses found.
500 Internal Server Error: Server error.
Get Project Response

GET /api/response/project/{projectId}
Retrieves and deletes all responses associated with a project.
Responses:
200 OK: All responses deleted.
400 Bad Request: Invalid project ID.
404 Not Found: No responses found.
500 Internal Server Error: Server error.
Create Response

POST /api/response/create
Creates a new response.
Request Body:
{
  "clientId": int,
  "freelancerId": int,
  "projectId": int,
  "message": "string"
}
Responses:
200 OK: Response created.
400 Bad Request: Invalid data.
500 Internal Server Error: Server error.
Update Response

POST /api/response/update
Updates an existing response.
Request Body:
{
  "id": int,
  "message": "string",
  "status": "string"
}
Responses:
204 No Content: Response updated.
400 Bad Request: Invalid data.
404 Not Found: Response not found.
500 Internal Server Error: Server error.
Accept Response

POST /api/response/accept/{id}
Accepts a response and deletes others for the project.
Responses:
204 No Content: Response accepted.
400 Bad Request: Invalid response ID.
404 Not Found: Response not found.
500 Internal Server Error: Server error.
Delete Response

POST /api/response/delete/{id}
Deletes a specific response.
Responses:
204 No Content: Response deleted.
400 Bad Request: Invalid response ID.
404 Not Found: Response not found.
500 Internal Server Error: Server error.
Review Endpoints
Get Freelancer Reviews

GET /api/review/freelancer/{freelancerId}
Retrieves all reviews for a freelancer.
Responses:
200 OK: Returns reviews.
400 Bad Request: Invalid freelancer ID.
404 Not Found: No reviews found.
500 Internal Server Error: Server error.
Get Project Reviews

GET /api/review/project/{projectId}
Retrieves all reviews for a project.
Responses:
200 OK: Returns reviews.
400 Bad Request: Invalid project ID.
404 Not Found: No reviews found.
500 Internal Server Error: Server error.
Create Review

POST /api/review/create
Creates a new review.
Request Body:
{
  "freelancerId": int,
  "projectId": int,
  "comment": "string",
  "rating": float
}
Responses:
201 Created: Review created.
400 Bad Request: Invalid data.
500 Internal Server Error: Server error.
Update Review

POST /api/review/update
Updates an existing review.
Request Body:
{
  "id": int,
  "comment": "string",
  "rating": float
}
Responses:
204 No Content: Review updated.
400 Bad Request: Invalid data.
404 Not Found: Review not found.
500 Internal Server Error: Server error.
Delete Review

POST /api/review/delete/{reviewId}
Deletes a specific review.
Responses:
204 No Content: Review deleted.
400 Bad Request: Invalid review ID.
404 Not Found: Review not found.
500 Internal Server Error: Server error.
Database Schema
Tables
Reviews

Id: Primary Key
FreelancerId: Foreign Key
ProjectId: Foreign Key
ClientId: Foreign Key
Comment: String
Rating: Float
Responses

Id: Primary Key
FreelancerId: Foreign Key
ClientId: Foreign Key
ProjectId: Foreign Key
Message: String
Status: String
Messaging with RabbitMQ
Queues
ResponseQueue

Publishes messages for response actions.
Example Message:
{
  "Action": "CreateResponse",
  "CorrelationId": "unique-id",
  "ClientId": int
}
ReviewQueue

Handles review-related notifications.
### Configuration
appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=reviewdb;port=3306;database=reviewdb;user=root;password=root;"
  },
  "RabbitMQ": {
    "HostName": "rabbitmq",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest"
  }
}
### Deployment
Prerequisites
Docker
RabbitMQ Server
MySQL Database
Docker Compose
version: '3.8'
services:
  rating-service:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
    depends_on:
      - rabbitmq
      - db
  rabbitmq:
    image: rabbitmq:management
  db:
    image: mysql:8.0
Run the Service
docker-compose up --build

# UserManagementService Documentation

## Overview
**UserManagementService** is a microservice designed to handle user authentication, registration, and profile management for a freelance platform. It provides RESTful APIs for managing client and freelancer profiles and integrates with RabbitMQ for asynchronous messaging and notifications.

---

## Features

### **Authentication**
- **Login**: Authenticate users with credentials.
- **Registration**: Create new user accounts for clients and freelancers.

### **Profile Management**
- Retrieve and update profiles for both clients and freelancers.
- Fetch filtered lists of clients or freelancers.

### **Messaging Integration**
- Asynchronous communication through RabbitMQ:
  - Queues: `UserNotificationQueue` (outbound), `NotificationUserQueue` (inbound).

---

## API Endpoints

### **Authentication Endpoints**

#### `POST /api/auth/login`
- **Description**: Authenticates a user with email and password.
- **Request Body**:
  ```json
  {
    "email": "string",
    "passwordHash": "string"
  }
Responses:
200 OK: Authentication successful.
401 Unauthorized: Invalid email or password.
500 Internal Server Error: Server error.
POST /api/auth/register
Description: Registers a new user.
Request Body:###
{
  "username": "string",
  "email": "string",
  "passwordHash": "string",
  "role": "Client | Freelancer",
  "additionalInfo": {}
}
Responses:
201 Created: User registered successfully.
400 Bad Request: Invalid input or email already exists.
500 Internal Server Error: Server error.
Profile Endpoints
GET /api/profile/freelancer/{userId}
Description: Retrieves a freelancer's profile using their userId.
Responses:
200 OK: Profile retrieved successfully.
404 Not Found: No profile found for the provided ID.
GET /api/profile/client/{userId}
Description: Retrieves a client’s profile using their userId.
Responses:
200 OK: Profile retrieved successfully.
404 Not Found: No profile found for the provided ID.
GET /api/profile/freelancers
Description: Retrieves a list of freelancers excluding a specific userId.
Query Parameters:
userId (required): ID of the user to exclude.
Responses:
200 OK: List retrieved successfully.
GET /api/profile/clients
Description: Retrieves a list of clients excluding a specific userId.
Query Parameters:
userId (required): ID of the user to exclude.
Responses:
200 OK: List retrieved successfully.
### Database Schema
Users Table
Field	Type	Description
Id	Primary Key	Unique identifier
Email	String	Unique user email
PasswordHash	String	Encrypted password
Role	String	Client or Freelancer
FreelancerProfile Table
Field	Type	Description
UserId	Foreign Key	Links to Users.Id
Skills	String[]	Array of skills
Bio	String	Freelancer biography
ClientProfile Table
Field	Type	Description
UserId	Foreign Key	Links to Users.Id
CompanyName	String	Name of the company
Description	String	Client description
Messaging
### RabbitMQ Integration
Queues:
UserNotificationQueue: Publishes user-related notifications.
NotificationUserQueue: Receives requests for user-related actions.
Message Format Examples
Request to NotificationUserQueue:
{
  "Action": "GetFreelancerMail",
  "FreelancerId": 123,
  "CorrelationId": "unique-id"
}
Response from UserNotificationQueue:
{
  "Action": "GetFreelancerMail",
  "Mail": "freelancer@example.com",
  "CorrelationId": "unique-id"
}
### Configuration
appsettings.json
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
### Deployment
Prerequisites
Docker
RabbitMQ Server
MySQL Database
Docker Compose
Define services in docker-compose.yml:

version: '3.8'
services:
  user-management:
    build: .
    environment:
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION}
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
### Run the service:
docker-compose up --build
