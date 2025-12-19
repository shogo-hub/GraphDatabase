from .factory import EmbeddingFactory

# Note: Concrete implementations to be added later
# from .node2vec import Node2VecGenerator
# from .graphsage import GraphSAGEGenerator

# Register algorithms with factory when implementations are ready
# EmbeddingFactory.register_algorithm("Node2Vec", Node2VecGenerator)
# EmbeddingFactory.register_algorithm("GraphSAGE", GraphSAGEGenerator)

__all__ = ["EmbeddingFactory"]