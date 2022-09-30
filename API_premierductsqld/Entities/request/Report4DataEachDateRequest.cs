using System;
namespace API_premierductsqld.Entities.request
{
    public class Report4DataEachDateRequest
    {
        public string jobday { get; set; }
        public double total_sum_nonprod_time { get; set; }
        public double total_sum_prod_time { get; set; }
        public double total_total_working_time { get; set; }
        public double totalMetalArea { get; set; }
        public double totalInsulationlArea { get; set; }
        public long qty { get; set; }

        public Report4DataEachDateRequest()
        {
        }
    }
}

