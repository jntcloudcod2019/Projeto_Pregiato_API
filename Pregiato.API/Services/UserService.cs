using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Pregiato.API.Data;
using Pregiato.API.DTO;
using Pregiato.API.Enums;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Pregiato.API.Services.ServiceModels;


namespace Pregiato.API.Services
{
    public class UserService(IUserRepository userRepository, IJwtService jwtService,IProcessWhatsApp processWhatsApp,
                       IPasswordHasherService passwordHasherService, IDbContextFactory<ModelAgencyContext> contextFactory,
                       IEmailService emailService, IHttpContextAccessor httpContextAccessor) : IUserService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IProcessWhatsApp _processWhatsApp = processWhatsApp;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IEmailService _emailService = emailService;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;
        private readonly IDbContextFactory<ModelAgencyContext> _contextFactory = contextFactory;
        private readonly CustomResponse _customResponse = new();
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteUserAsync(id);
            await _userRepository.SaveChangesAsync();
        }
        public async Task<RegistrationResult> RegisterUserAsync(string username, string email)
        {

            if (await _userRepository.GetByUsernameAsync(username) != null)
            {
                _customResponse.Message = $"Usuário: {username} já cadasttrado.";
            }

            string nikeName = username.Replace(" ", "").ToLower();

            string password = await _passwordHasherService.GenerateRandomPasswordAsync(12);

            Dictionary<string, string> replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService.SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato");

            string passwordHash = await _passwordHasherService.CreatePasswordHashAsync(password);

            User? user = new User
            {
                UserId = new Guid(),
                Name = nikeName,
                Email = email,
                PasswordHash = passwordHash,
                UserType = UserType.Administrator,
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return RegistrationResult.Success;
        }
        public async Task<string> AuthenticateUserAsync(LoginUserRequest? loginUserRequest)
        {
            try
            {
                User? user = await _userRepository.GetByUsernameAsync(loginUserRequest.NickNAme);

                if (user == null)
                {
                    throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                    {
                        Message = "USUÁRIOS OU SENHA INVÁLIDOS.".ToUpper()
                    }));
                }

                if (string.IsNullOrWhiteSpace(loginUserRequest.Password) || !BCrypt.Net.BCrypt.Verify(loginUserRequest.Password, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                    {
                        Message = "Usuário ou senha inválidos.",
                        Details = "A senha fornecida não corresponde ao usuário."
                    }));
                }

                loginUserRequest.UserType = user.UserType;
                loginUserRequest.IdUser = user.UserId;
                loginUserRequest.Email = user.Email;

                return await _jwtService.GenerateToken(loginUserRequest);
            }
            catch (Exception ex)
            {
                _customResponse.Message = "Erro durante o processo de autenticação. Por favor, contate o time de I.T.";
                throw new Exception($"Erro durante o processo de autenticação. {ex.Message}");
            }
        }
        public async Task<RegistrationResult> RegisterUserModelAsync(string username, string email, string CodProducers, Model model)
        {

            var userResult = await _userRepository.GetByUser(username, email)
                                                  .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
                Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();
            var password = "123456";

            var replacements = new Dictionary<string, string>
            {   {"Position", UserType.Model},
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };


            await _processWhatsApp.ProcessWhatsAppModelAsync(model, nikeName, password);

            await _emailService
                 .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                 .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = model.IdModel,
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                Cpf = model.CPF,
                WhatsApp = model.TelefonePrincipal,
                UserType = UserType.Model.ToString(),
                CodProducers = CodProducers
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");

            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserProducersAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Position", UserType.PRODUCERS},
                {"Nome",userRegisterDto.Username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.PRODUCERS.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserAdministratorAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Position", UserType.Administrator},
                {"Nome",userRegisterDto.Username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato");

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Administrator.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserCoordinationAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12);

            var replacements = new Dictionary<string, string>
            {
                {"Position", UserType.Coordination},
                {"Nome",userRegisterDto.Username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato");

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Coordination,
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterTelemarketingAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12);

            var replacements = new Dictionary<string, string>
            {
                {"Position", UserType.Telemarketing},
                {"Nome",userRegisterDto.Username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato");

            var passwordHash = await _passwordHasherService.CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Telemarketing,
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);
            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterCEOAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
    {
        {"Position", UserType.PRODUCERS},
        {"Nome",userRegisterDto.Username},
        {"User",nikeName },
        {"Password", password}
    };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato");

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Ceo.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterManagerAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
    {
        {"Position", UserType.PRODUCERS},
        {"Nome",userRegisterDto.Username},
        {"User",nikeName },
        {"Password", password}
    };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Manager.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterProductionAsync(UserRegisterDto userRegisterDto)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {userRegisterDto.Username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(userRegisterDto.Username, userRegisterDto.Email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = userRegisterDto.Username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
    {
        {"Position", UserType.PRODUCERS},
        {"Nome",userRegisterDto.Username},
        {"User",nikeName },
        {"Password", password}
    };

            await _emailService
                .SendEmailAsync(replacements, userRegisterDto.Email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userRegisterDto.Username,
                Email = userRegisterDto.Email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                WhatsApp = userRegisterDto.WhatsApp,
                Cpf = userRegisterDto.Cpf,
                UserType = UserType.Production.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
            };

            await _processWhatsApp.ProcessWhatsAppCollaboratorAsync(user, nikeName, password);

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public Task<string> GenerateProducerCodeAsync()
        {
            const string prefix = "PM";
            const int randomNumberLength = 6;
            Random random = new Random();
            int randomNumber = random.Next(0, 999999);
            string code = $"{prefix}{randomNumber:000000}";
            return Task.FromResult(code);
        }
        public async Task<User> UserCaptureByToken()
        {

            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("TOKEN INVÁLIDO");

            var token = authorizationHeader["Bearer ".Length..].Trim();
            var handler = new JwtSecurityTokenHandler();

            JwtSecurityToken jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                throw new UnauthorizedAccessException("TOKEN MAL FORMADO");
            }

            string GetClaimValue(string claimType) =>
                jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == claimType || c.Type.EndsWith($"/{claimType}", StringComparison.OrdinalIgnoreCase))?.Value;

            var email = GetClaimValue("email") ?? GetClaimValue(ClaimTypes.Email);
            var userTypeFromToken = GetClaimValue("role") ?? GetClaimValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userTypeFromToken))
                throw new UnauthorizedAccessException("INFORMAÇÕES DO TOKEN INCOMPLETAS");

            using var context = _contextFactory.CreateDbContext();

            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                throw new UnauthorizedAccessException("USUÁRIO NÃO ENCONTRADO");
            return user;
        }
        public async Task<bool> UpdatePasswordAsync(Guid userId, string newPassword)
        {
            await using var context = _contextFactory.CreateDbContext();

            var affectedRows = await context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(newPassword))
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow)
                );

            return affectedRows > 0;
        }
    }
}
