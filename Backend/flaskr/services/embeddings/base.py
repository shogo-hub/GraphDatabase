from abc import ABC, abstractmethod
from typing import Dict, Any, List, Union


class IEmbeddingGenerator(ABC):
    """Interface for graph/node embedding generation algorithms."""
    
    @abstractmethod
    def generate_embeddings(self, graph: Dict[str, Any], target: str = "nodes", 
                          parameters: Dict[str, Any] = None) -> Dict[str, Any]:
        """
        Generate embeddings for nodes or entire graph.
        
        Args:
            graph: Graph as dict with 'nodes' and 'edges' keys
            target: "nodes", "graph", or "subgraph"
            parameters: Algorithm-specific parameters
            
        Returns:
            Dict containing embeddings and metadata
        """
        pass
    
    @abstractmethod
    def get_algorithm_name(self) -> str:
        """Return the algorithm name."""
        pass
    
    @abstractmethod
    def get_default_parameters(self) -> Dict[str, Any]:
        """Return default parameters for this algorithm."""
        pass
    
    @abstractmethod
    def get_embedding_dimensions(self) -> int:
        """Return the dimensionality of generated embeddings."""
        pass