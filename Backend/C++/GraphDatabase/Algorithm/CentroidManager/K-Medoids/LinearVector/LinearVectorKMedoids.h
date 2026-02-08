#pragma once

#include <cstddef>
#include <cstdint>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

class LinearVectorKMedoids {
public:
    /**
     * @brief K-Medoids centroid computation
     * 
     * Selects new medoids for each cluster.
     * Can use a full search (checking all points) or a sampling-based approximation (CLARA-like)
     * which is much faster for large datasets.
     */
    static void compute_centroids(
        size_t d,
        size_t k,
        size_t n,
        size_t k_frozen,
        const uint8_t* x,
        const faiss::Index* codec,
        const int64_t* assign,
        const float* weights,
        float* hassign,
        float* centroids,
        bool use_sampling = true,
        size_t sample_size = 256);

    /**
     * @brief 空クラスタの分割処理
     * 
     * K-Medoids では「データを少しずらす」ことができないため、
     * 最大のクラスタからランダムにデータ点を選んで新しいメドイドにするなどの処理を行います。
     */
    static int split_clusters(
        size_t d,
        size_t k,
        size_t n,
        size_t k_frozen,
        float* hassign,
        float* centroids);

    /**
     * @brief 後処理
     * 
     * K-Medoids ではデータ点そのものを使うため、正規化などは基本的に不要ですが、
     * インターフェースを合わせるために用意します。
     */
    static void post_process_centroids(
        size_t d,
        size_t k,
        float* centroids,
        bool spherical,
        bool int_centroids);
};

}
