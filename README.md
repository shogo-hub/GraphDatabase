# GraphDatabase

A graph-based vector database that combines graph similarity algorithms with fast approximate nearest neighbor search. This system enables storing, searching, and managing graph-structured data with high performance.

## Overview

This project implements a hybrid graph database system that provides:
- Graph data management with CRUD operations
- Vector similarity search using embeddings
- Multiple similarity algorithms (GNN, Graph Edit Distance, Graph2Vec)
- Multiple search backends (HNSW, IVF, exact search)
- RESTful API for graph operations
- Pluggable algorithm selection via configuration

## Tech Stack

### Backend - .NET (C#)
- **Language**: C# (net9.0)
- **SDK Version**: .NET 9.0.307
- **Framework**: ASP.NET Core Web API
- **Key Dependencies**:
  - MediatR 14.0.0 (CQRS pattern)
  - Paseto.Core 1.4.1 (Token-based authentication)
  - xUnit (Testing framework)

### Backend - Python (Flask)
- **Language**: Python 3.12.3
- **Framework**: Flask (planned/in development)
- **Purpose**: ML/AI services for graph embeddings and similarity computation

### CI/CD
- **GitHub Actions**: Automated testing on push
- **.NET Test Runner**: TRX format test results with artifact upload

## Project Structure

```
GraphDatabase/
├── Backend/
│   ├── Dotnet/                     # Main C# API service
│   │   ├── Program.cs              # Application entry point
│   │   ├── GraphDatabase.csproj    # Project configuration
│   │   ├── GraphDatabase.sln       # Solution file
│   │   ├── Controllers/            # HTTP controllers
│   │   ├── Application/            # Application layer (commands/queries)
│   │   │   └── AIChat/             # AI chat integration
│   │   ├── Common/                 # Shared utilities
│   │   │   ├── Authentication/     # Token auth, schemes
│   │   │   ├── Errors/             # Error handling & problem details
│   │   │   ├── Json/               # JSON serialization
│   │   │   ├── Mediator/           # CQRS mediator implementation
│   │   │   └── Miscellaneous/      # Result types (TryResult, Unit)
│   │   ├── Views/                  # Razor views (if needed)
│   │   ├── wwwroot/                # Static files
│   │   ├── Design.md               # Search graph design document
│   │   └── archtecture.md          # System architecture documentation
│   │
│   ├── Dotnet.Test/                # xUnit test project
│   │   ├── Dotnet.Test.csproj      # Test project configuration
│   │   └── UnitTest1.cs            # Sample unit tests
│   │
│   ├── flaskr/                     # Python Flask service (in development)
│   │   ├── requirements.txt        # Python dependencies
│   │   ├── extensions.py           # Flask extensions
│   │   ├── application/            # Business logic
│   │   ├── controllers/            # Flask controllers/routes
│   │   │   ├── auth.py
│   │   │   └── blog.py
│   │   └── services/               # AI/ML services
│   │       ├── embeddings/         # Graph embedding generation
│   │       ├── search/             # Search algorithms
│   │       └── similarity/         # Similarity computation
│   │
│   └── docker-compose.yml          # Docker orchestration (planned)
│
├── .github/
│   └── workflows/
│       └── test.yaml               # CI/CD pipeline for .NET tests
│
└── README.md                       # This file
```

## Prerequisites

### For .NET Development
- .NET SDK 9.0 or later
- A C# IDE (Visual Studio, VS Code, Rider)

### For Python Development
- Python 3.12 or later
- pip (Python package manager)

### For Docker (Optional)
- Docker Desktop or Docker Engine
- docker-compose

## Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd GraphDatabase
```

### 2. Run the .NET API

#### Build and Run
```bash
cd Backend/Dotnet
dotnet restore
dotnet build
dotnet run
```

The API will start on `http://localhost:5000` (or `https://localhost:5001` for HTTPS).

#### Run Tests
```bash
# From repository root
dotnet test Backend/Dotnet.Test/Dotnet.Test.csproj

# With detailed output
dotnet test Backend/Dotnet.Test/Dotnet.Test.csproj --logger "console;verbosity=detailed"

# Generate TRX test results
dotnet test Backend/Dotnet.Test/Dotnet.Test.csproj --logger trx --results-directory test-results
```

