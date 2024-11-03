using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.Services
{
    public class ProfileService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ProfileService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<FreelancerProfile> GetFreelancerProfileAsync(int userid)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancer/{userid}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FreelancerProfile>(content);
            }
            return null; // Либо выбросьте исключение или обработайте ошибку другим образом
        }

        public async Task<ClientProfile> GetClientProfileAsync(int userid)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/client/{userid}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientProfile>(content);
            }
            return null;
        }

        public async Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancerId/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FreelancerProfile>(content);
            }
            return null; // Либо выбросьте исключение или обработайте ошибку другим образом
        }

        public async Task<ClientProfile> GetClientProfileByIdAsync(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/clientId/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ClientProfile>(content);
            }
            return null;
        }

        public async Task<List<ClientProfile>> GetClientsAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/clients?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ClientProfile>>(content);
            }
            return null;
        }

        public async Task<List<FreelancerProfile>> GetFreelancersAsync(string userId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancers?userId={userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<FreelancerProfile>>(content);
            }
            return null;
        }
    }

}
