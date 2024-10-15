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
                action = "create",
                title = project.Title,
                budget = project.Budget,
                description = project.Description,
                clientId = 1,
                freelancerId = (int?)null,
                correlationId = correlationId
            });

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.action == "create" && response.correlationId == correlationId)
                {
                    if (response.status == "success")
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
                action = "update",
                projectId = id,
                title = project.Title,
                budget = project.Budget,
                description = project.Description,
                correlationId = correlationId
            });

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.action == "update" && response.correlationId == correlationId)
                {
                    if (response.status == "success")
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
                action = "delete",
                projectId = id,
                correlationId = correlationId
            });

            // Отправляем сообщение в очередь RabbitMQ
            _rabbitMqService.PublishMessage("ProjectQueue", message, correlationId);

            // Ожидаем ответ от микросервиса
            _rabbitMqService.ListenForMessages("ProjectResponseQueue", (responseMessage) =>
            {
                var response = JsonConvert.DeserializeObject<dynamic>(responseMessage);
                if (response.action == "delete" && response.correlationId == correlationId)
                {
                    if (response.status == "success")
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
