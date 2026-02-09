#include <iostream>
#include <vector>
#include <cassert>
#include <cmath>
#include <cstring>
#include <random>
#include <algorithm>

// 相対パスはプロジェクト構成に合わせて調整してください
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

// 2つのベクトルの距離の二乗
float l2_sqr(const float* a, const float* b, size_t d) {
    float sum = 0;
    for (size_t i = 0; i < d; ++i) {
        float diff = a[i] - b[i];
        sum += diff * diff;
    }
    return sum;
}

// 配列同士が等しいか確認
bool arrays_equal(const float* a, const float* b, size_t d, float eps = 1e-5) {
    for (size_t i = 0; i < d; ++i) {
        if (std::abs(a[i] - b[i]) > eps) return false;
    }
    return true;
}

// --- Tests ---

void test_kmeans_basic() {
    std::cout << "[Test] KMeans Basic Logic" << std::endl;

    size_t k = 2;
    size_t d = 2;
    size_t n = 100;

    // Cluster 1 set around (10, 10), Cluster 2 around (-10, -10)
    auto data1 = generate_data(n / 2, d, 10.0f, 1.0f, 1);
    auto data2 = generate_data(n / 2, d, -10.0f, 1.0f, 2);
    
    std::vector<float> data = data1;
    data.insert(data.end(), data2.begin(), data2.end());

    // Initial centroids (0,0) and (1,1) - intentionally bad/neutral
    std::vector<float> centroids = {0.0f, 0.0f, 1.0f, 1.0f};

    // Prepare buffers
    std::vector<int64_t> assignments(n);
    std::vector<float> distances(n);
    std::vector<float> hassign(k); // Weights buffer

    // Factory creation
    CentroidManagerOptions opts;
    auto manager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, opts);

    // 1. Assignment Step
    // raw float data passed as uint8_t* via reinterpret_cast (standard use case without codec)
    manager->findClosestCentroids(
        n, k, d, 
        reinterpret_cast<const uint8_t*>(data.data()), 
        nullptr, // No codec
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

    // Check: Centroids should have moved towards large positive and negative numbers
    std::cout << "  Centroid 0: (" << centroids[0] << ", " << centroids[1] << ")" << std::endl;
    std::cout << "  Centroid 1: (" << centroids[2] << ", " << centroids[3] << ")" << std::endl;

    // One centroid should be roughly positive, one negative. Or dependent on initial assignment.
    // Given (0,0) and (1,1), likely split into positive/negative logic.
    bool has_positive = (centroids[0] > 5.0f && centroids[1] > 5.0f) || (centroids[2] > 5.0f && centroids[3] > 5.0f);
    bool has_negative = (centroids[0] < -5.0f && centroids[1] < -5.0f) || (centroids[2] < -5.0f && centroids[3] < -5.0f);

    assert(has_positive && "One centroid should move to +10 region");
    assert(has_negative && "One centroid should move to -10 region");

    std::cout << "  -> PASSED" << std::endl;
}

void test_kmeans_frozen() {
    std::cout << "[Test] KMeans Frozen Centroids" << std::endl;
    
    size_t k = 2;
    size_t d = 2;
    size_t n = 20;

    // Data around (10,10)
    auto data = generate_data(n, d, 10.0f, 1.0f, 123);

    // Centroid 0: Frozen at (-100, -100)
    // Centroid 1: Free at (0, 0)
    std::vector<float> centroids = {-100.0f, -100.0f, 0.0f, 0.0f};
    std::vector<float> initial_centroids = centroids;

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
    assert(centroids[0] == initial_centroids[0]);
    assert(centroids[1] == initial_centroids[1]);
    std::cout << "  Frozen logic verified." << std::endl;

    // Check 2: Free centroid should move towards data (10,10)
    assert(centroids[2] > 5.0f && centroids[3] > 5.0f);
    std::cout << "  Free centroid update verified." << std::endl;

    std::cout << "  -> PASSED" << std::endl;
}

void test_kmedoids_property() {
    std::cout << "[Test] KMedoids Property Check" << std::endl;

    size_t k = 2;
    size_t d = 5; // Higher dimension
    size_t n = 50;

    auto data = generate_data(n, d, 0.0f, 10.0f, 999);
    
    // Random centroids to start
    std::vector<float> centroids(k * d, 0.0f);

    std::vector<int64_t> assignments(n);
    std::vector<float> hassign(k);

    CentroidManagerOptions opts;
    opts.use_sampling = false; // Disable sampling to force exact search
    auto manager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEDOIDS, opts);

    // 1. Assign (randomly for simple test setup or run findClosest)
    // Let's run findClosest once
    manager->findClosestCentroids(
        n, k, d, 
        reinterpret_cast<const uint8_t*>(data.data()), 
        nullptr,
        data.data(), // Use first k points as initial centroids (cheat)
        assignments.data(), 
        nullptr
    );
    
    // Copy initial "centroids" used for assignment
    for(size_t i=0; i<k*d; ++i) centroids[i] = data[i];

    // 2. Update
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
    // K-Medoids always selects a member of the dataset as the representative
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
        else {
            std::cout << "  Centroid " << ci << " is NOT in dataset!" << std::endl;
        }
    }

    assert(matched_count == k && "All K-Medoids centroids must match a data point");
    std::cout << "  All centroids exist in dataset." << std::endl;

    std::cout << "  -> PASSED" << std::endl;
}

int main() {
    try {
        test_kmeans_basic();
        test_kmeans_frozen();
        test_kmedoids_property();
        
        std::cout << "\nAll CentroidManager tests passed successfully!" << std::endl;
    } catch (const std::exception& e) {
        std::cerr << "Test failed with exception: " << e.what() << std::endl;
        return 1;
    }
    return 0;
}
