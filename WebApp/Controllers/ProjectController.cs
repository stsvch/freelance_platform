using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("project")]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ProjectService projectService, ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Freelancer")
                    return RedirectToAction("Freelancer", new { id = HttpContext.Session.GetString("Id") });
                else
                    return RedirectToAction("Client", new { id = HttpContext.Session.GetString("Id") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while navigating to Index");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке страницы.";
                return View();
            }
        }

        [HttpGet("freelancer/{id}")]
        public async Task<IActionResult> Freelancer(int id)
        {
            try
            {
                var model = await _projectService.GetProjectsForFreelancerAsync(id);
                if (model == null || !model.Any())
                {
                    ViewBag.ErrorMessage = "Не удалось получить проекты.";
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching freelancer projects for user {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке проектов.";
                return View("Index");
            }
        }

        [HttpGet("client/{id}")]
        public async Task<IActionResult> Client(int id)
        {
            try
            {
                var model = await _projectService.GetProjectsForClientAsync(id);
                if (model == null || !model.Any())
                {
                    ViewBag.ErrorMessage = "Не удалось получить проекты.";
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching client projects for user {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке проектов.";
                return View("Index");
            }
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet("update/{id}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                ProjectModel model = await _projectService.GetProject(id);
                if (model == null)
                {
                    ViewBag.ErrorMessage = "Проект не найден.";
                    return RedirectToAction("List");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching project with id {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке проекта.";
                return RedirectToAction("List");
            }
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                ProjectModel model = await _projectService.GetProject(id);
                if (model == null)
                {
                    ViewBag.ErrorMessage = "Проект не найден.";
                    return RedirectToAction("List");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching project with id {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке проекта.";
                return RedirectToAction("List");
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromForm] ProjectModel project)
        {
            try
            {
                var previousUrl = Request.Headers["Referer"].ToString();
                var id = HttpContext.Session.GetString("Id");
                await _projectService.Create(id, project);
                ViewBag.SuccessMessage = "Проект успешно создан!";
                if (string.IsNullOrEmpty(previousUrl))
                {
                    return RedirectToAction("Create");
                }

                return Redirect(previousUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating project");
                ViewBag.ErrorMessage = "Произошла ошибка при создании проекта.";
                return View("Create", project);
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateProject(int id, [FromForm] ProjectModel project)
        {
            try
            {
                await _projectService.Update(id, project);
                ViewBag.SuccessMessage = "Проект успешно обновлен!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while updating project with id {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при обновлении проекта.";
                return View("Update", project);
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit(int id)
        {
            try
            {
                var project = await _projectService.GetProject(id);
                project.Status = "Finished";
                await _projectService.Update(id, project);
                ViewBag.SuccessMessage = "Проект успешно завершен!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while submitting project with id {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при завершении проекта.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var response = await _projectService.Delete(id);

                if (response.Action == "Delete")
                {
                    if (response.Status == "Success")
                    {
                        ViewBag.SuccessMessage = "Проект успешно удален!";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Ошибка при удалении проекта.";
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while deleting project with id {id}");
                ViewBag.ErrorMessage = "Произошла ошибка при удалении проекта.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<ProjectModel> model = await _projectService.GetList();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching the project list");
                ViewBag.ErrorMessage = "Произошла ошибка при загрузке списка проектов.";
                return View();
            }
        }

        [HttpPost("find")]
        public async Task<IActionResult> Find(string tags)
        {
            try
            {
                if (!string.IsNullOrEmpty(tags))
                {
                    var tagsArray = tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                         .Select(t => t.Trim())
                                         .ToArray();

                    var model = await _projectService.Find(tagsArray);
                    return View("List", model);
                }
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while searching for projects");
                ViewBag.ErrorMessage = "Произошла ошибка при поиске проектов.";
                return RedirectToAction("List");
            }
        }
    }
}

