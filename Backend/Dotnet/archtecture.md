# Graph Vector Database - Architecture Design

## Overview

A graph-based vector database that combines graph similarity algorithms with fast approximate nearest neighbor search. This system enables storing, searching, and managing graph-structured data with high performance.

**MVP Version**: Django Monolith (Single Service)

---

## System Architecture (MVP - Django Monolith)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                 Clients                                      │
│                    (REST API / SDK / CLI)                                   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Django Application                                  │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                     Django REST Framework (API)                        │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────────────┐ │  │
│  │  │    Auth     │  │   Routing   │  │      Rate Limiting (optional)   │ │  │
│  │  └─────────────┘  └─────────────┘  └─────────────────────────────────┘ │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                      Graph Manager Service                             │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │  │
│  │  │   Create    │  │   Search    │  │  Add Node   │  │ Delete Node │   │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────┐  ┌───────────────────────────────────┐   │
│  │   Similarity Algorithm Layer  │  │      Search Algorithm Layer       │   │
│  │  ┌───────┐ ┌───────┐ ┌──────┐│  │  ┌────────┐ ┌───────┐ ┌────────┐  │   │
│  │  │  GNN  │ │  GED  │ │Graph ││  │  │  HNSW  │ │  IVF  │ │  Flat  │  │   │
│  │  │       │ │       │ │2Vec  ││  │  │        │ │       │ │ (Exact)│  │   │
│  │  └───────┘ └───────┘ └──────┘│  │  └────────┘ └───────┘ └────────┘  │   │
│  └───────────────────────────────┘  └───────────────────────────────────┘   │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                       Graph Loader (Lazy Load)                         │  │
│  │                   In-Memory Graph Cache (LRU)                          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Persistence Layer                                   │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────────┐  │
│  │   PostgreSQL    │  │     Redis       │  │      File System            │  │
│  │   + pgvector    │  │   (Hot Cache)   │  │   (Graph Snapshots)         │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Django Project Structure (MVP)

```
graph_vector_db/
├── manage.py
├── requirements.txt
├── docker-compose.yml
├── Dockerfile
│
├── config/                          # Project configuration
│   ├── __init__.py
│   ├── settings.py
│   ├── urls.py
│   └── wsgi.py
│
├── graphs/                          # Main application
│   ├── __init__.py
│   ├── models.py                    # Graph, Node, Edge models
│   ├── serializers.py               # DRF serializers
│   ├── views.py                     # API views
│   ├── urls.py                      # URL routing
│   ├── admin.py
│   │
│   ├── services/                    # Business logic
│   │   ├── __init__.py
│   │   ├── graph_manager.py         # Graph CRUD operations
│   │   └── graph_loader.py          # Lazy loading logic
│   │
│   ├── algorithms/                  # Algorithm implementations
│   │   ├── __init__.py
│   │   ├── base.py                  # Abstract interfaces
│   │   ├── similarity/
│   │   │   ├── __init__.py
│   │   │   ├── gnn.py               # GNN implementation
│   │   │   ├── ged.py               # Graph Edit Distance
│   │   │   └── graph2vec.py         # Graph2Vec
│   │   │
│   │   └── search/
│   │       ├── __init__.py
│   │       ├── hnsw.py              # HNSW implementation
│   │       ├── ivf.py               # IVF implementation
│   │       └── flat.py              # Exact search
│   │
│   └── persistence/                 # Storage backends
│       ├── __init__.py
│       ├── base.py                  # Abstract interface
│       ├── postgresql.py
│       ├── redis_backend.py
│       └── file_backend.py
│
└── tests/
    ├── __init__.py
    ├── test_graphs.py
    ├── test_algorithms.py
    └── test_search.py
```

---

## API Design

### 1. Create Graph

Create a new graph with specified algorithms and persistence configuration.

```
POST /api/v1/graphs
```

#### Request Body

```json
{
  "name": "my-knowledge-graph",
  "similarity_algorithm": "gnn",
  "search_algorithm": "hnsw",
  "persistence": {
    "type": "postgresql",
    "connection_string": "postgresql://..."
  },
  "config": {
    "embedding_dimension": 128,
    "hnsw_m": 16,
    "hnsw_ef_construction": 200
  }
}
```

