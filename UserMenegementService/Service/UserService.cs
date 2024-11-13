using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserMenegementService.Model;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Diagnostics;
using Org.BouncyCastle.Utilities;
using Microsoft.AspNetCore.Authentication;

namespace UserMenegementService.Service
{
    public interface IUserService
    {
        Task<User> AuthenticateUserAsync(string username, string password);
        Task<User> RegisterUserAsync(UserRegister registerModel);
        Task Handle(dynamic message);
    }
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<UserService> _logger;

        public UserService(UserDbContext context, RabbitMqService rabbitMqService, ILogger<UserService> logger)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
            _logger = logger;   
        }

        public async Task<User> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Попытка аутентификации пользователя с email: {Email}", email);

                var user = await _context.Users
                                          .Include(u => u.FreelancerProfile)
                                          .Include(u => u.ClientProfile)
                                          .SingleOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("Пользователь с email {Email} не найден", email);
                    return null;
                }

                if (!VerifyPassword(user, password))
                {
                    _logger.LogWarning("Неверный пароль для пользователя с email {Email}", email);
                    return null;
                }

                _logger.LogInformation("Пользователь с email {Email} успешно аутентифицирован", email);
                return user;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка при взаимодействии с базой данных");
                throw new ApplicationException("Произошла ошибка при аутентификации пользователя.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при аутентификации пользователя с email: {Email}", email);
                throw new ApplicationException("Произошла неизвестная ошибка при аутентификации пользователя.", ex);
            }
        }

        public async Task Handle(dynamic message)
        {
            var userMessage = JsonConvert.DeserializeObject<dynamic>(message);
            var action = userMessage.Action.ToString();

            switch (action)
            {
                case "GetFreelancerMail":
                    {
                        var mail = await GetFreelancerMail(Convert.ToInt32(userMessage.FreelancerId));
                        var response = new
                        {
                            Action = "GetFreelancerMail",
                            Mail = mail,
                            CorrelationId = userMessage.CorrelationId
                        };
                        await _rabbitMqService.PublishAsync("UserNotificationQueue", JsonConvert.SerializeObject(response));
                    }
                    break;
                case "GetClientMail":
                    {
                        var mail = await GetClientMail(Convert.ToInt32(userMessage.ClientId));
                        var response = new
                        {
                            Action = "GetClientMail",
                            Mail = mail,
                            CorrelationId = userMessage.CorrelationId
                        };
                        await _rabbitMqService.PublishAsync("UserNotificationQueue", JsonConvert.SerializeObject(response));
                    }
                    break;

            }
        }

        public async Task<string> GetClientMail(int clientId)
        {
            try
            {
                _logger.LogInformation("Попытка получения email для клиента с ID: {ClientId}", clientId);

                var userEmail = await _context.Users
                    .Where(u => u.ClientProfile.Id == clientId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (userEmail == null)
                {
                    _logger.LogWarning("Email для клиента с ID {ClientId} не найден", clientId);
                    return null; 
                }

                _logger.LogInformation("Email для клиента с ID {ClientId}: {UserEmail}", clientId, userEmail);
                return userEmail;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка при работе с базой данных при получении email для клиента с ID: {ClientId}", clientId);
                throw new ApplicationException("Произошла ошибка при получении email клиента.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при получении email для клиента с ID: {ClientId}", clientId);
                throw new ApplicationException("Произошла неизвестная ошибка при получении email клиента.", ex);
            }
        }

        public async Task<string> GetFreelancerMail(int freelancerId)
        {
            try
            {
                _logger.LogInformation("Попытка получения email для фрилансера с ID: {FreelancerId}", freelancerId);

                var userEmail = await _context.Users
                    .Where(u => u.FreelancerProfile.Id == freelancerId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();

                if (userEmail == null)
                {
                    _logger.LogWarning("Email для фрилансера с ID {FreelancerId} не найден", freelancerId);
                    return null; 
                }

                _logger.LogInformation("Email для фрилансера с ID {FreelancerId}: {UserEmail}", freelancerId, userEmail);
                return userEmail;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка при работе с базой данных при получении email для фрилансера с ID: {FreelancerId}", freelancerId);
                throw new ApplicationException("Произошла ошибка при получении email фрилансера.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при получении email для фрилансера с ID: {FreelancerId}", freelancerId);
                throw new ApplicationException("Произошла неизвестная ошибка при получении email фрилансера.", ex);
            }
        }

        public async Task<User> RegisterUserAsync(UserRegister registerModel)
        {
            try
            {
                _logger.LogInformation("Попытка регистрации пользователя с email: {Email}", registerModel.Email);

                var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == registerModel.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Попытка регистрации с уже существующим email: {Email}", registerModel.Email);
                    throw new InvalidOperationException("User with this email already exists.");
                }

                var newUser = new User
                {
                    Username = registerModel.Username,
                    Email = registerModel.Email,
                    Role = registerModel.Role,
                    PasswordHash = registerModel.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(); 

                if (registerModel.Role == "Freelancer")
                {
                    var freelancerProfile = new FreelancerProfile
                    {
                        UserId = newUser.Id,
                        Skills = registerModel.Skills,
                        Bio = registerModel.Bio
                    };
                    _context.FreelancerProfiles.Add(freelancerProfile);
                    newUser.FreelancerProfile = freelancerProfile;
                    _logger.LogInformation("Профиль фрилансера для пользователя с email {Email} был успешно создан.", registerModel.Email);
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
                    newUser.ClientProfile = clientProfile;
                    _logger.LogInformation("Профиль клиента для пользователя с email {Email} был успешно создан.", registerModel.Email);
                }

                await _context.SaveChangesAsync(); 

                var registeredUser = await _context.Users
                    .Include(u => u.FreelancerProfile)
                    .Include(u => u.ClientProfile)
                    .SingleOrDefaultAsync(u => u.Id == newUser.Id);

                _logger.LogInformation("Пользователь с email {Email} успешно зарегистрирован.", registerModel.Email);

                return registeredUser;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LogError(invOpEx, "Ошибка регистрации: пользователь с таким email уже существует.");
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка при сохранении данных в базу данных.");
                throw new ApplicationException("Произошла ошибка при сохранении данных пользователя.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неизвестная ошибка при регистрации пользователя с email: {Email}", registerModel.Email);
                throw new ApplicationException("Произошла неизвестная ошибка при регистрации пользователя.", ex);
            }
        }


        private bool VerifyPassword(User user, string password)
        {
            var userPassword = user.PasswordHash;
            return userPassword == password;
        }
    }

}
