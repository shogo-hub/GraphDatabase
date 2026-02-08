#include "LinearVectorKMeans.h"
#include <cstring>
#include <omp.h>
#include <cassert>
#include <vector>
#include <cmath>
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/utils.h> 
#include <faiss/utils/random.h>

namespace GraphDatabase::Algorithm {

// a bit above machine epsilon for float16
#define EPS (1 / 1024.)

void LinearVectorKMeans::compute_centroids(
        size_t d,
        size_t k,
        size_t n,
        size_t k_frozen,
        const uint8_t* x,
        const faiss::Index* codec,
        const int64_t* assign,
        const float* weights,
        float* hassign,
        float* centroids) {
    
    // 1. 更新数の調整
    // 凍結された k_frozen 個は更新しないため、その分ポインタを進め、処理する数 k を減らす
    k -= k_frozen;
    centroids += k_frozen * d;
    
    // 2. メモリの初期化
    // 重心データを蓄積するためのメモリを0で埋める
    memset(centroids, 0, sizeof(*centroids) * d * k);

    // データ1つあたりのバイトサイズ（ストライド）を計算
    // codecがある場合は圧縮サイズ、そうでなければ d * floatサイズ
    size_t line_size = codec ? codec->sa_code_size() : d * sizeof(float);

#pragma omp parallel
    {
        int nt = omp_get_num_threads();
        int rank = omp_get_thread_num();

        // 並列化戦略:
        // ベクトル(n)を分割するのではなく、スレッドごとに担当する重心(k)の範囲を決める
        // これにより、書き込み時の競合（ロック）を回避できる
        size_t c0 = (k * rank) / nt;
        size_t c1 = (k * (rank + 1)) / nt;
        std::vector<float> decode_buffer(d);

        // 全ベクトルをスキャンする
        for (size_t i = 0; i < n; i++) {
            int64_t ci = assign[i];
            assert(ci >= 0 && ci < k + k_frozen);
            ci -= k_frozen;
            
            // このベクトルが、現在のスレッドの担当する重心範囲 [c0, c1) に属する場合のみ処理
            if (ci >= c0 && ci < c1) {
                float* c = centroids + ci * d;
                const float* xi;
                
                // データの取得（必要ならデコード）
                if (!codec) {
                    xi = reinterpret_cast<const float*>(x + i * line_size);
                } else {
                    float* xif = decode_buffer.data();
                    codec->sa_decode(1, x + i * line_size, xif);
                    xi = xif;
                }
                
                // 重心への加算
                if (weights) {
                    float w = weights[i];
                    hassign[ci] += w;
                    for (size_t j = 0; j < d; j++) {
                        c[j] += xi[j] * w;
                    }
                } else {
                    hassign[ci] += 1.0;
                    for (size_t j = 0; j < d; j++) {
                        c[j] += xi[j];
                    }
                }
            }
        }
    }

    // 3. 正規化
    // 合計値を個数（重み合計）で割って、平均（重心）を求める
#pragma omp parallel for
    for (int64_t ci = 0; ci < (int64_t)k; ci++) {
        if (hassign[ci] == 0) {
            continue;
        }
        float norm = 1 / hassign[ci];
        float* c = centroids + ci * d;
        for (size_t j = 0; j < d; j++) {
            c[j] *= norm;
        }
    }
}

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

    /* Take care of void clusters */
    size_t nsplit = 0;
    faiss::RandomGenerator rng(1234);
    for (size_t ci = 0; ci < k; ci++) {
        if (hassign[ci] == 0) { /* need to redefine a centroid */
            size_t cj;
            for (cj = 0; true; cj = (cj + 1) % k) {
                /* probability to pick this cluster for split */
                float p = (hassign[cj] - 1.0) / (float)(n - k);
                float r = rng.rand_float();
                if (r < p) {
                    break; /* found our cluster to be split */
                }
            }
            memcpy(centroids + ci * d,
                   centroids + cj * d,
                   sizeof(*centroids) * d);

            /* small symmetric perturbation */
            for (size_t j = 0; j < d; j++) {
                if (j % 2 == 0) {
                    centroids[ci * d + j] *= 1 + EPS;
                    centroids[cj * d + j] *= 1 - EPS;
                } else {
                    centroids[ci * d + j] *= 1 - EPS;
                    centroids[cj * d + j] *= 1 + EPS;
                }
            }

            /* assume even split of the cluster */
            hassign[ci] = hassign[cj] / 2;
            hassign[cj] -= hassign[ci];
            nsplit++;
        }
    }

    return nsplit;
}

void LinearVectorKMeans::post_process_centroids(
        size_t d,
        size_t k,
        float* centroids,
        bool spherical,
        bool int_centroids) {
    if (spherical) {
        faiss::fvec_renorm_L2(d, k, centroids);
    }

    if (int_centroids) {
        for (size_t i = 0; i < d * k; i++) {
            centroids[i] = roundf(centroids[i]);
        }
    }
}

}
