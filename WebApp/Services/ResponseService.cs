using Newtonsoft.Json;
using System.Text;
using WebApp.Models;

namespace WebApp.Services
{
    public class ResponseService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ResponseService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Response>> GetClientRespose(int clientId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ratingservice:8080/api/response/client/{clientId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Response>>(content);
            }
            return null;
        }

        public async Task<List<Response>> GetFreelancerRespose(int freelancerId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ratingservice:8080/api/response/freelancer/{freelancerId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Response>>(content);
            }
            return null;
        }

        public async Task<List<Response>> GetProjectRespose(int projectId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"http://ratingservice:8080/api/response/project/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Response>>(content);
            }
            return null;
        }

        public async Task CreateResponse(Response newResponse)
        {
            var client = _httpClientFactory.CreateClient();

            var json = JsonConvert.SerializeObject(newResponse);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://ratingservice:8080/api/response/create", content);

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error creating response: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task DeleteRespose(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"http://ratingservice:8080/api/response/delete/{id}", content);

            if (response.IsSuccessStatusCode)
            {
            }
        }

        public async Task AcceptRespose(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"http://ratingservice:8080/api/response/accept/{id}", content);

            if (response.IsSuccessStatusCode)
            {
            }
        }

        public async Task CancelRespose(int id)
        {
            await DeleteRespose(id);
        }
    }
}