#### Parameters

| Parameter | Type | Required | Options | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | ✅ | - | Unique identifier for the graph |
| `similarity_algorithm` | string | ✅ | `gnn`, `ged`, `graph2vec` | Algorithm to compute graph/node similarity |
| `search_algorithm` | string | ✅ | `hnsw`, `ivf`, `flat` | Fast search algorithm for ANN |
| `persistence.type` | string | ✅ | `postgresql`, `redis`, `file`, `memory` | Where to store the graph |
| `config` | object | ❌ | - | Algorithm-specific configuration |

#### Response

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "my-knowledge-graph",
  "status": "created",
  "created_at": "2025-12-14T10:30:00Z",
  "metadata": {
    "similarity_algorithm": "gnn",
    "search_algorithm": "hnsw",
    "persistence_type": "postgresql",
    "node_count": 0
  }
}
```

#### Flow Diagram

```
┌────────┐     ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Client │────▶│ API Gateway │────▶│  ML Engine   │────▶│ Persistence │
└────────┘     └─────────────┘     └──────────────┘     └─────────────┘
    │                                     │
    │          POST /graphs               │
    │─────────────────────────────────────│
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Validate    │
    │                              │ Parameters  │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Initialize  │
    │                              │ Algorithm   │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Create      │
    │                              │ Index       │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Save to     │
    │                              │ Persistence │
    │                              └──────┬──────┘
    │                                     │
    │◀────────────────────────────────────│
    │           201 Created               │
```

---

### 2. Search Similar Nodes

Search for nodes similar to a query node or embedding.

```
POST /api/v1/graphs/{graph_name}/search
```

#### Request Body

```json
{
  "query": {
    "type": "node",
    "data": {
      "features": [0.1, 0.2, 0.3, ...],
      "edges": [[0, 1], [1, 2]]
    }
  },
  "top_k": 10,
  "threshold": 0.8
}
```

#### Alternative: Search by Embedding

```json
{
  "query": {
    "type": "embedding",
    "vector": [0.1, 0.2, 0.3, ...]
  },
  "top_k": 10
}
```

#### Response

```json
{
  "results": [
    {
      "node_id": "node-001",
      "similarity_score": 0.95,
      "metadata": { "label": "Entity A" }
    },
    {
      "node_id": "node-002", 
      "similarity_score": 0.89,
      "metadata": { "label": "Entity B" }
    }
  ],
  "query_time_ms": 12,
  "graph_loaded": true
}
```

#### Flow Diagram (with Lazy Loading)

```
┌────────┐     ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Client │────▶│ API Gateway │────▶│  ML Engine   │────▶│ Persistence │
└────────┘     └─────────────┘     └──────────────┘     └─────────────┘
    │                                     │
    │     POST /graphs/{name}/search      │
    │─────────────────────────────────────│
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Check Graph │
    │                              │ in Memory?  │
    │                              └──────┬──────┘
    │                                     │
    │                         ┌───────────┴───────────┐
    │                         │                       │
    │                    [Not Loaded]            [Loaded]
    │                         │                       │
    │                  ┌──────▼──────┐               │
    │                  │ Load from   │               │
    │                  │ Persistence │               │
    │                  └──────┬──────┘               │
    │                         │                       │
    │                  ┌──────▼──────┐               │
    │                  │ Build Index │               │
    │                  │ in Memory   │               │
    │                  └──────┬──────┘               │
    │                         │                       │
    │                         └───────────┬───────────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Generate    │
    │                              │ Query Embed │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ HNSW/IVF    │
    │                              │ Search      │
    │                              └──────┬──────┘
    │                                     │
    │◀────────────────────────────────────│
    │         Search Results              │
