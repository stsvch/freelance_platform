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

        public ResponseController(ResponseService responseService, ProjectService projectService)
        {
            _responseService = responseService;
            _projectService = projectService;   
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            switch(role)
            {
                case "Freelancer":
                    {
                        
                        var responses = await _responseService.GetFreelancerRespose(int.Parse(HttpContext.Session.GetString("Id")));
                        List<ProjectModel> projects = new List<ProjectModel>();
                        foreach (var response in responses)
                        {
                            projects.Add(await _projectService.GetProject(response.ProjectId));
                        }
                        var model = (Responses: responses.AsEnumerable(), Projects : projects.AsEnumerable());
                        return View(model);
                    }
                case "Client":
                    {
                        var responses = await _responseService.GetClientRespose(int.Parse(HttpContext.Session.GetString("Id")));
                        List<ProjectModel> projects = new List<ProjectModel>();
                        foreach (var response in responses)
                        {
                            projects.Add(await _projectService.GetProject(response.ProjectId));
                        }
                        var model = (Responses: responses.AsEnumerable(), Projects: projects.AsEnumerable());
                        return View(model);
                    }
                default: return RedirectToAction("Login", "Auth");
            }
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create(int projectId)
        {
            return View(projectId);
        }

        [HttpPost("create/response")]
        public async Task<IActionResult> CreateResponse([FromForm] Response response)
        {
            var freelancerId = int.Parse(HttpContext.Session.GetString("Id"));
            var client = await _projectService.GetProject(response.ProjectId);
            Response newResponse = new Response()
            {
                Message = response.Message,
                FreelancerId = freelancerId, 
                ProjectId = response.ProjectId,
                ClientId = client.ClientId,
            };
            await _responseService.CreateResponse(newResponse);
            return RedirectToAction("List", "Project");
        }

        [HttpPost("accept/{id}")]
        public async Task<IActionResult> Accept(int id)
        {
            await _responseService.AcceptRespose(id);
            return RedirectToAction("Index");
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            await _responseService.CancelRespose(id);
            return RedirectToAction("Index");
        }

    }
}
