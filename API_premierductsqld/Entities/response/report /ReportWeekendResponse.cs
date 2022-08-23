using System;
namespace API_premierductsqld.Entities.response
{
    public class ReportWeekendResponse
    {

        public string user { get; set; }
        public int total_job { get; set; }
        public double sum_time_break { get; set; }
        public int break_quantity { get; set; }
        public double sum_nonprod_time { get; set; }
        public double sum_prod_time { get; set; }
        public double total_working_time { get; set; }

        public ReportWeekendResponse(string user, int total_job, double sum_time_break, int break_quantity, double sum_nonprod_time, double sum_prod_time, double total_working_time)
        {
            this.user = user;
            this.total_job = total_job;
            this.sum_time_break = sum_time_break;
            this.break_quantity = break_quantity;
            this.sum_nonprod_time = sum_nonprod_time;
            this.sum_prod_time = sum_prod_time;
            this.total_working_time = total_working_time;
        }
    }
}