```

---

### 3. Add Node

Add a new node to an existing graph.

```
POST /api/v1/graphs/{graph_name}/nodes
```

#### Request Body

```json
{
  "node_id": "node-new-001",
  "features": [0.1, 0.2, 0.3, ...],
  "edges": [
    { "target": "node-002", "weight": 0.8 },
    { "target": "node-005", "weight": 0.6 }
  ],
  "metadata": {
    "label": "New Entity",
    "type": "person",
    "created_by": "user-123"
  }
}
```

#### Response

```json
{
  "node_id": "node-new-001",
  "status": "added",
  "embedding": [0.05, 0.12, ...],
  "index_updated": true
}
```

#### Flow Diagram

```
┌────────┐     ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Client │────▶│ API Gateway │────▶│  ML Engine   │────▶│ Persistence │
└────────┘     └─────────────┘     └──────────────┘     └─────────────┘
    │                                     │
    │     POST /graphs/{name}/nodes       │
    │─────────────────────────────────────│
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Validate    │
    │                              │ Node Data   │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Generate    │
    │                              │ Embedding   │
    │                              │ (GNN/GED)   │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Update      │
    │                              │ Search Index│
    │                              │ (HNSW/IVF)  │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Persist     │
    │                              │ to Storage  │
    │                              └──────┬──────┘
    │                                     │
    │◀────────────────────────────────────│
    │           201 Created               │
```

---

### 4. Delete Node

Remove a node from the graph.

```
DELETE /api/v1/graphs/{graph_name}/nodes/{node_id}
```

#### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `cascade` | boolean | ❌ | Delete connected edges (default: true) |
| `rebuild_index` | boolean | ❌ | Rebuild search index after deletion (default: false) |

#### Response

```json
{
  "node_id": "node-001",
  "status": "deleted",
  "edges_removed": 3,
  "index_status": "marked_deleted"
}
```

#### Flow Diagram

```
┌────────┐     ┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ Client │────▶│ API Gateway │────▶│  ML Engine   │────▶│ Persistence │
└────────┘     └─────────────┘     └──────────────┘     └─────────────┘
    │                                     │
    │  DELETE /graphs/{name}/nodes/{id}   │
    │─────────────────────────────────────│
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Check Node  │
    │                              │ Exists      │
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Remove from │
    │                              │ Search Index│
    │                              │ (Mark/Delete)│
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Remove Edges│
    │                              │ (if cascade)│
    │                              └──────┬──────┘
    │                                     │
    │                              ┌──────▼──────┐
    │                              │ Delete from │
    │                              │ Persistence │
    │                              └──────┬──────┘
    │                                     │
    │◀────────────────────────────────────│
    │           200 OK                    │
```

---

## Additional APIs

### 5. Delete Graph

```
DELETE /api/v1/graphs/{graph_name}
```

### 6. List Graphs

```
GET /api/v1/graphs
```

### 7. Get Graph Info

```
GET /api/v1/graphs/{graph_name}
```

### 8. Reload Graph (Force)

```
POST /api/v1/graphs/{graph_name}/reload
```

---

## Component Details

### Similarity Algorithm Layer

| Algorithm | Use Case | Pros | Cons |
|-----------|----------|------|------|
| **GNN** (Graph Neural Network) | Large graphs, learned similarity | Scalable, captures complex patterns | Requires training |
| **GED** (Graph Edit Distance) | Small graphs, exact matching | Interpretable, exact | O(n!) complexity |
| **Graph2Vec** | Document-like graphs | Fast, unsupervised | Less precise |

### Search Algorithm Layer

| Algorithm | Use Case | Pros | Cons |
|-----------|----------|------|------|
| **HNSW** | High recall needed | Best recall, fast | High memory |
| **IVF** | Memory constrained | Lower memory | Slower, less recall |
| **Flat** | Small datasets, exact | 100% recall | O(n) search |

### Persistence Options

| Type | Use Case | Pros | Cons |
|------|----------|------|------|
| **PostgreSQL + pgvector** | Production, ACID needed | Reliable, SQL queries | Slower than Redis |
| **Redis** | Hot cache, real-time | Very fast | Volatile |
| **File System** | Snapshots, backup | Simple, portable | No query capability |
| **Memory** | Testing, development | Fastest | Lost on restart |

---

## Data Models

### Graph Metadata

```json
{
  "id": "uuid",
  "name": "string",
  "similarity_algorithm": "gnn | ged | graph2vec",
  "search_algorithm": "hnsw | ivf | flat",
  "persistence_type": "postgresql | redis | file | memory",
  "embedding_dimension": 128,
  "node_count": 10000,
  "edge_count": 50000,
  "created_at": "datetime",
  "updated_at": "datetime",
  "loaded_in_memory": true,
  "index_status": "ready | building | stale"
}
```

### Node

```json
{
  "id": "string",
  "graph_id": "uuid",
  "features": [0.1, 0.2, ...],
  "embedding": [0.05, 0.12, ...],
  "metadata": {},
  "created_at": "datetime"
}
```

### Edge

```json
{
  "source_id": "string",
  "target_id": "string",
  "weight": 0.8,
  "type": "string",
  "metadata": {}
}
```

---

## Technology Stack (MVP)

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Web Framework** | Django 5.x | API, ORM, Admin |
| **API** | Django REST Framework | REST API endpoints |
| **Graph Algorithms** | PyTorch Geometric, NetworkX | GNN, GED |
| **Vector Search** | FAISS, hnswlib | HNSW, IVF |
| **Database** | PostgreSQL + pgvector | Persistence |
| **Cache** | Redis (optional) | Hot graph cache |
| **Task Queue** | Celery (optional) | Async processing |
| **Container** | Docker | Deployment |

---

## Python Dependencies

```txt
# requirements.txt

