#include "LinearVectorKMedoids.h"
#include <cstring>
#include <omp.h>
#include <cassert>
#include <vector>
#include <cmath>
#include <limits>
#include <algorithm>
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/utils.h> 
#include <faiss/utils/distances.h>
#include <faiss/utils/random.h>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

int LinearVectorKMedoids::updateCentroids(
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
    
    // Adjust for frozen centroids
    size_t k_active = k - k_frozen;
    float* centroids_active = centroids + k_frozen * d;
    float* hassign_active = hassign + k_frozen;
    
    // Reset weights
    memset(hassign_active, 0, sizeof(float) * k_active);

    // Build inverted index for active clusters
    std::vector<std::vector<size_t>> cluster_members(k_active);
    if (k_active > 0) {
        size_t avg_n = n / k_active;
        for(auto& vec : cluster_members) vec.reserve(avg_n * 2); 
    }

    for (size_t i = 0; i < n; i++) {
        int64_t c = assignments[i];
        if (c >= (int64_t)k_frozen && c < (int64_t)k) {
            cluster_members[c - k_frozen].push_back(i);
        }
    }

    bool use_sampling = options.use_sampling;
    size_t sample_size = options.sample_size;
    size_t line_size = codec ? codec->sa_code_size() : d * sizeof(float);

    #pragma omp parallel for
    for (size_t ci = 0; ci < k_active; ci++) {
        const auto& members = cluster_members[ci];
        size_t Nc = members.size();

        float weight_sum = 0;
        if (weights) {
            for (auto idx : members) weight_sum += weights[idx];
        } else {
            weight_sum = (float)Nc;
        }
        hassign_active[ci] = weight_sum;

        if (Nc == 0) continue; 

        // --- Candidate Selection ---
        std::vector<size_t> candidates;
        if (use_sampling && Nc > sample_size) {
            candidates.reserve(sample_size);
            faiss::RandomGenerator rng(1234 + ci);
            
            int attempts = 0;
            while(candidates.size() < sample_size && attempts < sample_size * 2) {
                size_t rnd_idx = rng.rand_int64() % Nc;
                size_t real_idx = members[rnd_idx];
                
                bool dup = false;
                for(size_t ex : candidates) if(ex == real_idx) { dup = true; break; }
                
                if(!dup) candidates.push_back(real_idx);
                attempts++;
            }
            if (candidates.empty()) candidates.push_back(members[0]);
        } else {
            candidates = members;
        }

        // --- Evaluation ---
        double best_total_dist = std::numeric_limits<double>::max();
        size_t best_idx = members[0]; 

        std::vector<float> cand_vec(d);
        std::vector<float> member_vec(d);

        // Helper to retrieve vector
        auto get_vec = [&](size_t idx, float* out) {
            if (codec) {
                codec->sa_decode(1, x + idx * line_size, out);
            } else {
                memcpy(out, x + idx * line_size, d * sizeof(float));
            }
        };

        for (size_t cand_idx : candidates) {
            get_vec(cand_idx, cand_vec.data());

            double current_total_dist = 0;
            for (size_t member_idx : members) {
                get_vec(member_idx, member_vec.data());
                
                float dist = similarityComputer_->compute(cand_vec.data(), member_vec.data(), d);
                
                if (weights) {
                    current_total_dist += dist * weights[member_idx];
                } else {
                    current_total_dist += dist;
                }
            }
            if (current_total_dist < best_total_dist) {
                best_total_dist = current_total_dist;
                best_idx = cand_idx;
            }
        }

        // --- Update ---
        // For K-Medoids, the centroid is one of the data points.
        // We write the float values of that data point into the centroid buffer.
        get_vec(best_idx, centroids_active + ci * d);
    }

    // split_clusters and post_process operate on the full array, pass adjusted if needed
    // But consistent with KMeans, let's pass the full array and let it helper decide or pass adjusted?
    // KMeans helper takes k_frozen. KMedoids helper `split_clusters` signature needs check.
    // In previous file, KMedoids split_clusters signature:
    // int split_clusters(size_t k, size_t d, float* centroids, float* hassign)
    // It has no k_frozen logic.
    // Since we only want to split active clusters, pass adjusted pointers.
    
    int nsplit = split_clusters(k_active, d, centroids_active, hassign_active);
    post_process_centroids(k_active, d, centroids_active);

    return nsplit;
}

int LinearVectorKMedoids::split_clusters(size_t k, size_t d, float* centroids, float* hassign) {
    size_t nsplit = 0;

    for (size_t ci = 0; ci < k; ci++) {
        if (hassign[ci] == 0) {
            size_t parent_cluster = 0;
            float max_weight = -1;
            
            for (size_t cj = 0; cj < k; cj++) {
                if (hassign[cj] > max_weight) {
                    max_weight = hassign[cj];
                    parent_cluster = cj;
                }
            }
            memcpy(centroids + ci * d,
                   centroids + parent_cluster * d,
                   sizeof(float) * d);

            hassign[ci] = hassign[parent_cluster] / 2;
            hassign[parent_cluster] -= hassign[ci];
            nsplit++;
        }
    }
    return nsplit;
}

void LinearVectorKMedoids::post_process_centroids(size_t k, size_t d, float* centroids) {
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
