using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [Route("response")]
    public class ResponseController : Controller
    {
        private readonly ResponseService _responseService;
        private readonly ProjectService _projectService;
        private readonly ILogger<ResponseController> _logger;

        public ResponseController(ResponseService responseService, ProjectService projectService, ILogger<ResponseController> logger)
        {
            _responseService = responseService;
            _projectService = projectService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");

                switch (role)
                {
                    case "Freelancer":
                        _logger.LogInformation("Загрузка откликов для фрилансера с ID {FreelancerId}", HttpContext.Session.GetString("Id"));
                        var freelancerResponses = await _responseService.GetFreelancerRespose(int.Parse(HttpContext.Session.GetString("Id")));
                        List<ProjectModel> freelancerProjects = new List<ProjectModel>();
                        foreach (var response in freelancerResponses)
                        {
                            freelancerProjects.Add(await _projectService.GetProject(response.ProjectId));
                        }
                        var freelancerModel = (Responses: freelancerResponses.AsEnumerable(), Projects: freelancerProjects.AsEnumerable());
                        return View(freelancerModel);

                    case "Client":
                        _logger.LogInformation("Загрузка откликов для клиента с ID {ClientId}", HttpContext.Session.GetString("Id"));
                        var clientResponses = await _responseService.GetClientRespose(int.Parse(HttpContext.Session.GetString("Id")));
                        List<ProjectModel> clientProjects = new List<ProjectModel>();
                        foreach (var response in clientResponses)
                        {
                            clientProjects.Add(await _projectService.GetProject(response.ProjectId));
                        }
                        var clientModel = (Responses: clientResponses.AsEnumerable(), Projects: clientProjects.AsEnumerable());
                        return View(clientModel);

                    default:
                        _logger.LogWarning("Неизвестная роль пользователя, перенаправление на страницу входа.");
                        return RedirectToAction("Login", "Auth");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке откликов.");
                return RedirectToAction("Logout", "Auth");
            }
        }

        [HttpGet("create")]
        public IActionResult Create(int projectId)
        {
            try
            {
                _logger.LogInformation("Переход к созданию отклика для проекта с ID {ProjectId}", projectId);
                return View(projectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке страницы создания отклика для проекта с ID {ProjectId}", projectId);
                return RedirectToAction("Index");
            }
        }

        [HttpPost("create/response")]
        public async Task<IActionResult> CreateResponse([FromForm] Response response)
        {
            try
            {
                var freelancerId = int.Parse(HttpContext.Session.GetString("Id"));
                var client = await _projectService.GetProject(response.ProjectId);

                if (client == null)
                {
                    _logger.LogWarning("Проект с ID {ProjectId} не найден", response.ProjectId);
                    return NotFound("Проект не найден.");
                }

                Response newResponse = new Response()
                {
                    Message = response.Message,
                    FreelancerId = freelancerId,
                    ProjectId = response.ProjectId,
                    ClientId = client.ClientId,
                };

                await _responseService.CreateResponse(newResponse);

                _logger.LogInformation("Отклик для проекта с ID {ProjectId} успешно создан фрилансером с ID {FreelancerId}", response.ProjectId, freelancerId);
                return RedirectToAction("List", "Project");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании отклика для проекта с ID {ProjectId}", response.ProjectId);
                return RedirectToAction("Index");
            }
        }

        [HttpPost("accept/{id}")]
        public async Task<IActionResult> Accept(int id)
        {
            try
            {
                _logger.LogInformation("Принятие отклика с ID {ResponseId}", id);
                await _responseService.AcceptRespose(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при принятии отклика с ID {ResponseId}", id);
                return RedirectToAction("Index");
            }
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                _logger.LogInformation("Отмена отклика с ID {ResponseId}", id);
                await _responseService.CancelRespose(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене отклика с ID {ResponseId}", id);
                return RedirectToAction("Index");
            }
        }
    }
}

