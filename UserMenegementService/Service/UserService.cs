﻿using Microsoft.AspNetCore.Identity;
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
        Task<User> RegisterUserAsync(UserRegisterModel registerModel);
    }
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;

        public UserService(UserDbContext context)
        {
            _context = context;
        }

        public async Task<User> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(user, password))
            {
                return null;
            }
            return user;
        }

        public async Task<User> RegisterUserAsync(UserRegisterModel registerModel)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Username == registerModel.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this username already exists.");
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
            }

            await _context.SaveChangesAsync();

            return newUser;
        }

        private bool VerifyPassword(User user, string password)
        {
            var userPassword = user.PasswordHash;
            return userPassword == password;
        }
    }

}
