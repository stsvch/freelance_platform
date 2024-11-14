using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectManagementService.Model;
using System.Diagnostics;

namespace ProjectManagementService.Service
{
    public interface IProjectService
    {
        Task CreateProjectAsync(dynamic projectMessage);
        Task UpdateProjectAsync(dynamic projectMessage);
        Task DeleteProjectAsync(dynamic projectMessage);
        Task HandleProjectMessage(string message);
        Task HandleResponseMessage(string message);

        Task GetProjectsByFreelancerIdAsync(dynamic projectMessage);
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
            _messageBus.ListenForMessages("ResponseToProjectQueue", async message =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
                    await projectService.HandleResponseMessage(message);
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
                case "GetList":
                    await GetProjectListAsync(projectMessage);
                    break;
                case "Find": 
                    await FindProject(projectMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }
        private async Task FindProject(dynamic message)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                string[] tags = JsonConvert.DeserializeObject<string[]>(message.Tags.ToString());
                tags = tags.Select(tag => tag.Trim('[', ']', '"')).ToArray();
                Console.WriteLine("Фильтрация по тегам:");
                foreach (var tag in tags)
                {
                    Console.WriteLine(tag); 
                }
                var correlationId = message.CorrelationId.ToString();
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var query = context.Projects.AsQueryable();
                        foreach (var tag in tags)
                        {
                            var tagLower = tag.ToLower().Trim();
                            query = query.Where(p => p.Description.ToLower().Contains(tagLower) && p.FreelancerId == null);
                        }
                        var projects = await query.ToListAsync();

                        if (projects == null || projects.Count == 0)
                            throw new Exception("Проекты не найдены");
                        var successMessage = new
                        {
                            Action = "Find",
                            Status = "Success",
                            CorrelationId = message.CorrelationId,
                            Projects = projects
                        };
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                        await transaction.RollbackAsync();
                    }
                }
            }
        }
        public async Task HandleResponseMessage(string message)
        {
            var responseMessage = JsonConvert.DeserializeObject<dynamic>(message);
            var action = responseMessage?.Action?.ToString();
            switch (action)
            {
                case "Accept":
                    await AcceptAsync(responseMessage);
                    break;
                default:
                    Console.WriteLine($"Неизвестное действие: {action}");
                    break;
            }
        }
        private async Task AcceptAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var project = await context.Projects.FindAsync((int)projectMessage.ProjectId);
                        if (project == null)
                            throw new Exception("Проект не найден");
                        project.FreelancerId = projectMessage.FreelancerId;

                        context.Projects.Update(project);
                        await context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                    }
                }
            }
        }
        private async Task GetProjectListAsync(dynamic projectMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();

                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var projects = await context.Projects
                                                    .Where(p => p.FreelancerId == null)
                                                    .ToListAsync();
                        if (projects.Any())
                        {
                            var successMessage = new
                            {
                                Action = "GetList",
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
                                Action = "GetList",
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
                            Action = "GetList",
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
                                                    .Where(p => p.FreelancerId == freelancerId && p.Status != "Finished")
                                                    .ToListAsync();
                        if (projects.Any())
                        {
                            var successMessage = new
                            {
                                Action = "GetAllFreelancer",
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
                                CorrelationId = projectMessage.CorrelationId,
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
                                                    .Where(p => p.ClientId == clientId && p.Status != "Finished")
                                                    .ToListAsync();
                        if (projects.Any())
                        {
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
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();
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
                        await context.SaveChangesAsync();
                                      
                        var successMessage = new
                        {
                            Action = "Create",
                            Status = "Success",
                            ProjectId = project.Id,
                            CorrelationId = projectMessage.CorrelationId,
                            Message = "Проект успешно создан"
                        };
                        
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        stopwatch.Stop();
                        Console.WriteLine($"Time taken for : {stopwatch.ElapsedMilliseconds} ms");
                        await transaction.CommitAsync();
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
                        var project = await context.Projects.FindAsync((int)projectMessage.ProjectId);
                        if (project == null)
                            throw new Exception("Проект не найден");
                        project.Status = projectMessage.Status ?? project.Status;
                        project.FreelancerId = projectMessage.FreelancerId ?? project.FreelancerId;
                        project.Title = projectMessage.Title ?? project.Title;
                        project.Description = projectMessage.Description ?? project.Description;
                        project.UpdatedAt = DateTime.UtcNow;

                        context.Projects.Update(project);
                        await context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        var successMessage = new
                        {
                            Action = "Update",
                            Status = "Success",
                            ProjectId = project.Id,
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = "Проект успешно обновлен"
                        };
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                    }
                    catch (Exception ex)
                    {
                        var message = new
                        {
                            Action = "Update",
                            Status = "Error",
                            CorrelationId = projectMessage.CorrelationId.ToString(),
                            Message = "Проект не обновлен"
                        };
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(message));
                        await transaction.RollbackAsync();
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
                        var project = await context.Projects.FindAsync((int)projectMessage.ProjectId);
                        if (project == null)
                            throw new Exception("Проект не найден");

                        context.Projects.Remove(project);
                        await context.SaveChangesAsync();
                        var successMessage = new
                        {
                            Action = "Delete",
                            Status = "Success",
                            CorrelationId = projectMessage.CorrelationId.ToString()
                        };
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        var successMessage = new
                        {
                            Action = "Delete",
                            Status = "Error",
                            CorrelationId = projectMessage.CorrelationId.ToString()
                        };
                        await _messageBus.PublishAsync("ProjectResponseQueue", JsonConvert.SerializeObject(successMessage));
                        Console.WriteLine($"Ошибка при удалении проекта: {ex.Message}");
                    }
                }
            }
        }
    }
}

