from typing import Dict, Any
from .base import ISimilarityScorer


class SimilarityFactory:
    """Factory for creating similarity scoring algorithm instances."""
    
    _algorithms: Dict[str, type] = {}
    
    @classmethod
    def register_algorithm(cls, name: str, algorithm_class: type) -> None:
        """Register a new similarity algorithm."""
        cls._algorithms[name] = algorithm_class
    
    @classmethod
    def create_scorer(cls, algorithm_name: str, **kwargs) -> ISimilarityScorer:
        """
        Create a similarity scorer instance.
        
        Args:
            algorithm_name: Name of the algorithm ("GED", "SimGNN", etc.)
            **kwargs: Additional arguments to pass to the algorithm constructor
            
        Returns:
            ISimilarityScorer instance
            
        Raises:
            ValueError: If algorithm_name is not registered
        """
        if algorithm_name not in cls._algorithms:
            available = list(cls._algorithms.keys())
            raise ValueError(f"Unknown similarity algorithm: {algorithm_name}. "
                           f"Available: {available}")
        
        algorithm_class = cls._algorithms[algorithm_name]
        return algorithm_class(**kwargs)
    
    @classmethod
    def list_algorithms(cls) -> Dict[str, str]:
        """List all registered algorithms with their class names."""
        return {name: cls.__name__ for name, cls in cls._algorithms.items()}
    
    @classmethod
    def get_algorithm_info(cls, algorithm_name: str) -> Dict[str, Any]:
        """Get information about a specific algorithm."""
        if algorithm_name not in cls._algorithms:
            raise ValueError(f"Unknown algorithm: {algorithm_name}")
        
        algorithm_class = cls._algorithms[algorithm_name]
        # Create a temporary instance to get default parameters
        temp_instance = algorithm_class()
        
        return {
            "name": algorithm_name,
            "class": algorithm_class.__name__,
            "default_parameters": temp_instance.get_default_parameters()
        }