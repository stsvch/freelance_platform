using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProjectController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Поиск проектов для фрилансеров
        [HttpGet]
        public async Task<IActionResult> SearchProjects()
        {
            var client = _httpClientFactory.CreateClient("SearchService");
            var response = await client.GetAsync("projects/search");

            if (response.IsSuccessStatusCode)
            {
                var projects = await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
                return View(projects);
            }

            return RedirectToAction("Error");
        }

        // Страница с проектами, которые фрилансер взял в работу
        [HttpGet]
        public async Task<IActionResult> FreelancerProjects()
        {
            var client = _httpClientFactory.CreateClient("UserManagementService");
            var response = await client.GetAsync("freelancer/currentProjects");

            if (response.IsSuccessStatusCode)
            {
                var projects = await response.Content.ReadFromJsonAsync<List<ProjectModel>>();
                return View(projects);
            }

            return RedirectToAction("Error");
        }

        // Создание проекта для заказчика
        [HttpPost]
        public async Task<IActionResult> CreateProject(ProjectModel model)
        {
            var client = _httpClientFactory.CreateClient("ProjectManagementService");
            var response = await client.PostAsJsonAsync("projects", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ClientProfile", "Profile");
            }

            ModelState.AddModelError("", "Ошибка при создании проекта.");
            return View(model);
        }
    }

}
