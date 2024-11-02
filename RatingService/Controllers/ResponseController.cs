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
        public async Task<IActionResult> GetReviews(int clientId)
        {
            var response = await _responseService.GetClientResponse(clientId);
            if (response == null) 
            {
                return NotFound();
            }
            return Ok(response);
        }

        [HttpPost("{projectId}")]
        public async Task<IActionResult> Accept(int projectId)
        {
            var projectResponse = await _responseService.GetProjectResponse(projectId);
            foreach(var response in projectResponse)
            {
                await _responseService.Delete(response.Id);
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _responseService.Delete(id);
            return NoContent();
        }
    }
}
