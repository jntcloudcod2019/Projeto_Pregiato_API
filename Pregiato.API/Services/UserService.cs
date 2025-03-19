using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using PuppeteerSharp;


namespace Pregiato.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; 
        private readonly IEmailService _emailService;   
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly CustomResponse _customResponse;
    
        public UserService(IUserRepository userRepository, IJwtService jwtService, 
                           IPasswordHasherService passwordHasherService,
                           IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;  
            _passwordHasherService = passwordHasherService; 
            _emailService = emailService;
            _customResponse = new CustomResponse();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }
        public async Task<string> RegisterUserAsync(string username, string email)
        {

            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                _customResponse.Message = $"Usuário: {username} já cadasttrado.";
            }

            string nikeName = username.Replace(" ", "").ToLower();

            string password = await _passwordHasherService.GenerateRandomPasswordAsync(12);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService.SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato");

            string passwordHash = await _passwordHasherService.CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = new Guid(),
                Name = nikeName,
                Email = email,
                PasswordHash = passwordHash,
                UserType = UserType.Administrator,
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return ("Usuário cadastrado com suesso.");
        }

        public async Task<string> AuthenticateUserAsync(LoginUserRequest loginUserRequest)
        {
            try
            {
                if (loginUserRequest == null || string.IsNullOrWhiteSpace(loginUserRequest.Username))
                {
                    throw new ArgumentException("Nome de usuário não pode ser nulo ou vazio.");
                }

                var user = await _userRepository.GetByUsernameAsync(loginUserRequest.Username);

                if (user == null)
                {
                    _customResponse.Message = "Usuário não encontrado. Verifique o nome de usuário e tente novamente.";
                    throw new Exception(_customResponse.Message);
                }

                if (string.IsNullOrWhiteSpace(loginUserRequest.Password) || !BCrypt.Net.BCrypt.Verify(loginUserRequest.Password, user.PasswordHash))
                {
                    _customResponse.Message = "Senha inválida. Verifique a senha digitada e tente novamente.";
                    throw new Exception(_customResponse.Message);
                }

                loginUserRequest.UserType = user.UserType;
                loginUserRequest.IdUser = user.UserId;
                loginUserRequest.Email = user.Email;

                return _jwtService.GenerateToken(loginUserRequest);
            }
            catch (Exception ex)
            {
                _customResponse.Message = "Erro durante o processo de autenticação. Por favor, contate o time de I.T.";
                throw new Exception($"Erro durante o processo de autenticação. {ex.Message}");
            }
        }

        public async Task<string> RegisterUserModel(string username, string email)
        {

            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                _customResponse.Message = $"Usuário: {username} já cadasttrado.";
            }

            string nikeName = username.Replace(" ", "").ToLower();

            string password = await _passwordHasherService.GenerateRandomPasswordAsync(12);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };     

            await _emailService.SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato");

            string passwordHash = await _passwordHasherService.CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = new Guid(),
                Name =nikeName,
                Email = email,
                PasswordHash = passwordHash,
                UserType = UserType.Model.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return ("Usuário cadastrado com suesso.");
        }  
    }
}
