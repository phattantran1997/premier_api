using System;
namespace API_premierductsqld.Entities.response
{
    public class JobNoandStationNoResponse
    {
        public string jobNo { get; set; }
        public int stationNo { get; set; }
        public JobNoandStationNoResponse()
        {
        }

        public JobNoandStationNoResponse(string jobNo, int stationNo)
        {
            this.jobNo = jobNo;
            this.stationNo = stationNo;
        }
    }
}

