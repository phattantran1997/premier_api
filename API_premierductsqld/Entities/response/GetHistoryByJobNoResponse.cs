using System;
namespace API_premierductsqld.Entities
{
    public class GetHistoryByJobNoResponse
    {
       
        public string jobday { get; set; }
        public string jobno { get; set; }
        public string stationName { get; set; }
        public int stationNo { get; set; }
        public int id { get; set; }

        //for App
        public string user_name { get; set; }
        public string job_time { get; set; }
        public string duration { get; set; }
        public GetHistoryByJobNoResponse(string jobday, string jobno, string stationName, int stationNo, int id, string user_name, string job_time, string duration)
        {
            this.jobday = jobday;
            this.jobno = jobno;
            this.stationName = stationName;
            this.stationNo = stationNo;
            this.id = id;
            this.user_name = user_name;
            this.job_time = job_time;
            this.duration = duration;
        }

    }
}
