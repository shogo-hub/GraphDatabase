// 1 st stage (Clustering and bruteforce computing)
// IVF Class
    // ICluster 
    // IQuantizer
    // nlist
    // Probe parallel computation
    //InvLists : list of cluster [id, code(quantized/not vector)]
    
#include <iostream>


namespace IVF {
    class IVF{
        
        IVF(
            IQuantizer* quantizer,
            ICluster* cluster,
            size_t nlist,
            //size_t code_size, -> Maybe set at env or others 
            //MetricType metric = METRIC_L2,
            bool own_invlists = true
        );

    }


}

// 2nd stage Incorporate Residual and PQ



// 3 rd stage (How to handle bigger data more which doesn't fit in RAM)
//IStorage (Inverted Lists): Where does the data live? (RAM? SSD? Database?)
// IEncoder (Level 2): How is the vector stored? (Raw float? Compressed binary code?)
// Code size