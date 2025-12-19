from abc import ABC, abstractmethod
from typing import Dict, Any, List, Tuple
import numpy as np


class IVectorIndex(ABC):
    """Interface for vector similarity search algorithms."""
    
    @abstractmethod
    def build_index(self, vectors: np.ndarray, ids: List[str], 
                   parameters: Dict[str, Any] = None) -> None:
        """
        Build the vector index from a set of vectors.
        
        Args:
            vectors: Array of shape (n_vectors, dimensions)
            ids: List of identifiers for each vector
            parameters: Algorithm-specific parameters
        """
        pass
    
    @abstractmethod
    def search(self, query_vector: np.ndarray, k: int = 10, 
              parameters: Dict[str, Any] = None) -> Tuple[List[str], List[float]]:
        """
        Search for k nearest neighbors.
        
        Args:
            query_vector: Query vector of shape (dimensions,)
            k: Number of nearest neighbors to return
            parameters: Search-specific parameters
            
        Returns:
            Tuple of (neighbor_ids, distances)
        """
        pass
    
    @abstractmethod
    def add_vector(self, vector: np.ndarray, vector_id: str) -> None:
        """Add a single vector to the index."""
        pass
    
    @abstractmethod
    def remove_vector(self, vector_id: str) -> bool:
        """Remove a vector from the index. Returns True if found and removed."""
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
    def get_index_size(self) -> int:
        """Return the number of vectors in the index."""
        pass