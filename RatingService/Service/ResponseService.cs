using Microsoft.EntityFrameworkCore;
using RatingService.Model;

namespace RatingService.Service
{
    public class ResponseService
    {
        private readonly ReviewDbContext _context;
        private readonly RabbitMqService _rabbitMqService;

        public ResponseService(ReviewDbContext context, RabbitMqService rabbitMqService)
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

        public async Task<List<Response>> GetProjectResponse(int projectId)
        {
            return await _context.Responses
                .Where(r => r.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<Response> CreateResponse(Response response)
        {
            _context.Responses.Add(response);
            await _context.SaveChangesAsync();

            return response;
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
        public void StartListeningForProjectCreated()
        {
            _rabbitMqService.ListenForMessages("ProjectCreatedQueue", async (message) =>
            {
            });
        }
    }
}
