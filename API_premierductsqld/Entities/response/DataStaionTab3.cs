using System;
using System.Collections.Generic;

namespace API_premierductsqld.Entities.response
{
    public class DataStaionTab3
    {
        public string totalDuration { get; set; }
        public List<JobTimingResponse> history { get; set; }
        public DataStaionTab3()
        {
            history = new List<JobTimingResponse>();
        }
    }
}

    