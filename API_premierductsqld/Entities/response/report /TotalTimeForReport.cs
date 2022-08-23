using System;
namespace API_premierductsqld.Entities.response
{
    public class TotalTimeForReport
    {

        public string total_break_time { get; set; }

        public string total_prduction_time { get; set; }
        public string total_non_prduction_time { get; set; }

        public string total_working_time { get; set; }
        public string total_job_worked_on { get; set; }

        public TotalTimeForReport()
        {

        }
        public TotalTimeForReport(
         string total_break_time,

         string total_prduction_time,
         string total_non_prduction_time,

         string total_working_time,
         string total_job_worked_on
)
        {
            this.total_break_time = total_break_time;
            this.total_prduction_time = total_prduction_time;
            this.total_non_prduction_time = total_non_prduction_time;
            this.total_working_time = total_working_time;
            this.total_job_worked_on = total_job_worked_on;
        }
    }
}
