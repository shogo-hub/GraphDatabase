# Flask Computation Service Design

This document describes the Flask service architecture that provides graph computation capabilities for the .NET middleware layer. Flask handles the computational heavy lifting while .NET manages HTTP middleware, authentication, database operations, and request routing.

## Overview
- **Goal**: Provide scalable graph computation services including similarity scoring, embedding generation, and vector search operations
- **Role**: Flask serves as the computation backend called by .NET middleware via HTTP APIs
- **Integration**: .NET handles all middleware concerns; Flask focuses purely on algorithms and computation

## Scope
- Implement graph similarity algorithms (GED, SimGNN)
- Generate node/graph embeddings (Node2Vec, GraphSAGE, GNN-based)
- Provide vector search capabilities (HNSW, IVF, disk-based ANN)
- Expose HTTP APIs for computation requests from .NET

## Architecture

### Division of Responsibilities

**Flask Computation Service:**
- Graph similarity scoring algorithms
- Embedding generation for nodes and graphs
- Vector index creation and search operations
- Algorithm factory patterns for pluggable implementations
- Pure computation with minimal middleware

**Dotnet Middleware:**
- HTTP request handling and routing
- Authentication and authorization
- Database schema and migrations
- Input validation and error handling
- Configuration management
- Problem details and consistent error responses

### Flask Project Structure

```
Backend/flaskr/
├── app.py                          # Flask app factory
├── config/
│   ├── __init__.py
│   ├── base.py                     # Base configuration
│   └── algorithms.py               # Algorithm selection config
├── services/                       # Core computation services
│   ├── __init__.py
│   ├── similarity/
│   │   ├── __init__.py
│   │   ├── base.py                 # ISimilarityScorer interface
│   │   ├── ged_scorer.py           # Graph Edit Distance implementation
│   │   ├── simgnn_scorer.py        # Neural graph similarity
│   │   └── factory.py              # Similarity algorithm factory
│   ├── embeddings/
│   │   ├── __init__.py
│   │   ├── base.py                 # IEmbeddingGenerator interface
│   │   ├── node2vec.py             # Node2Vec embeddings
│   │   ├── graphsage.py            # GraphSAGE embeddings
│   │   ├── gnn_embeddings.py       # GNN-based whole-graph embeddings
│   │   └── factory.py              # Embedding algorithm factory
│   └── search/
│       ├── __init__.py
│       ├── base.py                 # IVectorIndex interface
│       ├── hnsw_index.py           # In-memory HNSW implementation
│       ├── ivf_index.py            # Quantized IVF implementation
│       ├── disk_index.py           # Disk-based ANN implementation
│       └── factory.py              # Vector index factory
├── controllers/                    # REST API endpoints
│   ├── __init__.py
│   ├── similarity.py               # Similarity scoring endpoints
│   ├── embeddings.py               # Embedding generation endpoints
│   └── search.py                   # Vector search endpoints
├── models/                         # Data models and DTOs
│   ├── __init__.py
│   ├── graph.py                    # Graph data structures
│   ├── requests.py                 # API request models
│   └── responses.py                # API response models
└── utils/
    ├── __init__.py
    └── validation.py               # Input validation utilities
```

## API Endpoints

### Similarity Scoring
```http
POST /api/similarity/score
Content-Type: application/json

{
    "graphA": {
        "nodes": [{"id": 1, "properties": {...}}],
        "edges": [{"source": 1, "target": 2, "properties": {...}}]
    },
    "graphB": {
        "nodes": [{"id": 3, "properties": {...}}],
        "edges": [{"source": 3, "target": 4, "properties": {...}}]
    },
    "algorithm": "GED",  // or "SimGNN"
    "parameters": {
        "timeout_seconds": 30,
        "approximate": true
    }
}

Response:
{
    "similarity_score": 0.85,
    "algorithm_used": "GED",
    "computation_time_ms": 1250,
    "metadata": {
        "edit_distance": 3,
        "normalized": true
    }
}
```

### Embedding Generation
```http
POST /api/embeddings/generate
Content-Type: application/json

{
    "graph": {
        "nodes": [...],
        "edges": [...]
    },
    "target": "nodes",  // "nodes", "graph", or "subgraph"
    "algorithm": "Node2Vec",  // "Node2Vec", "GraphSAGE", "GNN"
    "parameters": {
        "dimensions": 256,
        "walk_length": 80,
        "num_walks": 10,
        "window_size": 10
    }
}

Response:
{
    "embeddings": {
        "1": [0.1, 0.2, ...],  // node_id -> vector
        "2": [0.3, 0.4, ...]
    },
    "algorithm_used": "Node2Vec",
    "dimensions": 256,
    "computation_time_ms": 5000
}
```

