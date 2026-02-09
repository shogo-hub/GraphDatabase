/**
 * Enum for concrete clustering class 
 * **/
#pragma once

#include <cstddef>

namespace GraphDatabase::Algorithm {

enum class ClusteringAlgorithmType {
    K_MEANS,
    K_MEDOIDS
};

struct CentroidManagerOptions {
    // K-Medoids specific
    bool use_sampling = true;
    size_t sample_size = 256;
    
    // Post-processing
    bool spherical = false;
    bool int_centroids = false;
};

}
