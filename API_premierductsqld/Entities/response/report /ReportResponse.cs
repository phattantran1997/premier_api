using System;
namespace API_premierductsqld.Entities.response
{
    public class ReportResponse
    {

        public string date { get; set; }
        public string user { get; set; }
        public string time_start { get; set; }
        public string area { get; set; }
        public string time_first_job { get; set; }
        public string job_no { get; set; }
        public string breaking_time { get; set; }
        public string time_back { get; set; }
        public string area_back { get; set; }
        public string time_of_jab { get; set; }
        public string job_after_back { get; set; }
        public string time_finished { get; set; }
        public string total_job { get; set; }
        public string sum_time_break { get; set; }
        public int break_quantity { get; set; }
        public string sum_nonprod_time { get; set; }
        public string sum_prod_time { get; set; }
        public string total_working_time { get; set; }

        public ReportResponse()
        {

        }

        public ReportResponse(string date, string user, string time_start, string area, string time_first_job, string job_no, string breaking_time, string time_back, string area_back, string time_of_jab, string job_after_back, string time_finished, string total_job,
            string sum_time_break,
            int break_quantity,
            string sum_nonprod_time,
            string sum_prod_time,
            string total_working_time)
        {
            this.date = date;
            this.user = user;
            this.time_start = time_start;
            this.area = area;
            this.time_first_job = time_first_job;
            this.job_no = job_no;
            this.breaking_time = breaking_time;
            this.time_back = time_back;
            this.area_back = area_back;
            this.time_of_jab = time_of_jab;
            this.job_after_back = job_after_back;
            this.time_finished = time_finished;
            this.total_job = total_job;
            this.sum_time_break = sum_time_break;
            this.sum_nonprod_time = sum_nonprod_time;
            this.sum_prod_time = sum_prod_time;
            this.total_working_time = total_working_time;
            this.break_quantity = break_quantity;
        }

        public ReportResponse(string date, string user, string time_start, string area, string time_first_job, string job_no)
        {
            this.date = date;
            this.user = user;
            this.time_start = time_start;
            this.area = area;
            this.time_first_job = time_first_job;
            this.job_no = job_no;
        }
    }
}
