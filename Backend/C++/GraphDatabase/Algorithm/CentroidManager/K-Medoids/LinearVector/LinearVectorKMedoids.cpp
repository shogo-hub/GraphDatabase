#include "LinearVectorKMedoids.h"
#include <cstring>
#include <omp.h>
#include <cassert>
#include <vector>
#include <cmath>
#include <limits>
#include <algorithm> // for std::sample if available, or random_shuffle
#include <faiss/impl/FaissAssert.h>
#include <faiss/utils/utils.h> 
#include <faiss/utils/distances.h>
#include <faiss/utils/random.h>

namespace GraphDatabase::Algorithm {

void LinearVectorKMedoids::compute_centroids(
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
        bool use_sampling,
        size_t sample_size) {
    
    // 凍結された重心の分だけクラスタ数を減らし、更新対象の数をkとします
    k -= k_frozen;
    // 重心配列のポインタも凍結分のオフセットを進めます
    centroids += k_frozen * d;

    // 各クラスタの重み（または要素数）を格納する配列 hassign を0で初期化します
    memset(hassign, 0, sizeof(float) * k);

    // クラスタIDから各データ点のインデックスリストへの逆引きインデックスを作成します
    std::vector<std::vector<size_t>> cluster_members(k);
    
    // ベクトルの再確保を減らすため、平均的な要素数を見積もって予約します
    if (k > 0) {
        size_t avg_n = n / k;
        // 偏りを考慮して平均の2倍程度確保しておきます
        for(auto& vec : cluster_members) vec.reserve(avg_n * 2); 
    }

    // 全データ点 n についてループし、所属するクラスタごとにインデックスを振り分けます
    for (size_t i = 0; i < n; i++) {
        // 重心インデックスから凍結分を引いて、ローカルな0始まりのインデックスにします
        int64_t c = assign[i] - k_frozen;
        // 有効な範囲内であれば、そのクラスタのメンバーリストに追加します
        if (c >= 0 && c < (int64_t)k) {
            cluster_members[c].push_back(i);
        }
    }

    // 各データベクトルのバイトサイズを計算します（codecがある場合は圧縮サイズ、なければfloat配列サイズ）
    size_t code_size = codec ? codec->sa_code_size() : d * sizeof(float);

    // OpenMPを使用して、クラスタ単位で並列処理を行います
#pragma omp parallel for
    for (size_t ci = 0; ci < k; ci++) {
        // 現在のクラスタのメンバーリストへの参照を取得します
        const auto& members = cluster_members[ci];
        // メンバー数（要素数）を取得します
        size_t Nc = members.size();

        // クラスタの重み（人数または重み付き和）を計算します
        float weight_sum = 0;
        if (weights) {
            // 重み配列がある場合は、各メンバーの重みを加算します
            for (auto idx : members) weight_sum += weights[idx];
        } else {
            // 重みがない場合は要素数をそのまま重みとします
            weight_sum = (float)Nc;
        }
        // 計算した重みを hassign 配列に保存します
        hassign[ci] = weight_sum;

        // 要素数が0の空クラスタであれば、計算をスキップします（後でsplit_clustersで処理されます）
        if (Nc == 0) continue; 

        // --- メドイド候補の選定（サンプリングまたは全検索） ---
        std::vector<size_t> candidates;
        
        // サンプリングが有効で、かつ要素数がサンプルサイズより大きい場合
        if (use_sampling && Nc > sample_size) {
            // 候補リストをサンプルサイズ分予約します
            candidates.reserve(sample_size);
            
            // クラスタインデックスに基づいたシードで乱数生成器を初期化します（スレッドセーフ）
            faiss::RandomGenerator rng(1234 + ci);
            
            // ランダムにインデックスを選びます（非復元抽出の簡易実装）
            int attempts = 0;
            // 候補数がサンプルサイズに達するか、試行回数が上限を超えるまでループします
            while(candidates.size() < sample_size && attempts < sample_size * 2) {
                // メンバーの中からランダムに1つ選びます
                size_t rnd_idx = rng.rand_int64() % Nc;
                size_t real_idx = members[rnd_idx];
                
                // すでに候補リストに含まれていないか線形探索で重複チェックします（リストが小さいので高速）
                bool dup = false;
                for(size_t ex : candidates) if(ex == real_idx) { dup = true; break; }
                
                // 重複がなければ候補に追加します
                if(!dup) {
                    candidates.push_back(real_idx);
                }
                attempts++;
            }
            // もし候補が一つも選べなかった場合（稀なケース）、最初のメンバーをフォールバックとして追加します
            if (candidates.empty()) candidates.push_back(members[0]); 
        } else {
            // 要素数が少ない場合は、メンバー全員を候補とします
            candidates = members;
        }

        // --- 評価（最適なメドイドの探索） ---
        // 最小合計距離を無限大で初期化します
        double best_total_dist = std::numeric_limits<double>::max();
        // 最適なインデックスを初期値として最初のメンバーにしておきます
        size_t best_idx = members[0]; 

        // ベクトルデータをデコードするための一時バッファを用意します
        std::vector<float> cand_vec(d);
        std::vector<float> member_vec(d);

        // 選ばれた候補点それぞれについてループします
        for (size_t cand_idx : candidates) {
            // 候補となるベクトルをデコード（またはコピー）して取得します
            if (codec) {
                codec->sa_decode(1, x + cand_idx * code_size, cand_vec.data());
            } else {
                memcpy(cand_vec.data(), ((const float*)x) + cand_idx * d, d * sizeof(float));
            }

            // この候補をメドイドとした場合の、クラスタ内全点との距離の総和を計算します
            double current_total_dist = 0;

            // 全メンバーに対して距離を計算します
            for (size_t member_idx : members) {
                // 比較対象のメンバーベクトルをデコード（またはコピー）します
                if (codec) {
                    codec->sa_decode(1, x + member_idx * code_size, member_vec.data());
                } else {
                    memcpy(member_vec.data(), ((const float*)x) + member_idx * d, d * sizeof(float));
                }

                // 候補ベクトルとメンバーベクトルのL2距離の二乗を計算します
                float dist = faiss::fvec_L2sqr(cand_vec.data(), member_vec.data(), d);
                
                // 重みがある場合は重み付けして加算し、なければそのまま加算します
                if (weights) {
                    current_total_dist += dist * weights[member_idx];
                } else {
                    current_total_dist += dist;
                }
            }

            // これまでの最小合計距離よりも小さければ、ベスト値を更新します
            if (current_total_dist < best_total_dist) {
                best_total_dist = current_total_dist;
                best_idx = cand_idx;
            }
        }

        // --- 重心の更新 ---
        // 見つかった最適なメドイドの実データで、centroids配列を更新します
        if (codec) {
            codec->sa_decode(1, x + best_idx * code_size, centroids + ci * d);
        } else {
            memcpy(centroids + ci * d, ((const float*)x) + best_idx * d, d * sizeof(float));
        }
    }
}

int LinearVectorKMedoids::split_clusters(
        size_t d,
        size_t k,
        size_t n,
        size_t k_frozen,
        float* hassign,
        float* centroids) {
    
    k -= k_frozen;
    centroids += k_frozen * d;

    /* Handle empty clusters */
    size_t nsplit = 0;
    
    for (size_t ci = 0; ci < k; ci++) {
        if (hassign[ci] == 0) {
            // Find largest cluster to split
            size_t parent_cluster = 0;
            float max_weight = -1;
            
            for (size_t cj = 0; cj < k; cj++) {
                if (hassign[cj] > max_weight) {
                    max_weight = hassign[cj];
                    parent_cluster = cj;
                }
            }
            
            // Copy parent centroid
            memcpy(centroids + ci * d,
                   centroids + parent_cluster * d,
                   sizeof(*centroids) * d);

            // Split the weight
            hassign[ci] = hassign[parent_cluster] / 2;
            hassign[parent_cluster] -= hassign[ci];
            nsplit++;
        }
    }

    return nsplit;
}

void LinearVectorKMedoids::post_process_centroids(
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
