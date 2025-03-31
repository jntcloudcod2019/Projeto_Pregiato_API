using Microsoft.EntityFrameworkCore;
using Pregiato.API.Enums;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly ModelAgencyContext _context;

        public UserRepository(ModelAgencyContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User? user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync().ConfigureAwait(true);
            }
             catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao salvar o usuário: {ex.Message}");
                Console.WriteLine($"Exceção interna: {ex.InnerException?.Message}");
             }
        }

        public async Task<IEnumerable<User?>> GetAllUserAsync()
        {
            return await _context.Users.ToListAsync().ConfigureAwait(true);
        }

        public async Task<User?> GetByUserIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id).ConfigureAwait(true);
        }

        public async Task<User> GetByUsernameAsync(string nikeName)
        {
            return await _context.Users
                .AsNoTracking() 
                .SingleOrDefaultAsync(u => u.NickName == nikeName);
        }

        public async Task<UserWhitResultRegister> GetByUser(string nikeName, string email)
        {
            var user = await _context.Users
                .AsTracking()
                .SingleOrDefaultAsync
                    (u => u.Name== nikeName && u.Email == email)
                .ConfigureAwait(true);

            var result = new UserWhitResultRegister();

            if (user == null)
            {
              
                result.User = null;
                result.RegistrationResult = RegistrationResult.NonExistentUser;
            }
            else
            {
                result.User = user;
                result.RegistrationResult = RegistrationResult.UserAlreadyExists;
            }

            return result;
        }

        public async Task UpdateUserAsync(User? user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            User? idUser = await _context.Users.FindAsync(id).ConfigureAwait(true);
            if (idUser != null)
            {
                _context.Users.Remove(idUser);
                await _context.SaveChangesAsync().ConfigureAwait(true);
            }
        }

        public async Task GetByUserAsync(LoginUserRequest loginUserRequest)
        {
            LoginUserRequest? loginRequest  =  (from l in _context.Users
                                where l.NickName == loginUserRequest.NickNAme
                                select new LoginUserRequest
                                {
                                    NickNAme = loginUserRequest.NickNAme,
                                    Password = l.PasswordHash
                                }).FirstOrDefault();
        }

        public async Task<User> GetByProducersAsync(string name)
        {
            return await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => EF.Functions.Like(u.Name.Trim(), name.Trim()));
        }
    }
}
