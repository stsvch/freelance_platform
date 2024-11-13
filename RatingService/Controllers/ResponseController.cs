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

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetClientResponse(int clientId)
        {
            if (clientId <= 0)
            {
                return BadRequest("Invalid client ID.");
            }

            try
            {
                var response = await _responseService.GetClientResponse(clientId);
                if (response == null)
                {
                    return NotFound("No response found for the specified client.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the client's response.");
            }
        }

        [HttpGet("freelancer/{freelancerId}")]
        public async Task<IActionResult> GetFreelancerResponse(int freelancerId)
        {
            if (freelancerId <= 0)
            {
                return BadRequest("Invalid client ID.");
            }

            try
            {
                var response = await _responseService.GetFreelancerResponse(freelancerId);
                if (response == null)
                {
                    return NotFound("No response found for the specified client.");
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the client's response.");
            }
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetProjectResponse(int projectId)
        {
            if (projectId <= 0)
            {
                return BadRequest("Invalid project ID.");
            }

            try
            {
                var projectResponses = await _responseService.GetProjectResponse(projectId);
                if (projectResponses == null || !projectResponses.Any())
                {
                    return NotFound("No responses found for the specified project.");
                }

                foreach (var response in projectResponses)
                {
                    await _responseService.Delete(response.Id);
                }

                return Ok("All responses for the specified project have been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting project responses.");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateResponse([FromBody] Response response)
        {
            if (response == null)
            {
                return BadRequest("Response data is required.");
            }

            try
            {
                await _responseService.CreateResponse(response);
                await _responseService.SendCreateMessage(response.ClientId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while creating the response.");
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateResponse([FromBody] Response response)
        {
            if (response == null || response.Id == 0)
            {
                return BadRequest("Valid response data with an ID is required.");
            }

            try
            {
                var existingResponse = await _responseService.GetResponseById(response.Id);
                if (existingResponse == null)
                {
                    return NotFound("Response not found.");
                }

                await _responseService.UpdateResponse(response);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the response.");
            }
        }

        [HttpPost("accept/{id}")]
        public async Task<IActionResult> AcceptResponse(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid response ID.");
            }

            try
            {
                var existingResponse = await _responseService.GetResponseById(id);
                if (existingResponse == null)
                {
                    return NotFound("Response not found.");
                }
                await _responseService.SendAcceptMessage(existingResponse.FreelancerId, existingResponse.ProjectId);
                var responses = await _responseService.GetProjectResponse(existingResponse.ProjectId);
                foreach (var response in responses)
                {
                    await _responseService.Delete(response.Id);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the response.");
            }
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> DeleteResponse(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid response ID.");
            }

            try
            {
                var existingResponse = await _responseService.GetResponseById(id);
                if (existingResponse == null)
                {
                    return NotFound("Response not found.");
                }

                await _responseService.Delete(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the response.");
            }
        }
    }

}
