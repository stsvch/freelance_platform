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
            try 
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancer/{userid}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FreelancerProfile>(content);
                }
                return new FreelancerProfile();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new FreelancerProfile();
            }
        }

        public async Task<ClientProfile> GetClientProfileAsync(int userid)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/client/{userid}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ClientProfile>(content);
                }
                return new ClientProfile();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new ClientProfile();
            }
        }

        public async Task<FreelancerProfile> GetFreelancerProfileByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancerId/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FreelancerProfile>(content);
                }
                return new FreelancerProfile();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return new FreelancerProfile();
            }
        }

        public async Task<ClientProfile> GetClientProfileByIdAsync(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/clientId/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ClientProfile>(content);
                }
                return new ClientProfile();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message );
                return new ClientProfile();
            }
        }

        public async Task<List<ClientProfile>> GetClientsAsync(string userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/clients?userId={userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ClientProfile>>(content);
                }
                return new List<ClientProfile>();
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<ClientProfile>();
            }
        }

        public async Task<List<FreelancerProfile>> GetFreelancersAsync(string userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://localhost:7145/api/profile/freelancers?userId={userId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<FreelancerProfile>>(content);
                }
                return new List<FreelancerProfile>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message );
                return new List<FreelancerProfile>();
            }
        }
    }

}