### Vector Search
```http
POST /api/search/vector
Content-Type: application/json

{
    "query_vector": [0.1, 0.2, 0.3, ...],
    "k": 10,
    "algorithm": "HNSW",  // "HNSW", "IVF", "Disk"
    "index_id": "graph_v1_embeddings",
    "parameters": {
        "ef_search": 64,
        "include_distances": true
    }
}

Response:
{
    "results": [
        {
            "id": "node_123",
            "distance": 0.15,
            "vector": [0.12, 0.18, ...]  // optional
        }
    ],
    "algorithm_used": "HNSW",
    "search_time_ms": 5
}
```

## Algorithm Factory Pattern

Each computation type uses a factory pattern for algorithm selection:

```python
# services/similarity/factory.py
class SimilarityFactory:
    @staticmethod
    def create_scorer(algorithm_type: str) -> ISimilarityScorer:
        if algorithm_type == "GED":
            return GEDScorer()
        elif algorithm_type == "SimGNN":
            return SimGNNScorer()
        else:
            raise ValueError(f"Unknown similarity algorithm: {algorithm_type}")
```

## Configuration

Algorithm selection and parameters are configured via environment variables or config files:

```python
# config/algorithms.py
SIMILARITY_ALGORITHMS = {
    "GED": {
        "class": "services.similarity.ged_scorer.GEDScorer",
        "default_params": {
            "timeout_seconds": 60,
            "approximate": True
        }
    },
    "SimGNN": {
        "class": "services.similarity.simgnn_scorer.SimGNNScorer", 
        "model_path": "models/simgnn.onnx",
        "default_params": {}
    }
}

EMBEDDING_ALGORITHMS = {
    "Node2Vec": {
        "class": "services.embeddings.node2vec.Node2VecGenerator",
        "default_params": {
            "dimensions": 128,
            "walk_length": 80,
            "num_walks": 10,
            "window_size": 10,
            "p": 1.0,
            "q": 1.0
        }
    },
    "GraphSAGE": {
        "class": "services.embeddings.graphsage.GraphSAGEGenerator",
        "default_params": {
            "dimensions": 256,
            "num_layers": 2,
            "aggregator": "mean"
        }
    }
}

VECTOR_INDEX_ALGORITHMS = {
    "HNSW": {
        "class": "services.search.hnsw_index.HNSWIndex",
        "default_params": {
            "M": 32,
            "ef_construction": 200,
            "ef_search": 64
        }
    },
    "IVF": {
        "class": "services.search.ivf_index.IVFIndex",
        "default_params": {
            "n_lists": 4096,
            "n_probes": 32
        }
    }
}
```

## Error Handling

Flask returns JSON error responses that .NET can map to ProblemDetails:

```python
# Standard error response format
{
    "error": {
        "type": "ValidationError",
        "title": "Invalid input parameters",
        "detail": "Graph must contain at least one node",
        "status": 400,
        "instance": "/api/similarity/score",
        "computation_id": "req_12345"
    }
}
```

Common error types:
- `ValidationError` (400): Invalid input format or parameters
- `AlgorithmError` (422): Algorithm-specific computation errors
- `TimeoutError` (408): Computation exceeded time limits
- `ResourceError` (503): Insufficient memory or resources

## Performance Considerations

- **Memory Management**: Use streaming for large graphs; implement memory limits per request
- **Timeouts**: Configurable timeouts for long-running computations
- **Caching**: Cache embeddings and intermediate results where appropriate
- **Concurrency**: Use asyncio for I/O-bound operations; threading for CPU-bound tasks
- **Batching**: Support batch operations for multiple graphs/embeddings

## Integration with .NET Middleware

1. **Request Flow**: 
   - .NET receives HTTP request
   - .NET validates authentication/authorization
   - .NET transforms DTO to Flask API format
   - .NET calls Flask computation service
   - .NET transforms Flask response to client format

2. **Configuration Synchronization**:
   - .NET reads algorithm configurations
   - .NET passes algorithm selection to Flask
   - Flask validates and applies algorithm parameters

3. **Error Propagation**:
   - Flask returns structured error responses
   - .NET maps to ProblemDetails format
   - .NET adds context (user info, request ID)

## Future Extensions

- **Model Management**: Support for loading/updating ML models (SimGNN, GNN embeddings)
- **Distributed Computing**: Scale computation across multiple Flask instances
- **GPU Acceleration**: CUDA support for embedding generation and similarity scoring
- **Streaming APIs**: WebSocket or SSE for long-running computations
- **Metrics Export**: Prometheus metrics for computation performance

## Dependencies

Core Python packages:
- `flask`: Web framework
- `numpy`: Numerical computations
- `networkx`: Graph data structures and basic algorithms
- `scikit-learn`: Machine learning utilities
- `faiss-cpu/faiss-gpu`: Vector similarity search
- `torch/tensorflow`: Neural network implementations (optional)
- `gensim`: Node2Vec implementation
- `onnxruntime`: Model inference (for SimGNN)

## Deployment Notes

- Flask runs as a separate service from .NET
- Communication via HTTP (consider gRPC for performance)
- Docker containers for both services
- Shared data volumes for large models/indexes
- Health checks and graceful shutdown handling
