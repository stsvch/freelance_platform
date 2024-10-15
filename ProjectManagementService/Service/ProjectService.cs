using Microsoft.EntityFrameworkCore;
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
        Task HandleProjectMessage(string message);
    }
    public class ProjectService : IProjectService
    {
        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _serviceProvider;

        public ProjectService(IMessageBus messageBus, IServiceProvider serviceProvider)
        {
            _messageBus = messageBus;
            _serviceProvider = serviceProvider;
        }

        public void StartListeningForMessages()
        {
            _messageBus.ListenForMessages("ProjectQueue", async message =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
                    await projectService.HandleProjectMessage(message);
                }
            });
        }

        public async Task HandleProjectMessage(string message)
        {
            var projectMessage = JsonConvert.DeserializeObject<dynamic>(message);
            var action = projectMessage.Action.ToString();

            switch (action)
            {
                case "Create":
                    await CreateProjectAsync(projectMessage);
                    break;
                case "Update":
                    await UpdateProjectAsync(projectMessage);
                    break;
                case "Delete":
                    await DeleteProjectAsync(projectMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }

        public async Task CreateProjectAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var project = new Project
                        {
                            Title = projectMessage.Title,
                            Description = projectMessage.Description,
                            ClientId = projectMessage.ClientId,
                            FreelancerId = projectMessage.FreelancerId,
                            Budget = projectMessage.Budget,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.Projects.Add(project);
                        try
                        {
                            await context.SaveChangesAsync();
                        }
                        catch (DbUpdateException ex)
                        {
                            Console.WriteLine("Ошибка при сохранении изменений: " + ex.Message);
                            if (ex.InnerException != null)
                            {
                                Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                            }
                        }

                        await transaction.CommitAsync(); // Коммитим транзакцию
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Откатываем транзакцию при ошибке
                        Console.WriteLine($"Ошибка при создании проекта: {ex.Message}");
                    }
                }
            }
        }

        public async Task UpdateProjectAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var project = await context.Projects.FindAsync((int)projectMessage.projectId);
                        if (project == null)
                            throw new Exception("Проект не найден");

                        project.Title = projectMessage.title ?? project.Title;
                        project.Description = projectMessage.description ?? project.Description;
                        project.UpdatedAt = DateTime.UtcNow;

                        context.Projects.Update(project);
                        await context.SaveChangesAsync();

                        await transaction.CommitAsync(); // Коммитим транзакцию
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Откатываем транзакцию при ошибке
                        Console.WriteLine($"Ошибка при обновлении проекта: {ex.Message}");
                    }
                }
            }
        }

        public async Task DeleteProjectAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var project = await context.Projects.FindAsync((int)projectMessage.projectId);
                        if (project == null)
                            throw new Exception("Проект не найден");

                        context.Projects.Remove(project);
                        await context.SaveChangesAsync();

                        await transaction.CommitAsync(); // Коммитим транзакцию
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // Откатываем транзакцию при ошибке
                        Console.WriteLine($"Ошибка при удалении проекта: {ex.Message}");
                    }
                }
            }
        }
    }

}

