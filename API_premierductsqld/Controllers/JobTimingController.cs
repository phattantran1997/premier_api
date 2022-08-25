using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using API_premierductsqld.Entities.request;
using API_premierductsqld.Service;
using DTO_PremierDucts;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
    [ApiController]
    [Authorize]
    [Route("jobtiming")]
    public class JobTimingController : ControllerBase
    {
        private JobTimingService jobTimingService;

        public JobTimingController()
        {
            jobTimingService = new JobTimingService();
        }
        /// <summary>
        /// Get all current jobtiming of list users
        /// </summary>
        /// 
        /// <returns></returns>
        [HttpPost("users/current")]
        public ResponseData getCurrentJobByUsers([FromBody] GetLastestJobTimingsRequest request)
        {

            return jobTimingService.getCurrentJobByUsers(request.jobday, request.users);

        }


    /// <summary>
    /// Get list jobno which were string by specific date.
    /// </summary>
    /// 
    /// <param name="date"></param>
    ///  <param name="end"></param>
    /// <returns></returns>
    [HttpGet("list/dates")]
        public ResponseData getListJobNoString([Required]string date, string end)
        {

            return jobTimingService.getListJobNoString(date, end);
        }


    /// <summary>
    /// Get all data include jobno, history, total duration by startdate and enddate
    /// </summary>
    /// 
    /// <param name="date"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    [HttpGet("all/data/tab3")]
        public Task<ResponseData> getAllDataTabs3([Required] string date, string end)
        {
            string token = Request.Headers["Token"].FirstOrDefault()?.Split(" ").Last();
            return jobTimingService.getAllDataTabs3Async(date, end, token);
        }

        /// <summary>
        /// Get detail list jobtiming
        /// </summary>
        /// 
        /// <param name="jobno"></param>
        /// <returns></returns>
        [HttpPost("data/detail")]
        public ResponseData getJobTimingsDetail([Required][FromBody] List<string> jobno)
        {

            return jobTimingService.getJobTimingsDetail(jobno);
        }

        /// <summary>
        /// Get  all jobtiming specific date
        /// </summary>
        /// 
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("all/data/by_date")]
        public ResponseData getAllDataJobtimingByDate(string date)
        {

           return jobTimingService.getAllDataJobtimingByDate(date);
           
        }

 


    }
}
