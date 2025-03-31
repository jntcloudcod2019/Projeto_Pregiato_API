using System.Text.Json;
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
        public async Task<string> RegisterUserAsync(string username, string email)
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

            User user = new User
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
                if (loginUserRequest == null || string.IsNullOrWhiteSpace(loginUserRequest.NickName))
                {
                    throw new ArgumentException("Nome de usuário não pode ser nulo ou vazio.");
                }

                User? user = await _userRepository.GetByUsernameAsync(loginUserRequest.NickName);

                if (user == null)
                {
                    throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                    {
                        Message = "Usuário ou senha inválidos.",
                        Details = "O NickName fornecido não foi encontrado na base de dados."
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

        public async Task<string> RegisterUserModelAsync(string username, string email)
        {

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username} já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Model.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }

        public async Task<string> RegisterUserProducersAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Producers.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }

        public async Task<string> RegisterUserAdministratorAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Administrator.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }

        public async Task<string> RegisterUserCoordinationAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Coordination.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }

        public async Task<string> RegisterManagerAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Manager.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }

        public async Task<string> RegisterTelemarketingAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} |  Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Telemarketing.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }
        public async Task<string> RegisterCEOAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username).ConfigureAwait(false);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

            string nikeName = username.Replace(" ", "").ToLower();

            string password = await _passwordHasherService.GenerateRandomPasswordAsync(12).ConfigureAwait(false);

            Dictionary<string, string> replacements = new Dictionary<string, string>
            {
                {"Nome",username},
                {"User",nikeName },
                {"Password", password}
            };

            await _emailService.SendEmailAsync(replacements, email, "Bem-vindo à Plataforma My Pregiato").ConfigureAwait(false);
            string passwordHash = await _passwordHasherService.CreatePasswordHashAsync(password).ConfigureAwait(false);

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync().ConfigureAwait(false),
                UserType = UserType.CEO.ToString(),
            };

            await _userRepository.AddUserAsync(user).ConfigureAwait(false);
            await _userRepository.SaveChangesAsync().ConfigureAwait(false);

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
        }
        public async Task<string> RegisterProductionAsync(string username, string email)
        {
            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Validando se o USER_MODEL: {username}, já está cadastrado... ");

            User? shfUser = await _userRepository.GetByUsernameAsync(username);

            if (shfUser != null)
            {
                throw new UnauthorizedAccessException(JsonSerializer.Serialize(new ErrorResponse
                {
                    Message = $"Este usuário {username}já posseui cadastrao.",
                })); ;
            }

            Console.WriteLine($"[PROCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | Gerando cadastro... ");

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

            User user = new User
            {
                UserId = new Guid(),
                Name = username,
                Email = email,
                NickName = nikeName,
                PasswordHash = passwordHash,
                CodProducers = await GenerateProducerCodeAsync(),
                UserType = UserType.Production.ToString(),
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            Console.WriteLine($"");
            return ($"Usuário {username} cadastrado com sucesso.");
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
