#pragma once

#include "ICentroidManager.h"
#include "ClusteringTypes.h"
#include <memory>
#include <faiss/Index.h>

namespace GraphDatabase::Algorithm {

class CentroidManagerFactory {
public:
    static std::unique_ptr<ICentroidManager> create(
        ClusteringAlgorithmType type,
        const CentroidManagerOptions& options,
        faiss::MetricType metric = faiss::METRIC_L2
    );
};

} // namespace GraphDatabase::Algorithm
