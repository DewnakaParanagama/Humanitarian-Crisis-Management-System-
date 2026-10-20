using Microsoft.AspNetCore.Mvc;
using Humanitarian.Application.DTOs; // DTO එක තියෙන තැන සම්බන්ධ කරනවා

namespace Humanitarian.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        [HttpPost]
        public IActionResult SubmitRequest([FromBody] AssistanceRequestDto requestDto)
        {
            var response = new 
            {
                requestId = "req-" + new Random().Next(100, 999),
                status = "Pending Verification",
                message = "Assistance request submitted successfully.",
                receivedData = requestDto 
            };

            return Created("", response); 
        }
    }
}