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
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(UserRegister registerModel);
    }
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;

        public UserService(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            var user = await _context.Users
                            .Include(u => u.FreelancerProfile)
                            .Include(u => u.ClientProfile) // Добавляем ClientProfile
                            .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(user, password))
            {
                return null;
            }
            return user;
        }

        public async Task<User> RegisterUserAsync(UserRegister registerModel)
        {
            // Проверка на существующего пользователя
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == registerModel.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Создаем нового пользователя
            var newUser = new User
            {
                Username = registerModel.Username,
                Email = registerModel.Email,
                Role = registerModel.Role,
                PasswordHash = registerModel.PasswordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Сохраняем пользователя, чтобы получить его ID
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // Теперь newUser.Id установлен

            // Создаем профиль в зависимости от роли
            if (registerModel.Role == "Freelancer")
            {
                var freelancerProfile = new FreelancerProfile
                {
                    UserId = newUser.Id,
                    Skills = registerModel.Skills,
                    Bio = registerModel.Bio
                };
                _context.FreelancerProfiles.Add(freelancerProfile);
                newUser.FreelancerProfile = freelancerProfile; // Присваиваем профиль пользователю
            }
            else if (registerModel.Role == "Client")
            {
                var clientProfile = new ClientProfile
                {
                    UserId = newUser.Id,
                    CompanyName = registerModel.CompanyName,
                    Description = registerModel.Description
                };
                _context.ClientProfiles.Add(clientProfile);
                newUser.ClientProfile = clientProfile; // Присваиваем профиль пользователю
            }

            // Сохраняем профиль
            await _context.SaveChangesAsync();

            // Перезагружаем пользователя с профилями для корректной загрузки связанных данных
            var registeredUser = await _context.Users
                .Include(u => u.FreelancerProfile)
                .Include(u => u.ClientProfile)
                .SingleOrDefaultAsync(u => u.Id == newUser.Id);

            return registeredUser;
        }


        private bool VerifyPassword(User user, string password)
        {
            var userPassword = user.PasswordHash;
            return userPassword == password;
        }
    }

}
