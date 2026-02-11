#pragma once

#include "../../BaseCentroidManager.h"

namespace GraphDatabase::Algorithm {

class LinearVectorKMeans : public BaseCentroidManager {
public:
    LinearVectorKMeans(const CentroidManagerOptions& opts) 
        : BaseCentroidManager(opts) {}

    int updateCentroids(
        size_t n, 
        size_t k,
        size_t d,
        float* hassign,
        const uint8_t* x,
        const faiss::Index* codec,
        size_t k_frozen,
        float* centroids, 
        const int64_t* assignments, 
        const float* weights) override;

private:
    // Helper functionality internally used
    int split_clusters(
        size_t d,
        size_t k,
        size_t n,
        size_t k_frozen,
        float* hassign,
        float* centroids);

    void post_process_centroids(
        size_t k,
        size_t d,
        float* centroids);
};

}