### 3. Run the Flask Service (Coming Soon)

```bash
cd Backend/flaskr
pip install -r requirements.txt
flask run
```

### 4. Using Docker (Planned)

```bash
docker-compose up --build
```

## Configuration

### .NET Application Settings
Configuration is managed through `appsettings.json` and `appsettings.Development.json` files in `Backend/Dotnet/`.

Key configuration areas:
- **Authentication**: Token schemes and policies
- **Algorithms**: Similarity and search algorithm selection
  - Similarity types: GNN, GED, Graph2Vec
  - Search indexes: HNSW, IVF, Flat (exact)
- **Error handling**: Problem details options

Example algorithm configuration:
```json
{
  "Algorithms": {
    "Similarity": { "Type": "SimGNN", "ModelPath": "models/simgnn.onnx" },
    "Embedding": { "Type": "GraphSAGE", "Dim": 256 },
    "Index": {
      "Type": "HNSW",
      "HNSW": { "M": 32, "EfConstruction": 200, "EfSearch": 64 }
    }
  }
}
```

## API Endpoints

### Graph Management (Planned)
- `POST /api/graphs` - Create a new graph
- `GET /api/graphs/{id}` - Retrieve a graph
- `PUT /api/graphs/{id}` - Update a graph
- `DELETE /api/graphs/{id}` - Delete a graph
- `POST /api/graphs/search` - Search for similar graphs

### Node Operations (Planned)
- `POST /api/graphs/{id}/nodes` - Add node to graph
- `DELETE /api/graphs/{id}/nodes/{nodeId}` - Remove node from graph

## Development

### Architecture Patterns
- **CQRS**: Commands and queries separated using MediatR
- **Factory/Strategy**: Pluggable algorithms selected via DI
- **Repository Pattern**: Data access abstraction
- **Problem Details**: RFC 7807 compliant error responses

### Key Components

#### Mediator (CQRS)
Custom mediator implementation at `Backend/Dotnet/Common/Mediator/`:
- `IMediator` - Main mediator interface
- `IRequest<TResponse>` - Request marker
- `IRequestHandler<TRequest, TResponse>` - Handler interface
- `IRequestHandlerDecorator<TRequest, TResponse>` - Decorator for cross-cutting concerns

#### Error Handling
Centralized error handling at `Backend/Dotnet/Common/Errors/`:
- Custom error types (ValidationFailedError, EntityNotFoundError, etc.)
- Problem details factory for consistent API responses
- See `Backend/Dotnet/Errors/README.md` for details

#### Authentication
Token-based authentication at `Backend/Dotnet/Common/Authentication/`:
- Paseto token support
- Known claim types
- Custom authorization policies (AdminOnlyAuthorizationPolicy)

### Testing
- **Framework**: xUnit
- **Test Project**: `Backend/Dotnet.Test/`
- **CI**: GitHub Actions runs tests automatically on push
- **Results**: TRX format uploaded as artifacts

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/test.yaml`):
1. Checkout code
2. Setup .NET SDK 9.0.x
3. Run `dotnet test` with TRX logger
4. Upload test results as artifacts (available even on failure)

## Roadmap

- [ ] Complete Flask service integration for ML/AI operations
- [ ] Implement graph CRUD endpoints
- [ ] Add vector similarity search endpoints
- [ ] Docker containerization with multi-service orchestration
- [ ] PostgreSQL + pgvector integration for persistence
- [ ] Redis caching layer
- [ ] Performance benchmarks and optimization
- [ ] SonarQube/Code quality integration in CI
- [ ] Python test suite with Bazel

## Documentation

- [Design Document](Backend/Dotnet/Design.md) - Search graph management design
- [Architecture](Backend/Dotnet/archtecture.md) - System architecture and MVP design
- [Error Handling](Backend/Dotnet/Errors/README.md) - Error handling patterns

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

[Specify your license here]

## Contact

[Your contact information or team information]
