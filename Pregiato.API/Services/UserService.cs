using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;


namespace Pregiato.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; 

        public UserService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;  
        }

        public async Task<string> RegisterUserAsync(string username, string email, string password, UserType userType)
        {
            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                throw new Exception("Username already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Name = username,
                Email = email,
                PasswordHash = passwordHash
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return "User registered successfully.";
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<string> AuthenticateUserAsync(LoginUserRequest loginUserRequest)
        {
            var user = await _userRepository.GetByUsernameAsync(loginUserRequest.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUserRequest.Password, user.PasswordHash))
            {
                throw new Exception("Invalid username or password.");
            }

            return _jwtService.GenerateToken(loginUserRequest);
        }
    }
  }
