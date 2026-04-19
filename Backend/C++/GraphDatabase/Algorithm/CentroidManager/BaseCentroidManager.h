#pragma once

#include "ICentroidManager.h"
#include "ClusteringTypes.h"
#include "../SimilarityComputer/ISimilarityComputer.h"
#include <memory>

namespace GraphDatabase::Algorithm {

class BaseCentroidManager : public ICentroidManager {
protected:
    CentroidManagerOptions options_;
    std::unique_ptr<ISimilarityComputer> similarityComputer_;

public:
    BaseCentroidManager(
        const CentroidManagerOptions& opts,
        std::unique_ptr<ISimilarityComputer> similarityComputer)
        : options_(opts)
        , similarityComputer_(std::move(similarityComputer)) {}

    virtual ~BaseCentroidManager() = default;

    void findClosestCentroids(
        size_t n,
        size_t k,
        size_t d,
        const uint8_t* x,
        const faiss::Index* codec,
        const float* centroids_in,
        int64_t* assignments,
        float* distances) override;
};

} // namespace GraphDatabase::Algorithm
