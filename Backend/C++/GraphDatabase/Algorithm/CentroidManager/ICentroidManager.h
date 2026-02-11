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
