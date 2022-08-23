using System;
namespace API_premierductsqld.Entities
{
    public class JobTiming
    {
        public JobTiming()
        {
        }

        public JobTiming(string jobno, string operatorID, string jobday, string jobtime, int id, int stationNo, string duration, string filename, string handle, string itemno)
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
    }
}
