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
        private readonly ProjectService _projectService;
        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
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
            var model = await _projectService.GetProjectsForFreelancerAsync(id);
            if (model == null || !model.Any())
            {
                TempData["ErrorMessage"] = "Не удалось получить проекты.";
            }
            return View("Index", model);
        }

        [HttpGet("client/{id}")]
        public async Task<IActionResult> Client(int id)
        {
            var model = await _projectService.GetProjectsForClientAsync(id);
            if (model == null || !model.Any())
            {
                TempData["ErrorMessage"] = "Не удалось получить проекты.";
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
            ProjectModel model = await _projectService.GetProject(id);

            return View(model);
        }


        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            ProjectModel model = await _projectService.GetProject(id);

            return View(model);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromForm] ProjectModel project)
        {
            var previousUrl = Request.Headers["Referer"].ToString();
            var id = HttpContext.Session.GetString("Id");
            await _projectService.Create(id, project);
            if (string.IsNullOrEmpty(previousUrl))
            {
                return RedirectToAction("Create");
            }

            return Redirect(previousUrl);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateProject(int id, [FromForm] ProjectModel project)
        {
            await _projectService.Update(id, project);

            return RedirectToAction("Index");
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitProject(int id)
        {
            var previousUrl = Request.Headers["Referer"].ToString();
            var project = await _projectService.GetProject(id);
            project.Status = "Finished";
            await _projectService.Update(id, project);
            if (string.IsNullOrEmpty(previousUrl))
            {
                return RedirectToAction("Index");
            }

            return Redirect(previousUrl);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var response = await _projectService.Delete(id);

            if (response.Action == "Delete")
            {
                if (response.Status == "Success")
                {
                    TempData["SuccessMessage"] = "Проект успешно!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка при проекта.";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {          
            List<ProjectModel> model = await _projectService.GetList();
            return View(model);
        }

        [HttpPost("find")]
        public async Task<IActionResult> Find([FromForm] string[] tags)
        {
            var model = await _projectService.Find(tags);
            return View("List", model);
        }
    }
}
