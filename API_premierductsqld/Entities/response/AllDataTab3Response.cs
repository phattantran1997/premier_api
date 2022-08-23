using System;
using System.Collections.Generic;

namespace API_premierductsqld.Entities.response
{
    public class AllDataTab3Response
    {
        public M2DataResponse job_data { get; set; }
        public DataStaionTab3 dataStaion { get; set; }
        public AllDataTab3Response()
        {
        }

   
        public AllDataTab3Response(M2DataResponse job_data, DataStaionTab3 dataStaion)
        {
            this.job_data = job_data;
            this.dataStaion = dataStaion;
        }
    }
}

