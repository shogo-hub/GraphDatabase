#pragma once

#include "../../BaseCentroidManager.h"

namespace GraphDatabase::Algorithm {

class LinearVectorKMedoids : public BaseCentroidManager {
public:
    LinearVectorKMedoids(
        const CentroidManagerOptions& opts,
        std::unique_ptr<ISimilarityComputer> similarityComputer)
        : BaseCentroidManager(opts, std::move(similarityComputer)) {}

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
   // Helpers
   int split_clusters(
       size_t k,
       size_t d,
       float* centroids,
       float* hassign);

   void post_process_centroids(
       size_t k,
       size_t d,
       float* centroids);
};

}
