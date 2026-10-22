using Microsoft.AspNetCore.Mvc;
using Humanitarian.Application.DTOs;

namespace Humanitarian.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CrisesController : ControllerBase
    {
        // POST: api/crises
        [HttpPost]
        public IActionResult CreateCrisis([FromBody] CreateCrisisDto crisisDto)
        {
            var response = new
            {
                crisisId = "cr-" + new Random().Next(100, 999),
                status = "Active",
                message = "Crisis event created successfully.",
                createdData = crisisDto
            };

            return Created("", response);
        }
    }
}