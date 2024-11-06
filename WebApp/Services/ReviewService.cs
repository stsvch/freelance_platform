using Newtonsoft.Json;
using System.Net;
using System.Text;
using WebApp.Models;

namespace WebApp.Services
{
    public class ReviewService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ReviewService(IHttpClientFactory httpClientFactory) 
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Review>> GetFreelacerReview(int freelancerId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7027/api/review/freelancer/{freelancerId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Review>>(content);
            }
            return null;
        }

        public async Task<List<Review>> GetProjectReview(int projectId)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://localhost:7027/api/review/project/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Review>>(content);
            }
            return null;
        }

        public async Task CreateReview(Review review)
        {
            var client = _httpClientFactory.CreateClient();

            var json = JsonConvert.SerializeObject(review);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:7027/api/review/create", content);

            if (response.IsSuccessStatusCode)
            {

            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error creating response: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task DeleteReview(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(id), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"https://localhost:7027/api/review/delete/{id}", content);

            if (response.IsSuccessStatusCode)
            {

            }

        }
    }
}
