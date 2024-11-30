using Moq;
using NUnit.Framework;
using UserMenegementService.Controllers;
using UserMenegementService.Service;
using UserMenegementService.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TestProjectUser
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<AuthController>> _mockLogger;
        private AuthController _controller;
        private ServiceProvider _serviceProvider;
        private UserDbContext _context;

        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

            _context = new UserDbContext(options);

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "RabbitMQ:HostName", "localhost" },
                    { "RabbitMQ:Port", "5672" },
                    { "RabbitMQ:UserName", "guest" },
                    { "RabbitMQ:Password", "guest" }
                }).Build())
                .AddSingleton<IUserService>(_mockUserService.Object)
                .AddDbContext<UserDbContext>(options => options.UseInMemoryDatabase("TestDatabase"))
                .AddScoped<ILogger<AuthController>>(_ => _mockLogger.Object)
                .BuildServiceProvider();

            // Создаем контроллер
            _controller = new AuthController(_mockUserService.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Очищаем ресурсы, если необходимо
            _context?.Dispose();
            _serviceProvider?.Dispose();
        }

        [Test]
        public async Task Login_ReturnsOk_WhenUserIsAuthenticated()
        {
            // Arrange
            var loginModel = new UserLogin { Email = "test@example.com", PasswordHash = "password" };

            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = "password",
                Role = "Client",
                ClientProfile = new ClientProfile
                {
                    Id = 1,
                    CompanyName = "Test Company",
                    Description = "A test client profile"
                }
            };

            _mockUserService.Setup(service => service.AuthenticateUserAsync("test@example.com", "password"))
                .ReturnsAsync(user);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = await _controller.Login(loginModel);

            Assert.IsInstanceOf<OkObjectResult>(response);
            var okResult = response as OkObjectResult;

            var result = JObject.FromObject(okResult.Value);

            Assert.AreEqual(1, (int)result["UserId"]);
            Assert.AreEqual("Client", (string)result["Role"]);
            Assert.AreEqual(1, (int)result["Id"]);
        }
    }
}

