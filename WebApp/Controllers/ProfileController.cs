using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    [ServiceFilter(typeof(RoleFilter))]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly ProfileService _profileService;
        private readonly ReviewService _reviewService;

        public ProfileController(ProfileService profileService, ReviewService reviewService)
        {
            _profileService = profileService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }
            object profileData = null; 

            switch (role)
            {
                case "Freelancer":
                    {
                        profileData = await _profileService.GetFreelancerProfileAsync(int.Parse(userId));
                        break;
                    }
                case "Client":
                    {
                        profileData = await _profileService.GetClientProfileAsync(int.Parse(userId));
                        break;
                    }
                default:
                    return RedirectToAction("Login", "Auth");
            }
            if (profileData!=null)
            {
                return View(profileData);
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View();
            }
        }

        [HttpGet("freelancer/{userId}")]
        public async Task<IActionResult> Freelancer(int userid)
        {
            if (userid != null)
            {
                var profileData = await _profileService.GetFreelancerProfileAsync(userid);

                return View("Index", profileData);
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View("Index");
            }
        }

        [HttpGet("client/{userId}")]
        public async Task<IActionResult> Client(int userId)
        {
            if (userId != null)
            {
                var profileData = await _profileService.GetClientProfileAsync(userId);
                return View("Index", profileData);
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить профиль. Попробуйте позже.";
                return View("Index");
            }
        }

        [HttpGet("clients")]
        public async Task<IActionResult> Clients()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "Не удалось получить UserId из сессии.";
                return View();
            }
            var clients = await _profileService.GetClientsAsync(userId);
            if (clients!=null)
            {
                return View(clients); 
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить список фрилансеров. Попробуйте позже.";
                return View();  
            }
        }

        [HttpGet("freelancers")]
        public async Task<IActionResult> Freelancers()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "Не удалось получить UserId из сессии.";
                return View();
            }
            var freelancers = await _profileService.GetFreelancersAsync(userId);
            if (freelancers != null)
            {
                return View(freelancers);
            }
            else
            {
                ViewBag.ErrorMessage = "Не удалось загрузить список фрилансеров. Попробуйте позже.";
                return View(); 
            }
        }

    }
}