# Django
Django>=5.0
djangorestframework>=3.14
django-cors-headers>=4.3

# Database
psycopg2-binary>=2.9
pgvector>=0.2

# Graph & ML
torch>=2.0
torch-geometric>=2.4
networkx>=3.2
numpy>=1.26
scikit-learn>=1.3

# Vector Search
faiss-cpu>=1.7       # or faiss-gpu for GPU support
hnswlib>=0.8

# Graph Embeddings
karateclub>=1.3      # for Graph2Vec

# Cache (optional)
redis>=5.0
django-redis>=5.4

# Utilities
python-dotenv>=1.0
gunicorn>=21.0

# Development
pytest>=7.4
pytest-django>=4.7
```

---

## Error Handling

### Error Response Format

```json
{
  "error": {
    "code": "GRAPH_NOT_FOUND",
    "message": "Graph 'my-graph' does not exist",
    "details": {},
    "timestamp": "2025-12-14T10:30:00Z"
  }
}
```

### Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `GRAPH_NOT_FOUND` | 404 | Graph does not exist |
| `NODE_NOT_FOUND` | 404 | Node does not exist |
| `GRAPH_ALREADY_EXISTS` | 409 | Graph name conflict |
| `INVALID_ALGORITHM` | 400 | Unknown algorithm specified |
| `EMBEDDING_DIMENSION_MISMATCH` | 400 | Query dimension mismatch |
| `GRAPH_LOADING` | 503 | Graph is being loaded |
| `INDEX_BUILDING` | 503 | Index is being rebuilt |

---

## Future Considerations

- [ ] Batch node insertion API
- [ ] Graph versioning
- [ ] Distributed graph sharding
- [ ] Real-time graph updates via WebSocket
- [ ] Graph visualization endpoints
- [ ] Export/Import functionality (GraphML, JSON)
- [ ] Query language for complex graph searches
- [ ] Migrate to hybrid architecture (ASP.NET Gateway + Python ML Engine) for scale

---

## Future Architecture (Post-MVP)

When scaling beyond MVP, consider splitting into microservices:

```
┌─────────────────────┐      ┌─────────────────────┐
│   ASP.NET Core      │      │   Django/FastAPI    │
│   (API Gateway)     │ ───▶ │   (ML Engine)       │
│   - Auth            │      │   - Algorithms      │
│   - Rate Limiting   │      │   - Search          │
│   - Caching         │      │   - Embeddings      │
└─────────────────────┘      └─────────────────────┘
```

---

## Appendix: Configuration Examples

### HNSW Configuration

```json
{
  "hnsw_m": 16,
  "hnsw_ef_construction": 200,
  "hnsw_ef_search": 50
}
```

### GNN Configuration

```json
{
  "model": "GraphSAGE",
  "layers": 3,
  "hidden_dim": 64,
  "aggregator": "mean"
}
```
