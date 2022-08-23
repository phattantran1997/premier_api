using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Service;
using DTO_PremierDucts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
    /// <summary>
    /// API for Dashboard
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiController]
    [Authorize]
    [Route("app")]
    public class AppController :ControllerBase
    {
        private JobTimingService jobTimingService ;


        public AppController()
        {
           jobTimingService = new JobTimingService();
        }


        /// <summary>
        /// Load all station on dashboard with Rate
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("station/data/with_rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ResponseData getAllStationDashBoard([Required(ErrorMessage = "Date is required")] string date)
        {
            return jobTimingService.GetAllStationWithRate(date);
        }
        /// <summary>
        /// When user click Station will request data list of JobNo data
        /// </summary>
        /// 
        /// <param name="date"></param>
        /// <param name="stationNo"></param>
        /// <returns></returns>
        [HttpGet("job/data/by_station")]
        public Task<ResponseData> getJobDataByStation(
        [Required(ErrorMessage = "station is required")] int station,
        [Required(ErrorMessage = "Date is required")] string date)
        {
            return jobTimingService.getJobDataByStation(station, date); ;

        }
        /// <summary>
        /// test call api in QLDData Database
        /// </summary>
        /// 
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("test")]
        public Task<List<Qlddataresponse>> tempCallApi(string date)
        {
            return jobTimingService.TestAsync(date);
        }

    }
}
