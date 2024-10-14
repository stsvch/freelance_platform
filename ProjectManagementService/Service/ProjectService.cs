using ProjectManagementService.Model;

namespace ProjectManagementService.Service
{
    // Services/IProjectService.cs
    public interface IProjectService
    {
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> GetProjectAsync(int id);
        Task UpdateProjectAsync(Project project);
        Task DeleteProjectAsync(int id);
    }

    // Services/ProjectService.cs
    public class ProjectService : IProjectService
    {
        private readonly ProjectDbContext _context;
        private readonly IMessageBus _messageBus;

        public ProjectService(ProjectDbContext context, IMessageBus messageBus)
        {
            _context = context;
            _messageBus = messageBus;
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Отправляем сообщение о создании проекта через RabbitMQ
            await _messageBus.PublishAsync("ProjectCreated", project);

            return project;
        }

        public async Task<Project> GetProjectAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task UpdateProjectAsync(Project project)
        {
            project.UpdatedAt = DateTime.UtcNow;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
    }

}
