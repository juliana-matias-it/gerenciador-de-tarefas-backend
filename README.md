# Task Manager API

REST API for task management built with **.NET 10**, **ASP.NET Core**, **Entity Framework Core**, and **SQLite**.

This repository is my personal portfolio version of a collaborative project originally developed during the **WoMakersCode bootcamp**. The original project was built with a squad, and this fork preserves that collaborative history while extending the application for learning and portfolio purposes.

## About the project

The API allows users to register and manage their own tasks.

Each task belongs to a specific user and can be created, updated, completed, listed, or deleted.

The project follows a layered architecture to keep responsibilities separated:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
Entity Framework Core
    ↓
SQLite
```

## Technologies

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger / OpenAPI
- xUnit
- Git and GitHub

## Architecture

The application is organized into the following main layers:

```text
Controllers/
    UsuarioController.cs
    TarefaController.cs

Services/
    Interfaces/
    UsuarioService.cs
    TarefaService.cs

Repositories/
    Interfaces/
    UsuarioRepository.cs
    TarefaRepository.cs

Models/
    Usuario.cs
    Tarefa.cs

DTOs/
    CadastrarUsuarioDto.cs
    UsuarioDto.cs
    CriarTarefaDto.cs
    AtualizarTarefaDto.cs
    TarefaDto.cs

Data/
    AppDbContext.cs

Middleware/
    ExceptionMiddleware.cs

Migrations/

tests/
    GerenciadorDeTarefas.Tests/
```

## Main features

### Users

- Register a new user
- Get a user by ID
- Prevent duplicate e-mail registration
- Generate unique IDs using `Guid`
- Prevent passwords from being exposed in API responses

### Tasks

- List tasks by user
- Get a task by ID
- Create a task
- Update a task
- Mark a task as completed
- Delete a task
- Prevent duplicate task titles for the same user
- Validate task ownership
- Validate due dates
- Prevent users from modifying tasks that belong to another user

## DTOs

DTOs are used to separate the API contract from the domain entities.

For example, user responses expose:

```json
{
  "id": "guid",
  "nome": "User Name",
  "email": "user@example.com"
}
```

The password is never returned by the API.

Task creation and update DTOs also prevent clients from directly modifying properties such as:

- `UsuarioId`
- `Usuario`
- `Status`
- `Id`

## Exception handling

The API uses a global `ExceptionMiddleware` to centralize error handling.

Controllers do not contain business exception `try/catch` blocks.

Examples of standardized responses:

### Business conflict

```json
{
  "statusCode": 409,
  "message": "E-mail já cadastrado."
}
```

### Resource not found

```json
{
  "statusCode": 404,
  "message": "Tarefa não encontrada."
}
```

### Unexpected error

```json
{
  "statusCode": 500,
  "message": "Ocorreu um erro interno no servidor."
}
```

Internal application details are not exposed in unexpected error responses.

## API endpoints

### Users

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/usuarios` | Register a user |
| `GET` | `/api/usuarios/{id}` | Get a user by ID |

### Tasks

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/usuarios/{usuarioId}/tarefas` | List user tasks |
| `GET` | `/api/usuarios/{usuarioId}/tarefas/{id}` | Get a specific task |
| `POST` | `/api/usuarios/{usuarioId}/tarefas` | Create a task |
| `PUT` | `/api/usuarios/{usuarioId}/tarefas/{id}` | Update a task |
| `PATCH` | `/api/usuarios/{usuarioId}/tarefas/{id}` | Mark a task as completed |
| `DELETE` | `/api/usuarios/{usuarioId}/tarefas/{id}` | Delete a task |

## Business rules

The service layer is responsible for business rules such as:

- A user e-mail must be unique.
- A task must belong to an existing user.
- A user cannot have two tasks with the same title.
- New tasks start with the `Pendente` status.
- A task can only be modified by its owner.
- Updating a task cannot change its owner.
- The due date must be later than the current date.
- Completing a task changes its status to `Concluida`.

## Tests

The project currently has **63 automated tests** covering services, controllers, and global exception handling.

The test suite includes scenarios such as:

- User registration
- Duplicate e-mail validation
- User lookup
- Task creation
- Duplicate tasks
- Due date validation
- Task ownership
- Task updates
- Task completion
- Task deletion
- HTTP status codes
- DTO responses
- Password protection
- Global exception handling

Run the tests with:

```bash
dotnet test ./tests/GerenciadorDeTarefas.Tests
```

Current result:

```text
63 tests passed
```

## Running the project

Clone the repository:

```bash
git clone https://github.com/juliana-matias-it/gerenciador-de-tarefas-backend.git
```

Enter the project directory:

```bash
cd gerenciador-de-tarefas-backend
```

Restore dependencies:

```bash
dotnet restore
```

Apply the database migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

Swagger will be available through the development URL displayed in the terminal.

## Database

The project uses **SQLite** with Entity Framework Core.

Connection string:

```text
Data Source=gerenciador-tarefas.db
```

The local database file is ignored by Git and can be recreated using the Entity Framework migrations.

## Project background

This project originated as a collaborative squad activity during the **WoMakersCode bootcamp**.

The original repository contains contributions from multiple developers. This fork preserves that history and serves as my personal version for continued development and portfolio presentation.

My contributions and extensions in this version include work on:

- Entity Framework Core configuration
- Database migrations
- Repository implementation
- Service layer and business rules
- REST controllers
- DTOs
- Global exception handling
- Automated tests
- API structure and integration

## Author

**Juliana Matias**

Software Developer focused on backend development, APIs, .NET, and modern web technologies.

GitHub: [juliana-matias-it](https://github.com/juliana-matias-it)