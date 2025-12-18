# Search Graph Management Design

This document describes how the system manages graph data and vector search indexes to support similarity search and graph-level comparisons. It expands the original outline with concrete flows, algorithm options, and integration points with the existing codebase.

## Overview
- Goal: Provide scalable CRUD for graphs and associated vector/ANN indexes with pluggable algorithms (Factory/Strategy), safe online updates, and clear observability.
- Non-goals: Choosing one permanent algorithm; instead we enable switching and side-by-side evaluation.

## Scope
- Manage graph artifacts (nodes, edges, attributes) and derived artifacts (embeddings, ANN indexes, whole-graph signatures).
- Expose Create/Read/Update/Delete for both primary graph data and search indexes.
- Support multiple similarity functions and search backends selected via configuration and DI.

## Terminology
- Graph: A collection of nodes and edges, optionally typed and attributed.
- Node/Edge: Entities with properties; edges may be directed.
- Embedding: Dense vector representing a node, subgraph, or whole graph.
- Similarity: A function for scoring two items (e.g., cosine, GED-based similarity).
- Index: ANN structure (e.g., HNSW, IVF) that accelerates nearest-neighbor search over embeddings.

## Architecture Integration
- Controllers: Handle HTTP, map DTOs, delegate to application layer via Mediator. See [Backend/Controllers](Backend/Controllers).
- Application layer: Implements commands/queries and orchestrates pipelines via interfaces. Mediator entry at [Backend/Common/Mediator/Mediator.cs](Backend/Common/Mediator/Mediator.cs).
- Errors: Consistent problem details and exception mapping at [Backend/Common/Errors](Backend/Common/Errors).
- AuthN/Z: Token-based authentication and policies at [Backend/Common/Authentication](Backend/Common/Authentication).
- Configuration: Algorithm and pipeline options come from [Backend/appsettings.json](Backend/appsettings.json) or environment overrides.

## Algorithm Choices

### Similarity Scoring (graph-level)
- Graph Edit Distance (GED)
    - Pros: Interpretable, exact (or bounded) structural comparison.
    - Cons: Expensive on large graphs; often approximated.
    - Use: Re-ranking a small candidate set; offline evaluation.
- SimGNN (neural graph similarity)
    - Pros: Learns task-specific similarity; faster inference after training.
    - Cons: Requires labeled pairs and training; generalization risk.
    - Use: Whole-graph similarity when training data is available.
- Other options
    - Weisfeiler–Lehman (WL) subtree kernels, Graph Isomorphism Networks (GIN) for embeddings; can complement or replace above depending on data.

### Vector Index / ANN
- HNSW (in-memory)
    - Pros: Very low latency, high recall, incremental inserts.
    - Cons: RAM heavy; challenging beyond memory limits.
    - Fit: Hot datasets, sub-10ms targets.
- IVF-Flat / IVF-PQ (quantized)
    - Pros: Good memory–speed trade-off; scalable training; shardable.
    - Cons: Requires training; PQ reduces accuracy unless re-ranked.
    - Fit: Large datasets; balanced latency and memory.
- Disk-based ANN (e.g., DiskANN or disk-backed HNSW)
    - Pros: Supports larger-than-RAM datasets.
    - Cons: Higher tail latency; careful tuning for I/O.
    - Fit: Massive corpora with cost constraints.

### Embedding / Graph–Vector Creation
- Node embeddings: Node2Vec/DeepWalk; inductive GraphSAGE.
- Subgraph/whole-graph embeddings: Readout over GNNs (e.g., GIN, GCN) or graph pooling.
- Text/code/multimodal: Use appropriate encoders, then align to the chosen index.

## Selection Mechanism (Factory/Strategy)
Define narrow interfaces and register concrete strategies with DI. Read active selection from configuration to instantiate at runtime.

- Interfaces (illustrative)
    - `ISimilarityScorer` → `Score(graphA, graphB)`
    - `IEmbeddingGenerator` → `Embed(node|subgraph|graph)`
    - `IVectorIndex` → `Build()`, `Insert()`, `Search(k)`, `Delete()`
- Factories
    - `ISimilarityFactory` → returns a scorer based on `Algorithms:Similarity:Type`.
    - `IIndexFactory` → returns an ANN implementation based on `Algorithms:Index:Type`.
- Configuration example (appsettings)
```json
{
    "Algorithms": {
        "Similarity": { "Type": "SimGNN", "ModelPath": "models/simgnn.onnx" },
        "Embedding": { "Type": "GraphSAGE", "Dim": 256 },
        "Index": {
            "Type": "HNSW",
            "HNSW": { "M": 32, "EfConstruction": 200, "EfSearch": 64 },
            "IVF": { "NLists": 4096, "PQ": { "M": 16, "Bits": 8 } },
            "Disk": { "NeighborFraction": 0.015, "IOThreads": 4 }
        }
    }
}
```

