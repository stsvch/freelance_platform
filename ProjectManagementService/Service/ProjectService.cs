using Newtonsoft.Json;
using ProjectManagementService.Model;

namespace ProjectManagementService.Service
{
    // Services/IProjectService.cs
    public interface IProjectService
    {
        Task CreateProjectAsync(dynamic projectMessage);
        Task UpdateProjectAsync(dynamic projectMessage);
        Task DeleteProjectAsync(dynamic projectMessage);
    }

    public class ProjectService : IProjectService
    {
        private readonly ProjectDbContext _context;
        private readonly IMessageBus _messageBus;

        public ProjectService(ProjectDbContext context, IMessageBus messageBus)
        {
            _context = context;
            _messageBus = messageBus;
            _messageBus.ListenForMessages("ProjectQueue", HandleProjectMessage);
        }

        private async Task HandleProjectMessage(string message)
        {
            var projectMessage = JsonConvert.DeserializeObject<dynamic>(message);
            var action = projectMessage.action.ToString(); // Проверяем, что за действие

            switch (action)
            {
                case "create":
                    await CreateProjectAsync(projectMessage);
                    break;
                case "update":
                    await UpdateProjectAsync(projectMessage);
                    break;
                case "delete":
                    await DeleteProjectAsync(projectMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }

        // Создание проекта
        public async Task CreateProjectAsync(dynamic projectMessage)
        {
            var correlationId = projectMessage.correlationId.ToString();

            try
            {
                var project = new Project
                {
                    Title = projectMessage.title,
                    Description = projectMessage.description,
                    ClientId = projectMessage.clientId,
                    FreelancerId = projectMessage.freelancerId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var successMessage = new
                {
                    status = "Success",
                    projectId = project.Id,
                    correlationId = correlationId,
                    Message = "Проект успешно создан"
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
            }
            catch (Exception ex)
            {
                var errorMessage = new
                {
                    status = "Error",
                    correlationId = correlationId,
                    Message = ex.Message
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
            }
        }

        public async Task UpdateProjectAsync(dynamic projectMessage)
        {
            var correlationId = projectMessage.correlationId.ToString();

            try
            {
                var project = await _context.Projects.FindAsync((int)projectMessage.projectId);
                if (project == null)
                    throw new Exception("Проект не найден");

                project.Title = projectMessage.title ?? project.Title;
                project.Description = projectMessage.description ?? project.Description;
                project.UpdatedAt = DateTime.UtcNow;

                _context.Projects.Update(project);
                await _context.SaveChangesAsync();

                var successMessage = new
                {
                    Status = "Success",
                    ProjectId = project.Id,
                    CorrelationId = correlationId,
                    Message = "Проект успешно обновлен"
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
            }
            catch (Exception ex)
            {
                var errorMessage = new
                {
                    Status = "Error",
                    CorrelationId = correlationId,
                    Message = ex.Message
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
            }
        }

        public async Task DeleteProjectAsync(dynamic projectMessage)
        {
            var correlationId = projectMessage.correlationId.ToString();

            try
            {
                var project = await _context.Projects.FindAsync((int)projectMessage.projectId);
                if (project == null)
                    throw new Exception("Проект не найден");

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                var successMessage = new
                {
                    Status = "Success",
                    ProjectId = project.Id,
                    CorrelationId = correlationId,
                    Message = "Проект успешно удален"
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
            }
            catch (Exception ex)
            {
                var errorMessage = new
                {
                    Status = "Error",
                    CorrelationId = correlationId,
                    Message = ex.Message
                };
                await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
            }
        }
    }
}

