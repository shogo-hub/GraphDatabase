from abc import ABC, abstractmethod
from typing import Dict, Any, Tuple


class ISimilarityScorer(ABC):
    """Interface for graph similarity scoring algorithms."""
    
    @abstractmethod
    def score(self, graph_a: Dict[str, Any], graph_b: Dict[str, Any], 
              parameters: Dict[str, Any] = None) -> Tuple[float, Dict[str, Any]]:
        """
        Calculate similarity score between two graphs.
        
        Args:
            graph_a: First graph as dict with 'nodes' and 'edges' keys
            graph_b: Second graph as dict with 'nodes' and 'edges' keys
            parameters: Algorithm-specific parameters
            
        Returns:
            Tuple of (similarity_score, metadata)
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