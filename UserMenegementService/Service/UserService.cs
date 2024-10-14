using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserMenegementService.Model;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json;

namespace UserMenegementService.Service
{
    public interface IUserService
    {
        public Task<IdentityResult> RegisterUserAsync(UserRegisterModel model);
        public Task<string> AuthenticateAsync(UserLoginModel model);
        public Task NotifyUserRegistered(string email);
    }

    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly RabbitMqService _rabbitMqService;

        public UserService(UserDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, RabbitMqService rabbitMqService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _rabbitMqService = rabbitMqService;
        }

        // Метод для регистрации пользователя
        public async Task<IdentityResult> RegisterUserAsync(UserRegisterModel model)
        {
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow,
                Role = model.Role,
                PasswordHash = _passwordHasher.HashPassword(null, model.Password) // создаем hash пароля
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        // Метод для аутентификации пользователя
        public async Task<string> AuthenticateAsync(UserLoginModel model)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
            {
                return null;
            }

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Метод для отправки уведомления через RabbitMQ (например, о регистрации)
        public async Task NotifyUserRegistered(string email)
        {
            var message = new { Email = email };
            await _rabbitMqService.PublishMessageAsync(message, "UserRegisteredQueue");
        }

        public void StartListening()
        {
            ListenForUserRegistrationMessages();
            ListenForUserLoginMessages();
        }

        // Обработка сообщений для регистрации пользователей
        public void ListenForUserRegistrationMessages()
        {
            _rabbitMqService.ListenForMessages("UserRegistrationQueue", async (message) =>
            {
                var userRegisterModel = JsonConvert.DeserializeObject<UserRegisterModel>(message);

                // Обработка регистрации пользователя
                var result = await RegisterUserAsync(userRegisterModel);
                if (result.Succeeded)
                {
                    // Отправляем уведомление об успешной регистрации в другую очередь
                    await _rabbitMqService.PublishMessageAsync(new { Email = userRegisterModel.Email }, "UserRegisteredResponseQueue");
                }
            });
        }

        // Обработка сообщений для авторизации пользователей
        public void ListenForUserLoginMessages()
        {
            _rabbitMqService.ListenForMessages("UserLoginQueue", (message) =>
            {
                var userLoginModel = JsonConvert.DeserializeObject<UserLoginModel>(message);

                // Логика аутентификации пользователя
                var token = AuthenticateAsync(userLoginModel).Result;

                // Отправляем результат аутентификации обратно в очередь
                if (token != null)
                {
                    _rabbitMqService.PublishMessageAsync(new { Token = token }, "UserLoginResponseQueue");
                }
            });
        }
    }

}
