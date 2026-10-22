using Microsoft.AspNetCore.Mvc;
using Humanitarian.Application.DTOs;

namespace Humanitarian.API.Controllers
{
   [Route("api/v1/[controller]")]
    [ApiController]
    public class CitizensController : ControllerBase
    {
        // POST: api/citizens/register
        [HttpPost("register")]
        public IActionResult RegisterCitizen([FromBody] RegisterCitizenDto citizenDto)
        {
            var response = new
            {
                citizenId = "c-" + new Random().Next(100, 999),
                message = "Citizen registered successfully.",
                registeredData = citizenDto
            };

            return Created("", response);
        }
    }
}