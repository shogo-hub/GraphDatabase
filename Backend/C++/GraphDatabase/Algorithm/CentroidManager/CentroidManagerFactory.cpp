#include "CentroidManagerFactory.h"
#include "K-Means/LinearVector/LinearVectorKMeans.h"
#include "K-Medoids/LinearVector/LinearVectorKMedoids.h"
#include "../SimilarityComputer/L2SimilarityComputer.h"
#include "../SimilarityComputer/InnerProductSimilarityComputer.h"
#include <faiss/MetricType.h>
#include <stdexcept>

namespace GraphDatabase::Algorithm {

static std::unique_ptr<ISimilarityComputer> makeSimilarityComputer(faiss::MetricType metric) {
    if (faiss::is_similarity_metric(metric)) {
        return std::make_unique<InnerProductSimilarityComputer>();
    }
    return std::make_unique<L2SimilarityComputer>();
}

std::unique_ptr<ICentroidManager> CentroidManagerFactory::create(
    ClusteringAlgorithmType type,
    const CentroidManagerOptions& options,
    faiss::MetricType metric
) {
    switch (type) {
        case ClusteringAlgorithmType::K_MEANS:
            return std::make_unique<LinearVectorKMeans>(options, makeSimilarityComputer(metric));
        case ClusteringAlgorithmType::K_MEDOIDS:
            return std::make_unique<LinearVectorKMedoids>(options, makeSimilarityComputer(metric));
        default:
            throw std::invalid_argument("Unknown ClusteringAlgorithmType");
    }
}

} // namespace GraphDatabase::Algorithm
