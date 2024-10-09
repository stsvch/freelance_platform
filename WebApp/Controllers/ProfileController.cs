using Microsoft.AspNetCore.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Страница профиля фрилансера
        [HttpGet]
        public async Task<IActionResult> FreelancerProfile()
        {
            var client = _httpClientFactory.CreateClient("UserManagementService");
            var response = await client.GetAsync("freelancer/profile");

            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<FreelancerProfileModel>();

                return View(profile);
            }

            return RedirectToAction("Error");
        }

        // Страница профиля заказчика
        [HttpGet]
        public async Task<IActionResult> ClientProfile()
        {
            var client = _httpClientFactory.CreateClient("UserManagementService");
            var response = await client.GetAsync("client/profile");

            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<ClientProfileModel>();
                return View(profile);
            }

            return RedirectToAction("Error");
        }
    }

}
