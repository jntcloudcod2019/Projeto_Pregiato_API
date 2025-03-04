using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Pregiato.API.Interface;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;


namespace Pregiato.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService; 
        private readonly IEmailService _emailService;   
        private readonly IPasswordHasherService _passwordHasherService;

        public UserService(IUserRepository userRepository, IJwtService jwtService, 
                           IPasswordHasherService passwordHasherService,
                           IEmailService emailService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;  
            _passwordHasherService = passwordHasherService; 
            _emailService = emailService;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }
        public async Task<string> RegisterUserAsync(string username, string email, string password, string userType)
        {

            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                throw new Exception("Username already exists.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User

            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                PasswordHash = passwordHash,
                UserType = userType
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return "Usuário cadastrado comsucesso.";
        }

        public async Task<string> AuthenticateUserAsync(LoginUserRequest loginUserRequest)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(loginUserRequest.Username);

                if (user == null)
                {
                    throw new Exception("Usuário não encontrado. Verifique o nome de usuário e tente novamente.");
                }

                if (!BCrypt.Net.BCrypt.Verify(loginUserRequest.Password, user.PasswordHash))
                {
                    throw new Exception("Senha inválida. Verifique a senha digitada e tente novamente.");
                  
                }

                loginUserRequest.UserType = user.UserType;
                loginUserRequest.IdUser = user.UserId;
                loginUserRequest.Email = user.Email;
                return _jwtService.GenerateToken(loginUserRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Erro na Autenticação] {ex.Message}");
                throw new Exception("Erro durante o processo de autenticação. Por favor, contate o time de I.T.");
            }
        }

        public async Task<string> RegisterUserModel(string username, string email)
        {

            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                throw new Exception("Usuário já cadastrado.");
            }

            var password = _passwordHasherService.GenerateRandomPassword(8) ;

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "WelcomeEmailTemplate.html");

            var replacements = new Dictionary<string, string>
            {
                {"Nome",  username.Split(" ") [0]},
                {"User", username },
                {"Password", password }
            };

            var emailBody = await _emailService.LoadTemplate(templatePath, replacements);

         //   await _emailService.SendEmailAsync(email, "Bem-vindo à Plataforma My Pregiato", emailBody);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                UserId = new Guid(),
                Name = username.Split(" ")[0],
                Email = email,
                PasswordHash = passwordHash,
                UserType = UserType.Model.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return "Usuário cadastrado comsucesso.";
        }  
    }
}
