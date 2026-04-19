#pragma once

#include <vector>
#include <cstdint>
#include <cstddef>

namespace faiss { struct Index; }

namespace GraphDatabase::Algorithm {

class ICentroidManager {
public:
    virtual ~ICentroidManager() = default;

    // Assignment Step
    // TODO: Currently unused — Clustering::train_encoded() uses index.search() (SIMD path) for assignment.
    // This method is kept for a future refactor where index.search() is replaced with a
    // pluggable assignment step (separate branch). At that point, Clustering::train_encoded()
    // should call centroidManager->findClosestCentroids() instead of index.search().
    virtual void findClosestCentroids(
        size_t n,
        size_t k,
        size_t d,
        const uint8_t* x, 
        const faiss::Index* codec,
        const float* centroids,
        int64_t* assignments, 
        float* distances) = 0;

    // Update Step
    virtual int updateCentroids(
        size_t n, 
        size_t k,
        size_t d,
        float* hassign,
        const uint8_t* x, 
        const faiss::Index* codec,
        size_t k_frozen,
        float* centroids, 
        const int64_t* assignments, 
        const float* weights) = 0;
};

}
