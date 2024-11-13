using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RatingService.Model;

namespace RatingService.Service
{
    public class ResponseService
    {
        private readonly ReviewDbContext _context;
        private readonly IMessageBus _rabbitMqService;

        public ResponseService(ReviewDbContext context, IMessageBus rabbitMqService)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
        }

        public async Task<List<Response>> GetClientResponse(int clientId)
        {
            return await _context.Responses
                .Where(c => c.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<List<Response>> GetFreelancerResponse(int freelancerId)
        {
            return await _context.Responses
                .Where(c => c.FreelancerId == freelancerId)
                .ToListAsync();
        }

        public async Task<List<Response>> GetProjectResponse(int projectId)
        {
            return await _context.Responses
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Response> CreateResponse(Response response)
        {
            var existingResponse = await _context.Responses
                .FirstOrDefaultAsync(r => r.FreelancerId == response.FreelancerId && r.ProjectId == response.ProjectId);

            if (existingResponse != null)
            {
                throw new InvalidOperationException("the response already exists");
            }

            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            return response;
        }


        public async Task<Response> UpdateResponse(Response updatedResponse)
        {
            var response = await _context.Responses.FindAsync(updatedResponse.Id);
            if (response == null)
            {
                return null; 
            }

            response.Message = updatedResponse.Message ?? response.Message;
            response.Status = updatedResponse.Status ?? response.Status;
            response.FreelancerId = updatedResponse.FreelancerId != 0 ? updatedResponse.FreelancerId : response.FreelancerId;
            response.ClientId = updatedResponse.ClientId != 0 ? updatedResponse.ClientId : response.ClientId;
            response.ProjectId = updatedResponse.ProjectId != 0 ? updatedResponse.ProjectId : response.ProjectId;

            await _context.SaveChangesAsync();
            return response; 
        }

        public async Task SendAcceptMessage(int freelancerId, int projectId)
        {
            var message = new
            {
                Action = "Accept",
                CorrelationId = Guid.NewGuid().ToString(),
                ProjectId = projectId,
                FreelancerId = freelancerId
            };

            await _rabbitMqService.PublishAsync(JsonConvert.SerializeObject(message));
        }

        public async Task SendCreateMessage(int clientId)
        {
            var message = new
            {
                Action = "CreateResponse",
                CorrelationId = Guid.NewGuid().ToString(),
                ClientId = clientId
            };

            await _rabbitMqService.PublishAsync(JsonConvert.SerializeObject(message));
        }

        public async Task Delete(int responseId)
        {
            var response = await _context.Responses.FindAsync(responseId);
            if (response != null)
            {
                _context.Responses.Remove(response);
                await _context.SaveChangesAsync();
            }
        }

        internal async Task<Response> GetResponseById(int id)
        {
            return await _context.Responses.FindAsync(id);
        }
    }
}
