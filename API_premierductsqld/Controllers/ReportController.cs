using API_premierductsqld.Service;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
    [ApiController]
    [Route("report")]
    public class ReportController
    {

        private ReportService reportService;

        public ReportController()
        {
            reportService = new ReportService();
        }
  
        [HttpGet("1")]
        public void report1(string date)
        {
            reportService.report1(date);

        }

        [HttpGet("2")]
        public void report2(string date)
        {
            reportService.report2(date);

        }

        [HttpGet("3")]
        public void report3(string date)
        {

            reportService.report3(date);

        }

        [HttpGet("4")]
        public void report4(string date)
        {
            reportService.report4(date);

        }

        [HttpGet("weekend")]
        public string reportForWeekend()
        {
            return reportService.reportForWeekend();

        }
        [HttpDelete("delete")]
        public void delete(string date)
        {
            reportService.DeleteDataByDate(date);

        }
    }
}
