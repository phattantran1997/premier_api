using System;
namespace API_premierductsqld.Entities.response
{
    public class JobTimingResponse
    {
        public JobTimingResponse(string jobno, string operatorID,
            string jobday, string jobtime, int id,
            int stationNo, string duration,
            string filename, string handle, string itemno, string stationName, string stationStatus)
        {
            this.jobno = jobno;
            this.operatorID = operatorID;
            this.jobday = jobday;
            this.jobtime = jobtime;
            this.id = id;
            this.stationNo = stationNo;
            this.duration = duration;
            this.filename = filename;
            this.handle = handle;
            this.itemno = itemno;
            this.stationName = stationName;
            this.stationStatus = stationStatus;
        }

        public JobTimingResponse()
        {



        }

        public string jobno { get; set; }
        public string operatorID { get; set; }
        public string jobday { get; set; }
        public string jobtime { get; set; }
        public int id { get; set; }
        public int stationNo { get; set; }
        public string duration { get; set; }
        public string filename { get; set; }
        public string handle { get; set; }
        public string itemno { get; set; }
        public string stationName { get; set; }
        public string stationStatus { get; set; }


    }
}
