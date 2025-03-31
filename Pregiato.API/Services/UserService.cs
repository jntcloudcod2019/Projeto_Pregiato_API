using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Pregiato.API.Enums;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Response;
using Pregiato.API.Services.ServiceModels;


namespace Pregiato.API.Services
{
    public class UserService(IUserRepository userRepository, IJwtService jwtService,
                       IPasswordHasherService passwordHasherService,
                       IEmailService emailService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEmailService _emailService = emailService;
        private readonly IPasswordHasherService _passwordHasherService = passwordHasherService;
        private readonly CustomResponse _customResponse = new CustomResponse();

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
        public async Task<RegistrationResult> RegisterUserModelAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                                                  .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                                                          .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
             {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
             };

             await _emailService
                 .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                 .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {  
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Model.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                                    .ConfigureAwait(true),
                
            };

            await _userRepository.AddUserAsync(user)
                                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserProducersAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Producers.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserAdministratorAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Administrator.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterUserCoordinationAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Coordination,
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterTelemarketingAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Telemarketing,
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterCEOAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.CEO.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterManagerAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Manager.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

            Console.WriteLine($"Usuário {user.NickName} cadastrado com sucesso.");
            return RegistrationResult.Success;
        }
        public async Task<RegistrationResult> RegisterProductionAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            var userResult = await _userRepository.GetByUser(username, email)
                .ConfigureAwait(true);

            if (userResult.RegistrationResult == RegistrationResult.UserAlreadyExists)
            {
                return RegistrationResult.UserAlreadyExists;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            var nikeName = username.Replace(" ", "").ToLower();

            var password = await _passwordHasherService.GenerateRandomPasswordAsync(12)
                .ConfigureAwait(true);

            var replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService
                .SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato")
                .ConfigureAwait(true);

            var passwordHash = await _passwordHasherService
                .CreatePasswordHashAsync(password)
                .ConfigureAwait(true);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                UserType = UserType.Production.ToString(),
                CodProducers = await GenerateProducerCodeAsync()
                    .ConfigureAwait(true),

            };

            await _userRepository.AddUserAsync(user)
                .ConfigureAwait(true);
            await _userRepository.SaveChangesAsync()
                .ConfigureAwait(true);

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
    }
}
