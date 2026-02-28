#pragma once

#include <cstddef>

namespace GraphDatabase::Algorithm {

// Distance interface where lower = more similar (distance convention).
// Implementations wrap Faiss functions and must NOT be used
// in the assignment step (index.search handles that with SIMD).
class ISimilarityComputer {
public:
    virtual ~ISimilarityComputer() = default;

    virtual float compute(const float* a, const float* b, size_t d) const = 0;
};

} // namespace GraphDatabase::Algorithm
