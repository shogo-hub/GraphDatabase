#include "LinearVectorKMeans.h"
#include <cstring>
#include <omp.h>
#include <cassert>
#include <vector>
#include <cmath>
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/utils.h> 
#include <faiss/utils/random.h>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

#define EPS (1 / 1024.)

void LinearVectorKMeans::updateCentroids(
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
    k -= k_frozen;
    centroids += k_frozen * d;
    
    // 2. Clear memory
    memset(centroids, 0, sizeof(float) * d * k);

    size_t line_size = codec ? codec->sa_code_size() : d * sizeof(float);

    // --- Accumulation Step ---
    #pragma omp parallel
    {
        // Get therad number
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
    // Note: split_clusters receives the original k but also k_frozen to adjust internally?
    // The snippet says: split_clusters(size_t d, size_t k, size_t n, size_t k_frozen...) 
    // and inside: k -= k_frozen; centroids += k_frozen * d;
    // So we should pass the original pointers and counts, or consistent adjusted ones.
    // The snippet signature takes original k and k_frozen. 
    // We already adjusted local k and centroids in this function. 
    // To call the function correctly as per snippet, we pass original values 
    // (re-calculating original pointer for centroids is centroids - k_frozen*d).
    // Or simpler: just pass the adjusted ones if we change the helper signature? 
    // But I declared the helper to take k_frozen. So I pass original.
    
    split_clusters(d, k + k_frozen, n, k_frozen, hassign, centroids - k_frozen * d);

    // --- Post Process ---
    // Pass adjusted k and centroids?
    // post_process usually acts on all centroids? Or just active ones?
    // The snippet: post_process_centroids(size_t d, size_t k, float* centroids...)
    // Likely we only process the ones we updated? Or all?
    // User snippet implementation doesn't check k_frozen.
    // So pass the calculated active range.
    post_process_centroids(k, d, centroids);
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

void LinearVectorKMeans::post_process_centroids(size_t k, size_t d, float* centroids) {
    if (options.spherical) {
        faiss::fvec_renorm_L2(d, k, centroids);
    }
    if (options.int_centroids) {
        for (size_t i = 0; i < d * k; i++) {
            centroids[i] = roundf(centroids[i]);
        }
    }
}

}
