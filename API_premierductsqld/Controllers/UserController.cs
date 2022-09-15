using System.Collections.Generic;
using System.Threading.Tasks;
using API_premierductsqld.Entities;
using API_premierductsqld.Service;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private UserService userService;
        public UserController()
        {
            userService = new UserService();
        }
        /// <summary>
        /// Gets the list of all online Employees.
        /// </summary>
        /// <returns>The list of Employees.</returns>
        // GET: api/Employee
        [HttpGet("getAllOnlineUser")]
        public Task<List<StationAttendees>> GetAllOnlineUser()
        {
            Task<List<StationAttendees>> actionResult = userService.GetAllOnlineUser();
            return actionResult;

        }

        [HttpGet("{id}")]
        public string test(string id)
        {
            return id;

        }

    }
}