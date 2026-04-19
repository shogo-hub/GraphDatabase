#pragma once

#include "ISimilarityComputer.h"
#include <faiss/utils/distances.h>

namespace GraphDatabase::Algorithm {

class L2SimilarityComputer : public ISimilarityComputer {
public:
    float compute(const float* a, const float* b, size_t d) const override {
        return faiss::fvec_L2sqr(a, b, d);
    }
};

} // namespace GraphDatabase::Algorithm
