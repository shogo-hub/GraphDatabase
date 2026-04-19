#include "LinearVectorKMeans.h"
#include <cstring>
#include <omp.h>
#include <cassert>
#include <vector>
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/utils.h>
#include <faiss/utils/random.h>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

#define EPS (1 / 1024.)

int LinearVectorKMeans::updateCentroids(
    size_t n, 
    size_t k,
    size_t d,
    float* hassign,
    const uint8_t* x,
    const faiss::Index* codec,
    size_t k_frozen,
    float* centroids,
    const int64_t* assignments,
    const float* weights) {

    // 1. Adjust for frozen centroids
    FAISS_THROW_IF_NOT(k_frozen <= k);
    k -= k_frozen;
    centroids += k_frozen * d;
    
    // 2. Clear memory
    memset(centroids, 0, sizeof(float) * d * k);

    size_t line_size = codec ? codec->sa_code_size() : d * sizeof(float);

    // --- Accumulation Step ---
    #pragma omp parallel
    {
        // Get thread number
        int nt = omp_get_num_threads();
        // Get current thread id
        int rank = omp_get_thread_num();
        // Separate thread by cluster unit
        size_t c0 = (k * rank) / nt;
        size_t c1 = (k * (rank + 1)) / nt;
        std::vector<float> decode_buf(d);

        for (size_t i = 0; i < n; i++) {
            int64_t ci = assignments[i];
            // Adjust index relative to non-frozen part
            // Note: assignments are global indices [0, k_original-1]
            // We need to check if ci falls into the active range [k_frozen, k_original)
            // But verify: Does Faiss re-index or just mask?
            // The original code says:
            // int64_t ci = assign[i];
            // assert(ci >= 0 && ci < k + k_frozen);
            // ci -= k_frozen;
            // if (ci >= c0 && ci < c1) ...
            
            // So yes, we assume assignments are absolute indices.
            
            if (ci >= (int64_t)k_frozen) {
                int64_t ci_active = ci - k_frozen;
                
                if (ci_active >= (int64_t)c0 && ci_active < (int64_t)c1) {
                    float* c = centroids + ci_active * d;
                    const float* xi;

                    if (codec) {
                        codec->sa_decode(1, x + i * line_size, decode_buf.data());
                        xi = decode_buf.data();
                    } else {
                        xi = reinterpret_cast<const float*>(x + i * line_size);
                    }
                    
                    if (weights) {
                        float w = weights[i];
                        hassign[ci_active] += w;
                        for (size_t j = 0; j < d; j++) {
                            c[j] += xi[j] * w;
                        }
                    } else {
                        hassign[ci_active] += 1.0f;
                        for (size_t j = 0; j < d; j++) {
                            c[j] += xi[j];
                        }
                    }
                }
            }
        }
    }

    // --- Normalization ---
    #pragma omp parallel for
    for (int64_t ci = 0; ci < (int64_t)k; ci++) {
        if (hassign[ci] != 0) {
            float norm = 1.0f / hassign[ci];
            float* c = centroids + ci * d;
            for (size_t j = 0; j < d; j++) {
                c[j] *= norm;
            }
        }
    }

    // --- Split Clusters ---
    // split_clusters internally re-adjusts for k_frozen, so restore
    // the original k and centroids pointer before passing.
    int nsplit = split_clusters(d, k + k_frozen, n, k_frozen, hassign, centroids - k_frozen * d);

    return nsplit;
}

int LinearVectorKMeans::split_clusters(
    size_t d,
    size_t k,
    size_t n, 
    size_t k_frozen,
    float* hassign,
    float* centroids) {
    
    k -= k_frozen;
    centroids += k_frozen * d;

    size_t nsplit = 0;
    faiss::RandomGenerator rng(1234);

    for (size_t ci = 0; ci < k; ci++) {
        if (hassign[ci] == 0) {
            size_t cj;
            for (cj = 0; true; cj = (cj + 1) % k) {
                float p = (hassign[cj] - 1.0f) / (float)(n - k);
                float r = rng.rand_float();
                if (r < p) break;
            }
            memcpy(centroids + ci * d,
                   centroids + cj * d,
                   sizeof(float) * d);

            for (size_t j = 0; j < d; j++) {
                if (j % 2 == 0) {
                    centroids[ci * d + j] *= 1.0f + EPS;
                    centroids[cj * d + j] *= 1.0f - EPS;
                } else {
                    centroids[ci * d + j] *= 1.0f - EPS;
                    centroids[cj * d + j] *= 1.0f + EPS;
                }
            }
            hassign[ci] = hassign[cj] / 2;
            hassign[cj] -= hassign[ci];
            nsplit++;
        }
    }
    return nsplit;
}

}
