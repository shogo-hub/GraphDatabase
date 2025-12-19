from .factory import SimilarityFactory

# Note: Concrete implementations to be added later
# from .ged_scorer import GEDScorer
# from .simgnn_scorer import SimGNNScorer

# Register algorithms with factory when implementations are ready
# SimilarityFactory.register_algorithm("GED", GEDScorer)
# SimilarityFactory.register_algorithm("SimGNN", SimGNNScorer)

__all__ = ["SimilarityFactory"]