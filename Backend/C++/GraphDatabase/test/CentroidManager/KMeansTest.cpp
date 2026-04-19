#include <gtest/gtest.h>
#include <vector>
#include <cmath>
#include <random>

#include "../../Algorithm/CentroidManager/CentroidManagerFactory.h"
#include "../../Algorithm/CentroidManager/ClusteringTypes.h"

using namespace GraphDatabase::Algorithm;

// --- Helper Functions ---

/**
 * @brief Generate test vectors sampled from a normal distribution.
 *
 * @param n      Number of vectors to generate.
 * @param d      Dimension of each vector.
 * @param mean   Mean of the normal distribution.
 * @param stddev Standard deviation of the normal distribution.
 * @param seed   Seed for the pseudo-random number generator.
 * @return Flat vector of size `n * d`.
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
        1,
        centroids.data(),
        assignments.data(),
        nullptr);

    EXPECT_EQ(centroids[0], -100.0f);
    EXPECT_EQ(centroids[1], -100.0f);
    EXPECT_GT(centroids[2], 5.0f);
    EXPECT_GT(centroids[3], 5.0f);
}

// SplitEmptyCluster: Verifies that split_clusters fires when all points collapse
// to one centroid, leaving the other empty.
//
// Setup:
//   - 20 points tightly around (+10, +10).
//   - Both centroids start at (+10, +10) — identical, so all points go to centroid 0.
//   - Centroid 1 gets zero assignments and must be split off from centroid 0.
//
// Expected outcome:
//   - updateCentroids returns nsplit > 0.
//   - After the split, centroid 1 is no longer identical to centroid 0.
TEST(KMeansTest, SplitEmptyCluster) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 20;

    auto data = generate_data(n, d, 10.0f, 0.1f, 42);

    // Both centroids at the same location — all points will go to centroid 0
    std::vector<float> centroids = {10.0f, 10.0f, 10.0f, 10.0f};
    std::vector<int64_t> assignments(n, 0);  // all assigned to cluster 0
    std::vector<float> hassign(k, 0.0f);

    auto centroidManager = CentroidManagerFactory::create(ClusteringAlgorithmType::K_MEANS, CentroidManagerOptions{});

    int nsplit = centroidManager->updateCentroids(
        n, k, d,
        hassign.data(),
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        0,
        centroids.data(),
        assignments.data(),
        nullptr);

    EXPECT_GT(nsplit, 0) << "Expected at least one cluster split.";
    EXPECT_FALSE(centroids[0] == centroids[2] && centroids[1] == centroids[3])
        << "Centroids must differ after split.";
}

// InnerProduct: Verifies that the factory correctly wires InnerProductSimilarityComputer
// and that assignment follows inner-product similarity (higher = closer).
//
// Setup:
//   - Centroid 0 at (1, 0), centroid 1 at (0, 1).
//   - Query point (0.9, 0.1): inner product with centroid 0 is 0.9, with centroid 1 is 0.1.
//
// Expected outcome:
//   - The point is assigned to centroid 0.
TEST(KMeansTest, InnerProductAssignment) {
    size_t k = 2;
    size_t d = 2;
    size_t n = 1;

    std::vector<float> data     = {0.9f, 0.1f};
    std::vector<float> centroids = {1.0f, 0.0f,   // centroid 0
                                    0.0f, 1.0f};   // centroid 1
    std::vector<int64_t> assignments(n);

    auto centroidManager = CentroidManagerFactory::create(
        ClusteringAlgorithmType::K_MEANS,
        CentroidManagerOptions{},
        faiss::METRIC_INNER_PRODUCT);

    centroidManager->findClosestCentroids(
        n, k, d,
        reinterpret_cast<const uint8_t*>(data.data()),
        nullptr,
        centroids.data(),
        assignments.data(),
        nullptr);

    EXPECT_EQ(assignments[0], 0) << "Point closer to centroid 0 by inner product.";
}
