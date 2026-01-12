from .factory import IndexFactory

# Note: Concrete implementations to be added later
# from .hnsw_index import HNSWIndex
# from .ivf_index import IVFIndex

# Register algorithms with factory when implementations are ready
# IndexFactory.register_algorithm("HNSW", HNSWIndex)
# IndexFactory.register_algorithm("IVF", IVFIndex)

__all__ = ["IndexFactory"]