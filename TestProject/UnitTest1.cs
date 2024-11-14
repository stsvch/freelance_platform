using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using ProjectManagementService.Model;
using ProjectManagementService.Service;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ProjectServiceTests
{
    [Test]
    public async Task FindProject_ShouldPublishProjectDetails_WhenProjectExists()
    {
        var projectData = new List<Project>
        {
            new Project { Id = 1, Title = "Project 1", Description = "Tag1 Tag2", Budget = 1000 },
            new Project { Id = 2, Title = "Project 2", Description = "Tag2 Tag3", Budget = 2000 },
            new Project { Id = 3, Title = "Project 3", Description = "Tag1 Tag4", Budget = 3000 }
        };

        var mockConnection = new Mock<IConnection>();
        var mockModel = new Mock<IModel>();
        var mockMessageBus = new Mock<IMessageBus>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "RabbitMQ:HostName", "localhost" },
                { "RabbitMQ:Port", "5672" },
                { "RabbitMQ:UserName", "guest" },
                { "RabbitMQ:Password", "guest" }
            })
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddDbContext<ProjectDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase")
                       .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)))
            .AddSingleton(mockConnection.Object)
            .AddSingleton(mockModel.Object)
            .AddSingleton(mockMessageBus.Object)  
            .AddScoped<IProjectService, ProjectService>()
            .BuildServiceProvider();

        var dbContext = serviceProvider.GetRequiredService<ProjectDbContext>();
        dbContext.Projects.AddRange(projectData);
        await dbContext.SaveChangesAsync();

        var projectService = serviceProvider.GetRequiredService<IProjectService>();

        var projectMessage = new
        {
            Action = "Get",
            ProjectId = 1,
            CorrelationId = "test-correlation-id",
        };

        await projectService.HandleProjectMessage(JsonConvert.SerializeObject(projectMessage));

        mockMessageBus.Verify(bus => bus.PublishAsync(
            "ProjectResponseQueue",
            It.Is<string>(message =>
                message.Contains("\"Action\":\"Get\"") &&
                message.Contains("\"Status\":\"Success\"") &&
                message.Contains("\"CorrelationId\":\"test-correlation-id\"") &&
                message.Contains("\"Id\":1") &&
                message.Contains("\"Title\":\"Project 1\""))
        ), Times.Once);
    }

    [Test]
    public async Task FindProject_ShouldPublishError_WhenProjectDoesNotExist()
    {
        var mockConnection = new Mock<IConnection>();
        var mockModel = new Mock<IModel>();
        var mockMessageBus = new Mock<IMessageBus>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "RabbitMQ:HostName", "localhost" },
                { "RabbitMQ:Port", "5672" },
                { "RabbitMQ:UserName", "guest" },
                { "RabbitMQ:Password", "guest" }
            })
            .Build();

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddDbContext<ProjectDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase")
                       .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)))
            .AddSingleton(mockConnection.Object)
            .AddSingleton(mockModel.Object)
            .AddSingleton(mockMessageBus.Object) 
            .AddScoped<IProjectService, ProjectService>()
            .BuildServiceProvider();

        var projectService = serviceProvider.GetRequiredService<IProjectService>();

        var projectMessage = new
        {
            Action = "Get",
            ProjectId = 999,
            CorrelationId = "test-correlation-id",
        };

        await projectService.HandleProjectMessage(JsonConvert.SerializeObject(projectMessage));

        mockMessageBus.Verify(bus => bus.PublishAsync(
            "ProjectResponseQueue",
            It.Is<string>(message =>
                message.Contains("\"Action\":\"Get\"") &&
                message.Contains("\"Status\":\"Error\"") &&
                message.Contains("\"CorrelationId\":\"test-correlation-id\"") &&
                message.Contains("\"Message\":\"Проект не найден\""))
        ), Times.Once);
    }
}
