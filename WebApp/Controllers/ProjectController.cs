using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("project")]
    public class ProjectController : Controller
    {
        private readonly RabbitMqService _rabbitMqService;

        public ProjectController(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role")=="Freelancer") return RedirectToAction("Freelancer", new { id = HttpContext.Session.GetString("Id") });
            else return RedirectToAction("Client", new { id = HttpContext.Session.GetString("Id") });
        }

        [HttpGet("freelancer/{id}")]
        public async Task<IActionResult> Freelancer(int id)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "GetAllClient",
                ClientId = id,
                CorrelationId = correlationId
            });

            List<ProjectModel> model = new List<ProjectModel>();

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            var responseMessage = await _rabbitMqService.WaitForMessageAsync("ProjectResponseQueue");
            if (!string.IsNullOrEmpty(responseMessage))
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.Action == "GetAllClient" && response.CorrelationId == correlationId)
                {
                    if (response.Status == "Success")
                    {
                        var projects = response.Projects;
                        foreach (var project in projects)
                        {
                            model.Add(new ProjectModel
                            {
                                Id = project.Id,
                                Title = project.Title,
                                Description = project.Description,
                                Budget = project.Budget,
                                ClientId = project.ClientId,
                                FreelancerId = project.FreelancerId,
                                CreatedAt = project.CreatedAt,
                                UpdatedAt = project.UpdatedAt
                            });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при получении списка проектов.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Не удалось получить проекты.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось получить ответ от сервиса.";
            }
            return View("Index", model);
        }

        [HttpGet("client/{id}")]
        public async Task<IActionResult> Client(int id)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "GetAllClient",
                ClientId = id,
                CorrelationId = correlationId
            });

            List<ProjectModel> model = new List<ProjectModel>();

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            var responseMessage = await _rabbitMqService.WaitForMessageAsync("ProjectResponseQueue");
            if (!string.IsNullOrEmpty(responseMessage))
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.Action == "GetAllClient" && response.CorrelationId == correlationId)
                {
                    if (response.Status == "Success")
                    {
                        var projects = response.Projects;

                        foreach (var project in projects)
                        {
                            model.Add(new ProjectModel
                            {
                                Id = project.Id,
                                Title = project.Title,
                                Description = project.Description,
                                Budget = project.Budget,
                                ClientId = project.ClientId,
                                FreelancerId = project.FreelancerId,
                                CreatedAt = project.CreatedAt,
                                UpdatedAt = project.UpdatedAt
                            });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при получении списка проектов.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Не удалось получить проекты.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось получить ответ от сервиса.";
            }
            return View("Index", model);
        }


        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Get",
                ProjectId = id,
                CorrelationId = correlationId
            });

            ProjectModel model = new ProjectModel();

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);
            var responseMessage = await _rabbitMqService.WaitForMessageAsync("ProjectResponseQueue");

            if (!string.IsNullOrEmpty(responseMessage))
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.Action == "Get" && response.CorrelationId == correlationId)
                {
                    if (response.Status == "Success")
                    {
                        model.Title = response.Project.Title;
                        model.FreelancerId = response.Project.FreelancerId;
                        model.Budget = response.Project.Budget;
                        model.ClientId = response.Project.ClientId;
                        model.Description = response.Project.Description;
                        model.CreatedAt = response.Project.CreatedAt;
                        model.UpdatedAt = response.Project.UpdatedAt;

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при обновлении проекта.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Не удалось получить проект.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Не удалось получить ответ от сервиса.";
            }

            return View(model);
        }


        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromForm] ProjectModel project)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Create",
                Title = project.Title,
                Budget = project.Budget,
                Description = project.Description,
                ClientId = 1,
                FreelancerId = (int?)null,
                CorrelationId = correlationId
            });
            Console.WriteLine(correlationId);
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            var responseMessage = await _rabbitMqService.WaitForMessageAsync("ProjectResponseQueue");
            var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);

            if (response.Action == "Create" && response.CorrelationId == correlationId)
            {
                if (response.Status == "Success")
                {
                    TempData["SuccessMessage"] = "Проект успешно создан!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка при создании проекта.";
                }
            }

            return RedirectToAction("Create");
        }

        [HttpPost("update/{id}")]
        public IActionResult UpdateProject(int id, [FromForm] ProjectModel project)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Update",
                ProjectId = id,
                Title = project.Title,
                Budget = project.Budget,
                Description = project.Description,
                CorrelationId = correlationId
            });

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.Action == "Update" && response.CorrelationId == correlationId)
                {
                    if (response.Status == "Success")
                    {
                        TempData["SuccessMessage"] = "Проект успешно обновлен!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при обновлении проекта.";
                    }
                }
            });

            return RedirectToAction("Update", new { id });
        }

        [HttpPost("delete/{id}")]
        public IActionResult DeleteProject(int id)
        {
            var correlationId = Guid.NewGuid().ToString();
            var message = JsonConvert.SerializeObject(new
            {
                Action = "Delete",
                ProjectId = id,
                CorrelationId = correlationId
            });

            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.Action == "Delete" && response.CorrelationId == correlationId)
                {
                    if (response.Status == "Success")
                    {
                        TempData["SuccessMessage"] = "Проект успешно удален!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при удалении проекта.";
                    }
                }
            });

            return RedirectToAction("Delete", new { id });
        }
    }
}
