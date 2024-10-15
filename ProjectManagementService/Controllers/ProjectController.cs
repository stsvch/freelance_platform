using Microsoft.AspNetCore.Mvc;
using ProjectManagementService.Model;
using ProjectManagementService.Service;

namespace ProjectManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }
    }

}
