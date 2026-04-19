#include "CentroidManagerFactory.h"
#include "K-Means/LinearVector/LinearVectorKMeans.h"
#include "../SimilarityComputer/L2SimilarityComputer.h"
#include "../SimilarityComputer/InnerProductSimilarityComputer.h"
#include <faiss/MetricType.h>
#include <stdexcept>

namespace GraphDatabase::Algorithm {

/**
 * @brief Resolve and instantiate the similarity computer for the given metric.
 *
 * @param metric Metric type used to resolve the concrete similarity computer.
 * @return Unique pointer to an ISimilarityComputer implementation.
 * @throws std::invalid_argument If the metric is not supported.
 */
static std::unique_ptr<ISimilarityComputer> makeSimilarityComputer(faiss::MetricType metric) {
    switch (metric) {
        case faiss::METRIC_L2:
            return std::make_unique<L2SimilarityComputer>();
        case faiss::METRIC_INNER_PRODUCT:
            return std::make_unique<InnerProductSimilarityComputer>();
        default:
            throw std::invalid_argument("Unsupported metric type for SimilarityComputer");
    }
}

/**
 * @brief Create a centroid manager for the specified clustering algorithm.
 *
 * This factory currently supports K-means and injects a similarity computer
 * that matches the given metric.
 *
 * @param type Clustering algorithm type.
 * @param options Configuration options for centroid manager creation.
 * @param metric Similarity metric used by the centroid manager internals.
 * @return Unique pointer to a configured ICentroidManager.
 * @throws std::invalid_argument If the algorithm type is unknown.
 */
std::unique_ptr<ICentroidManager> CentroidManagerFactory::create(
    ClusteringAlgorithmType type,
    const CentroidManagerOptions& options,
    faiss::MetricType metric
) {
    switch (type) {
        case ClusteringAlgorithmType::K_MEANS:
            return std::make_unique<LinearVectorKMeans>(options, makeSimilarityComputer(metric));
        default:
            throw std::invalid_argument("Unknown ClusteringAlgorithmType");
    }
}

} // namespace GraphDatabase::Algorithm
