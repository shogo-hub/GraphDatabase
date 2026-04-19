#pragma once

#include "ISimilarityComputer.h"
#include <faiss/utils/distances.h>

namespace GraphDatabase::Algorithm {

// Negated inner product so that lower = more similar,
// consistent with the distance convention used in findClosestCentroids
// and KMedoids updateCentroids.
class InnerProductSimilarityComputer : public ISimilarityComputer {
public:
    float compute(const float* a, const float* b, size_t d) const override {
        return -faiss::fvec_inner_product(a, b, d);
    }
};

} // namespace GraphDatabase::Algorithm
