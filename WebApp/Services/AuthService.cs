using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using WebApp.Models;
using Microsoft.Extensions.Logging;

namespace WebApp.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IHttpClientFactory httpClientFactory, ILogger<AuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<dynamic?> Login(UserLogin model)
        {
            try
            {
                var passwordHash = HashPassword.Hash(model.PasswordHash);
                var loginRequest = new
                {
                    Email = model.Email,
                    PasswordHash = passwordHash
                };

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://127.0.0.1:7145/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var contentResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<dynamic>(contentResponse);
                }
                else
                {
                    _logger.LogWarning("Ошибка входа: получен статус {StatusCode}. Причина: {Reason}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Сетевая ошибка при попытке входа пользователя с email: {Email}", model.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при входе.");
                return null;
            }
        }

        public async Task<dynamic?> Register(UserRegister model)
        {
            try
            {
                string message;
                if (model.Role == "Client")
                {
                    message = JsonConvert.SerializeObject(new
                    {
                        Username = model.Username,
                        PasswordHash = HashPassword.Hash(model.PasswordHash),
                        Email = model.Email,
                        Role = model.Role,
                        Description = model.Description,
                        CompanyName = model.CompanyName
                    });
                }
                else
                {
                    message = JsonConvert.SerializeObject(new
                    {
                        Username = model.Username,
                        PasswordHash = HashPassword.Hash(model.PasswordHash),
                        Email = model.Email,
                        Role = model.Role,
                        Skills = model.Skills,
                        Bio = model.Bio
                    });
                }

                var client = _httpClientFactory.CreateClient();
                var content = new StringContent(message, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://usermenegementservice:8080/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    var contentResponse = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<dynamic>(contentResponse);
                }
                else
                {
                    _logger.LogWarning("Ошибка регистрации: получен статус {StatusCode}. Причина: {Reason}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Сетевая ошибка при попытке регистрации пользователя с email: {Email}", model.Email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при регистрации.");
                return null;
            }
        }
    }
}


