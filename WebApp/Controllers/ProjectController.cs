using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{

    [Route("project")]
    public class ProjectController : Controller
    {
        private readonly RabbitMqService _rabbitMqService;

        public ProjectController(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("update/{id}")]
        public IActionResult Update(int id)
        {
            return View();
        }

        [HttpGet("delete/{id}")]
        public IActionResult Delete(int id)
        {
            return View();
        }

        // POST: /Project/Create
        [HttpPost("create")]
        public IActionResult CreateProject([FromForm] ProjectModel project)
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

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                Console.WriteLine(response.ToString()); 
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
            });

            return RedirectToAction("Create");
        }

        // POST: /Project/Update
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

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
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

        // POST: /Project/Delete
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

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
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
