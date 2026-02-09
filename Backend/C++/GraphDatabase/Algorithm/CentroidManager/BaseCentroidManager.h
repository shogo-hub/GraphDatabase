#pragma once

#include "ICentroidManager.h"
#include "ClusteringTypes.h"
#include <vector>
#include <limits>
#include <omp.h>
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/distances.h>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

class BaseCentroidManager : public ICentroidManager {
protected:
    CentroidManagerOptions options;

public:
    BaseCentroidManager(const CentroidManagerOptions& opts) 
        : options(opts) {}

    virtual ~BaseCentroidManager() = default;

    void findClosestCentroids(
        size_t n, 
        size_t k,
        size_t d,
        const uint8_t* x,
        const faiss::Index* codec,
        const float* centroids_in,
        int64_t* assignments, 
        float* distances) override {
        
        size_t line_size = codec ? codec->sa_code_size() : d * sizeof(float);

        #pragma omp parallel
        {
            std::vector<float> decode_buf(d);
            
            #pragma omp for
            for (size_t i = 0; i < n; i++) {
                const float* xi;
                if (codec) {
                    codec->sa_decode(1, x + i * line_size, decode_buf.data());
                    xi = decode_buf.data();
                } else {
                    xi = reinterpret_cast<const float*>(x + i * line_size);
                }

                float min_dist = std::numeric_limits<float>::max();
                int64_t best_c = -1;

                for (size_t j = 0; j < k; j++) {
                    const float* c = centroids_in + j * d;
                    float dist = faiss::fvec_L2sqr(xi, c, d);
                    
                    if (dist < min_dist) {
                        min_dist = dist;
                        best_c = j;
                    }
                }

                assignments[i] = best_c;
                if (distances) distances[i] = min_dist;
            }
        }
    }
};

}
