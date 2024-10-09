using Microsoft.AspNetCore.Mvc;
using SearchService.Service;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string term)
        {
            var projects = await _projectService.SearchProjects(term);
            return Ok(projects);
        }
    }
}
