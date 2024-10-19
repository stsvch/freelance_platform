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
                case "Get":
                    await GetProjectAsync(projectMessage);
                    break;
                case "GetAllClient":
                    await GetProjectsByClientIdAsync(projectMessage);
                    break;
                case "GetAllFreelancer":
                    await GetProjectsByFreelancerIdAsync(projectMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }

        public async Task GetProjectsByFreelancerIdAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int freelancerId = projectMessage.FreelancerId;
                        var projects = await context.Projects
                                                    .Where(p => p.FreelancerId == freelancerId)
                                                    .ToListAsync();

                        if (projects.Any())
                        {
                            var successMessage = new
                            {
                                Action = "GetAllFreelancert",
                                Status = "Success",
                                CorrelationId = projectMessage.CorrelationId,
                                Projects = projects.Select(project => new
                                {
                                    project.Id,
                                    project.Title,
                                    project.Description,
                                    project.Budget,
                                    project.ClientId,
                                    project.FreelancerId,
                                    project.CreatedAt,
                                    project.UpdatedAt
                                }).ToList()
                            };

                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        }
                        else
                        {
                            var errorMessage = new
                            {
                                Action = "GetAllFreelancer",
                                Status = "Error",
                                CorrelationId = projectMessage.CorrelationId.ToString(),
                                Message = "Проекты не найдены"
                            };
                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = new
                        {
                            Action = "GetAllFreelancer",
                            Status = "Error",
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = ex.Message
                        };

                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        await transaction.RollbackAsync();
                    }
                }
            }
        }

        public async Task GetProjectsByClientIdAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int clientId = projectMessage.ClientId;
                        var projects = await context.Projects
                                                    .Where(p => p.ClientId == clientId)
                                                    .ToListAsync();

                        if (projects.Any())
                        {
                            // Формируем успешное сообщение с проектами
                            var successMessage = new
                            {
                                Action = "GetAllClient",
                                Status = "Success",
                                CorrelationId = projectMessage.CorrelationId,
                                Projects = projects.Select(project => new
                                {
                                    project.Id,
                                    project.Title,
                                    project.Description,
                                    project.Budget,
                                    project.ClientId,
                                    project.FreelancerId,
                                    project.CreatedAt,
                                    project.UpdatedAt
                                }).ToList() 
                            };

                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        }
                        else
                        {
                            var errorMessage = new
                            {
                                Action = "GetAllClient",
                                Status = "Error",
                                CorrelationId = projectMessage.CorrelationId.ToString(),
                                Message = "Проекты не найдены для данного ClientId"
                            };
                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = new
                        {
                            Action = "GetAllClient",
                            Status = "Error",
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = ex.Message
                        };

                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        await transaction.RollbackAsync();
                    }
                }
            }
        }


        public async Task GetProjectAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        int projectId = projectMessage.ProjectId;

                        var project = await context.Projects
                                                   .Where(p => p.Id == projectId)
                                                   .FirstOrDefaultAsync();

                        if (project != null)
                        {
                            var successMessage = new
                            {
                                Action = "Get",
                                Status = "Success",
                                CorrelationId = projectMessage.CorrelationId,
                                Project = new
                                {
                                    project.Id,
                                    project.Title,
                                    project.Description,
                                    project.Budget,
                                    project.ClientId,
                                    project.FreelancerId,
                                    project.CreatedAt,
                                    project.UpdatedAt
                                }
                            };

                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        }
                        else
                        {
                            var errorMessage = new
                            {
                                Action = "Get",
                                Status = "Error",
                                CorrelationId = projectMessage.CorrelationId.ToString(),
                                Message = "Проект не найден"
                            };
                            await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = new
                        {
                            Action = "Get",
                            Status = "Error",
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = ex.Message
                        };

                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(errorMessage));
                        await transaction.RollbackAsync();
                    }
                }
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

                        await transaction.CommitAsync(); 
                        var successMessage = new
                        {
                            Action = "Create",
                            Status = "Success",
                            ProjectId = project.Id,
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = "Проект успешно создан"
                        };
                        Console.WriteLine(projectMessage.CorrelationId.ToString());
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); 
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

                        await transaction.CommitAsync(); 
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
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

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); 
                        Console.WriteLine($"Ошибка при удалении проекта: {ex.Message}");
                    }
                }
            }
        }
    }

}