## CRUD Flows

### Create (ingest/build)
1) Validate and normalize input graph (IDs, types, schema).
2) Generate embeddings:
     - Node-level for node search; graph-level for whole-graph similarity.
3) Build or update the ANN index:
     - Bootstrap: bulk-build (HNSW/IVF/Disk) in a background job.
     - Warm path: incremental inserts with batching.
4) Persist metadata (version, parameters, metrics) and expose a read-only alias.
5) Health-check the new index; then atomically switch alias to the new version.

### Read (query/search)
- Inputs: query graph/node/vector; `k`, filters, scorer/index selection (optional overrides).
- Flow:
    1) If query is a graph/node, compute its embedding via `IEmbeddingGenerator`.
    2) Search `IVectorIndex` for top-k candidates.
    3) Optional re-ranking (e.g., GED/SimGNN) on the candidate set.
    4) Return results with scores, source, and version metadata.
- Latency targets: P50/P95 thresholds based on chosen index; enable efSearch / probes tuning per request or policy.

### Update (add/delete nodes/edges)
- Add node/edge:
    - Write to the primary store; derive/refresh embeddings for affected items.
    - Insert into ANN with micro-batches; backfill if transient failures occur.
- Delete node/edge:
    - Tombstone in the primary store; delete from ANN (or mark deleted and skip at read).
- Consistency:
    - Prefer eventual consistency for index with idempotent retries; provide rebuild tasks for drift.

### Delete (drop graph/index)
- Deactivate read alias; delete or archive index version and metadata.
- If needed, cascade delete embeddings and raw artifacts.

## Controllers and Application Layer
- Controllers receive DTOs, validate (model binding + FluentValidation if added), and dispatch to Mediator requests.
- Use command–query segregation:
    - Commands: Ingest graph, add node/edge, delete.
    - Queries: ANN search, graph similarity, metadata.
- See Mediator infrastructure at [Backend/Common/Mediator](Backend/Common/Mediator).

## Error Handling
- Use `ProblemDetails` via the helpers in [Backend/Common/Errors](Backend/Common/Errors) to ensure consistent HTTP errors.
- Common cases:
    - 400: schema validation failed, unknown algorithm type, incompatible parameters.
    - 404: graph/version not found.
    - 409: concurrent modification conflict on update.
    - 503: index warming/build in progress; include `Retry-After`.

## Security
- Token auth pipeline in [Backend/Common/Authentication](Backend/Common/Authentication).
- Recommend:
    - Scope-based authorization for mutating endpoints.
    - Optional result filtering by tenant/project.
    - Input size caps and timeouts for upload and GED re-ranking.

## Observability
- Metrics:
    - Build durations; recall@k (offline), latency (P50/P95), error rates.
    - Index version adoption and warmness.
- Logs:
    - Structured logs for pipeline stages and factory selections.
- Tracing:
    - Span around embed → ann → rerank with algorithm tags.

## Performance & Capacity
- Memory sizing for HNSW (vectors × edges per node × overhead).
- IVF/PQ codebook training schedule; re-train thresholds.
- Disk-based index prefetching and I/O concurrency tuning.
- Backpressure: queue limits and circuit breakers for re-ranking.

## Migration & Rollout Strategy
- Blue/green index versions with alias switching.
- Shadow mode evaluation: build alternative index/algorithm, compare offline/online before promoting.
- Safe rollback: keep N previous versions addressable for limited time.

## Open Questions
- Primary graph store: exact technology and schema (to be finalized).
- Training data availability for SimGNN/graph encoders.
- Multi-tenant isolation level (per-tenant index vs shared with filters).

---

## Original Summary (corrected)
- Create (make algorithm choice scalable via Factory pattern)
    - Choose similarity scoring algorithm (GED, SimGNN)
    - Choose searching algorithm (HNSW, IVF, Disk-based ANN)
    - Choose graph/vector creation algorithm (e.g., GraphSAGE, Node2Vec)
- Read
    - Execute search using selected index and optional re-ranking
- Update
    - Add/Delete nodes and edges; apply incremental index updates
- Delete
    - Remove graph and associated index versions safely
# General management
    - Create(Make algorithm choice scallable by Factory pattern)
        - Choose similarity scoreing algorithm(GED , simGNN)
        - Choose searching  algorithm (HNSW, IVF, Disk-based HNSW)
        - Choose algorithm  Graph/vector creation algorithm(not decided yet)
        - Create 
    - Read
        - Read searching algorithm 
    - Update
        Adding/Deleting  node on searching graph 
    - Delete
        Delete graph from node
    

