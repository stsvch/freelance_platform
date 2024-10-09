using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserMenegementService.Model;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;

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
        private readonly UserDbContext context;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly IConfiguration configuration;
        private readonly RabbitMqService rabbitMqService;

        public UserService(UserDbContext context, IPasswordHasher<User> passwordHasher, IConfiguration configuration, RabbitMqService rabbitMqService)
        {
            this.context = context;
            this.passwordHasher = passwordHasher;
            this.configuration = configuration;
            this.rabbitMqService = rabbitMqService;
        }
        public async Task<string> AuthenticateAsync(UserLoginModel model)
        {
            var user = await context.Users.SingleOrDefaultAsync(u => u.Username == model.Username);
            if (user == null|| passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password)!= PasswordVerificationResult.Success)
            {
                return null;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
                
        }

        public async Task NotifyUserRegistered(string email)
        {
            await rabbitMqService.PublishMessageAsync(new { Email = email }, "UserRegisteredQueue");
        }

        public async Task<IdentityResult> RegisterUserAsync(UserRegisterModel model)
        {
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow,
                Role = model.Role,
                PasswordHash = passwordHasher.HashPassword(null, model.Password)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
