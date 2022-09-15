using API_premierductsqld.Service;
using DTO_PremierDucts;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
    [ApiController]
    [Route("station")]
    public class StationController : ControllerBase
    {

        StationService stationService;

        public StationController()
        {
            stationService = new StationService();
        }

        /// <summary>
        /// Get all station data
        /// </summary>
        /// 
        [HttpGet("all")]
        public ResponseData getAllStation()
        {

            return stationService.getAllStation();

        }

        /// <summary>
        /// Get all duration of station per jobno
        /// </summary>
        /// 
        /// <param name="jobno"></param>
        /// <param name="stationNo"></param>
        /// <returns>ResponseData</returns>
        [HttpGet("all/duration/{jobno}")]
        public ResponseData getDurationOfStation(string jobno, int stationNo)
        {

            return stationService.getDurationOfStation(jobno, stationNo);

        }

    }
}