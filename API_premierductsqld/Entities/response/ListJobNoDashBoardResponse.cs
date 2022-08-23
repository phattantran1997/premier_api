using System;
namespace API_premierductsqld.Entities.response
{
    public class ListJobNoDashBoardResponse
    {
        public string jobno { get; set; }
        public string rate { get; set; }
        public string labour_time { get; set; }
        public int people { get; set; }
        public string metal_m2 { get; set; }
        public string insu_m2 { get; set; }
        public string status_current { get; set; }
        public string file_name { get; set; }
        public string interval { get; set; }

        public ListJobNoDashBoardResponse()
        {
        }

        public ListJobNoDashBoardResponse(string jobno, string rate, string labour_time, int people, string metal_m2, string insu_m2, string status_current,string filename, string interval)
        {
            this.jobno = jobno;
            this.rate = rate;
            this.labour_time = labour_time;
            this.people = people;
            this.metal_m2 = metal_m2;
            this.insu_m2 = insu_m2;
            this.status_current = status_current;
            this.file_name = filename;
            this.interval = interval;
        }
    }
}
