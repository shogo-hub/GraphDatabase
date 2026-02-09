#include <gtest/gtest.h>
#include <vector>
#include <cmath>
#include <random>
#include <algorithm>

// プロジェクトのヘッダーパスに合わせて調整してください
#include "../Algorithm/CentroidManager/CentroidManagerFactory.h"
#include "../Algorithm/CentroidManager/ICentroidManager.h"
#include "../Algorithm/CentroidManager/ClusteringTypes.h"

using namespace GraphDatabase::Algorithm;

// --- Helper Functions ---

// ランダムなデータを生成 (正規分布)
std::vector<float> generate_data(size_t n, size_t d, float mean, float stddev, int seed) {
    std::vector<float> data(n * d);
    std::mt19937 gen(seed);
    std::normal_distribution<float> dist(mean, stddev);
    for (size_t i = 0; i < n * d; ++i) {
        data[i] = dist(gen);
    }
    return data;
}

// 配列同士が近似的に等しいか確認
bool arrays_equal(const float* a, const float* b, size_t d, float eps = 1e-5) {
    for (size_t i = 0; i < d; ++i) {
        if (std::abs(a[i] - b[i]) > eps) return false;
    }
    return true;
}

// --- Test Classes ---

class CentroidManagerTest : public ::testing::Test {
protected:
    void SetUp() override {
        // 必要であれば共通の初期化処理をここに記述
    }

    void TearDown() override {
        // クリーンアップ処理
    }
};

// --- KMeans Tests ---

TEST_F(CentroidManagerTest, KMeans_BasicLogic) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 100;

    // Cluster 1 set around (10, 10), Cluster 2 around (-10, -10)
    auto data1 = generate_data(n / 2, d, 10.0f, 1.0f, 1);
    auto data2 = generate_data(n / 2, d, -10.0f, 1.0f, 2);
    
    std::vector<float> data = data1;
    data.insert(data.end(), data2.begin(), data2.end());

    // Initial centroids (0,0) and (1,1) - neutral starting point
    std::vector<float> centroids = {0.0f, 0.0f, 1.0f, 1.0f};

    // Prepare buffers
    std::vector<int64_t> assignments(n);
    std::vector<float> distances(n);
    std::vector<float> hassign(k); 

    // Factory creation
    CentroidManagerOptions opts;
    auto manager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, opts);

    // 1. Assignment Step
    manager->findClosestCentroids(
        n, k, d, 
        reinterpret_cast<const uint8_t*>(data.data()), 
        nullptr, 
        centroids.data(), 
        assignments.data(), 
        distances.data()
    );

    // 2. Update Step
    manager->updateCentroids(
        n, k, d, 
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr, // No codec
        0, // k_frozen
        centroids.data(),
        assignments.data(),
        nullptr // Equal weights
    );

    // Assertions
    // 重心が正と負の大きく離れた領域に移動しているはずです
    bool c1_positive = (centroids[0] > 5.0f && centroids[1] > 5.0f);
    bool c1_negative = (centroids[0] < -5.0f && centroids[1] < -5.0f);
    
    bool c2_positive = (centroids[2] > 5.0f && centroids[3] > 5.0f);
    bool c2_negative = (centroids[2] < -5.0f && centroids[3] < -5.0f);

    EXPECT_TRUE((c1_positive && c2_negative) || (c1_negative && c2_positive)) 
        << "Centroids did not separate into positive and negative clusters correctly.";
}

TEST_F(CentroidManagerTest, KMeans_FrozenCentroids) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 20;

    // Data around (10,10)
    auto data = generate_data(n, d, 10.0f, 1.0f, 123);

    // Centroid 0: Frozen at (-100, -100)
    // Centroid 1: Free at (0, 0)
    std::vector<float> centroids = {-100.0f, -100.0f, 0.0f, 0.0f};
    std::vector<float> initial_frozen = {-100.0f, -100.0f};

    std::vector<int64_t> assignments(n);
    std::vector<float> hassign(k);

    CentroidManagerOptions opts;
    auto manager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, opts);

    // Assign
    manager->findClosestCentroids(
        n, k, d, 
        reinterpret_cast<const uint8_t*>(data.data()), 
        nullptr, 
        centroids.data(), 
        assignments.data(), 
        nullptr
    );

    // Update with k_frozen = 1
    manager->updateCentroids(
        n, k, d, 
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        1, // <--- FREEZE FIRST CENTROID
        centroids.data(),
        assignments.data(),
        nullptr
    );

    // Check 1: Frozen centroid must NOT change
    EXPECT_EQ(centroids[0], initial_frozen[0]);
    EXPECT_EQ(centroids[1], initial_frozen[1]);

    // Check 2: Free centroid should move towards data
    EXPECT_GT(centroids[2], 5.0f);
    EXPECT_GT(centroids[3], 5.0f);
}

// --- KMedoids Tests ---

TEST_F(CentroidManagerTest, KMedoids_PropertyCheck) {
    size_t k = 2;
    size_t d = 5;
    size_t n = 50;

    auto data = generate_data(n, d, 0.0f, 10.0f, 999);
    
    // Random centroids to start
    std::vector<float> centroids(k * d, 0.0f);

    std::vector<int64_t> assignments(n);
    std::vector<float> hassign(k);

    CentroidManagerOptions opts;
    opts.use_sampling = false; // Disable sampling for deterministic checks
    auto manager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEDOIDS, opts);

    // Initialize centroids with first k points (cheat for setup)
    for(size_t i=0; i<k*d; ++i) centroids[i] = data[i];
    
    // Initial Assignment
    manager->findClosestCentroids(
        n, k, d, 
        reinterpret_cast<const uint8_t*>(data.data()), 
        nullptr,
        centroids.data(),
        assignments.data(), 
        nullptr
    );

    // Update
    manager->updateCentroids(
        n, k, d, 
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        0, 
        centroids.data(),
        assignments.data(),
        nullptr
    );

    // CRITICAL CHECK: Every new centroid MUST be one of the original data points
    int matched_count = 0;
    
    for (size_t ci = 0; ci < k; ++ci) {
        const float* c = centroids.data() + ci * d;
        bool found = false;

        for (size_t i = 0; i < n; ++i) {
            const float* x = data.data() + i * d;
            if (arrays_equal(c, x, d)) {
                found = true;
                break;
            }
        }
        
        if (found) matched_count++;
    }

    EXPECT_EQ(matched_count, k) 
        << "Each K-Medoids centroid must exactly match a point from the dataset.";
}
