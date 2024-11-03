using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using RatingService.Model;
using RatingService.Service;

namespace RatingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResponseController : ControllerBase
    {
        private readonly ResponseService _responseService;

        public ResponseController(ResponseService responseService)
        {
            _responseService = responseService;
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetResponse(int clientId)
        {
            var response = await _responseService.GetClientResponse(clientId);
            if (response == null) 
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectResponse(int projectId)
        {
            var projectResponse = await _responseService.GetProjectResponse(projectId);
            foreach(var response in projectResponse)
            {
                await _responseService.Delete(response.Id);
            }
            return Ok();
        }

        [HttpPost("create")]
        public async Task<IActionResult> UpdateResponse(Response response)
        {
            await _responseService.Create(response);
            return NoContent();
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateResponse(int id)
        {
            await _responseService.Update(id);
            return NoContent();
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteResponse(int id)
        {
            await _responseService.Delete(id);
            return NoContent();
        }
    }
}
