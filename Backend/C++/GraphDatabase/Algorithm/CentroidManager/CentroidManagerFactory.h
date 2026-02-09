#pragma once

#include "ICentroidManager.h"
#include "ClusteringTypes.h"
#include <memory>

namespace GraphDatabase::Algorithm {

class CentroidManagerFactory {
public:
    static std::unique_ptr<ICentroidManager> create(
        ClusteringAlgorithmType type,
        const CentroidManagerOptions& options
    );
};

}
