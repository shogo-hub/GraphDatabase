#include <gtest/gtest.h>
#include <vector>
#include <cmath>
#include <random>

#include "../../Algorithm/CentroidManager/CentroidManagerFactory.h"
#include "../../Algorithm/CentroidManager/ICentroidManager.h"
#include "../../Algorithm/CentroidManager/ClusteringTypes.h"

using namespace GraphDatabase::Algorithm;

// --- Helper Functions ---

/**
 * @brief Generate test vectors sampled from a normal distribution.
 *
 * This helper creates `n` vectors of dimension `d` and fills all coordinates
 * with random values drawn from a Gaussian distribution defined by `mean` and
 * `stddev`. The random engine is seeded so the generated test data is
 * reproducible.
 *
 * @param n Number of vectors to generate.
 * @param d Dimension of each vector.
 * @param mean Mean of the normal distribution used for each coordinate.
 * @param stddev Standard deviation of the normal distribution.
 * @param seed Seed for the pseudo-random number generator.
 * @return Flat vector of size `n * d` storing the generated data.
 */
static std::vector<float> generate_data(size_t n, size_t d, float mean, float stddev, int seed) {
    std::vector<float> data(n * d);
    std::mt19937 gen(seed);
    std::normal_distribution<float> dist(mean, stddev);
    for (size_t i = 0; i < n * d; ++i) {
        data[i] = dist(gen);
    }
    return data;
}

// --- Tests ---

// BasicLogic: Verifies that one round of findClosestCentroids + updateCentroids
// correctly separates two clearly distinct clusters.
//
// Setup:
//   - 100 points split evenly: half around (+10, +10), half around (-10, -10).
//   - Initial centroids are placed near the origin, far from the true cluster centres.
//
// Flow:
//   1. findClosestCentroids  — assign each point to its nearest centroid.
//   2. updateCentroids       — recompute centroids as the mean of assigned points.
//
// Expected outcome:
//   After one update the two centroids must end up in opposite half-planes
//   (one clearly positive, one clearly negative), proving the separation works.
TEST(KMeansTest, BasicLogic) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 100;

    // Cluster 1 around (10, 10), Cluster 2 around (-10, -10)
    auto data1 = generate_data(n / 2, d, 10.0f, 1.0f, 1);
    auto data2 = generate_data(n / 2, d, -10.0f, 1.0f, 2);
    std::vector<float> data = data1;
    data.insert(data.end(), data2.begin(), data2.end());

    std::vector<float> centroids = {0.0f, 0.0f, 1.0f, 1.0f};
    std::vector<int64_t> assignments(n);
    std::vector<float> distances(n);
    std::vector<float> hassign(k);

    auto centroidManager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, CentroidManagerOptions{});

    centroidManager->findClosestCentroids(
        n, k, d,
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        centroids.data(),
        assignments.data(),
        distances.data());

    centroidManager->updateCentroids(
        n, k, d,
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        0,
        centroids.data(),
        assignments.data(),
        nullptr);

    bool c1_positive = (centroids[0] > 5.0f && centroids[1] > 5.0f);
    bool c1_negative = (centroids[0] < -5.0f && centroids[1] < -5.0f);
    bool c2_positive = (centroids[2] > 5.0f && centroids[3] > 5.0f);
    bool c2_negative = (centroids[2] < -5.0f && centroids[3] < -5.0f);

    EXPECT_TRUE((c1_positive && c2_negative) || (c1_negative && c2_positive))
        << "Centroids did not separate into positive and negative clusters correctly.";
}

// FrozenCentroids: Verifies that the k_frozen mechanism keeps selected centroids
// unchanged even when updateCentroids is called.
//
// Setup:
//   - 20 points all clustered around (+10, +10).
//   - Centroid 0 is placed far away at (-100, -100) and marked frozen (k_frozen = 1).
//   - Centroid 1 is free, starting at (0, 0).
//
// Flow:
//   1. findClosestCentroids  — assign points (most will go to centroid 1, the nearer one).
//   2. updateCentroids       — recompute only the free centroids (index >= k_frozen).
//
// Expected outcome:
//   - Centroid 0 stays exactly at (-100, -100) — frozen, must not move.
//   - Centroid 1 shifts toward the data cloud, ending up above (5, 5).
TEST(KMeansTest, FrozenCentroids) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 20;

    auto data = generate_data(n, d, 10.0f, 1.0f, 123);

    // Centroid 0: Frozen at (-100, -100), Centroid 1: Free at (0, 0)
    std::vector<float> centroids = {-100.0f, -100.0f, 0.0f, 0.0f};
    std::vector<int64_t> assignments(n);
    std::vector<float> hassign(k);

    auto centroidManager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, CentroidManagerOptions{});

    centroidManager->findClosestCentroids(
        n, k, d,
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        centroids.data(),
        assignments.data(),
        nullptr);

    centroidManager->updateCentroids(
        n, k, d,
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        1,  // freeze first centroid
        centroids.data(),
        assignments.data(),
        nullptr);

    // Frozen centroid must NOT change
    EXPECT_EQ(centroids[0], -100.0f);
    EXPECT_EQ(centroids[1], -100.0f);

    // Free centroid should move towards data
    EXPECT_GT(centroids[2], 5.0f);
    EXPECT_GT(centroids[3], 5.0f);
}
