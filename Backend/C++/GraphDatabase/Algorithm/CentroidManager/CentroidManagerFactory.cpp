#include "CentroidManagerFactory.h"
#include "K-Means/LinearVector/LinearVectorKMeans.h"
#include "K-Medoids/LinearVector/LinearVectorKMedoids.h"
#include <stdexcept>

namespace GraphDatabase::Algorithm {

std::unique_ptr<ICentroidManager> CentroidManagerFactory::create(
    ClusteringAlgorithmType type,
    const CentroidManagerOptions& options
) {
    switch (type) {
        case ClusteringAlgorithmType::K_MEANS:
            return std::make_unique<LinearVectorKMeans>(options);
        case ClusteringAlgorithmType::K_MEDOIDS:
            return std::make_unique<LinearVectorKMedoids>(options);
        default:
            throw std::invalid_argument("Unknown ClusteringAlgorithmType");
    }
}

}
