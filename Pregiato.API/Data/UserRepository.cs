using Microsoft.EntityFrameworkCore;
using Pregiato.API.Enums;
using Pregiato.API.Interfaces;
using Pregiato.API.Models;
using Pregiato.API.Requests;
using Pregiato.API.Services.ServiceModels;

namespace Pregiato.API.Data
{
    public class UserRepository(ModelAgencyContext context) : IUserRepository
    {
        public async Task AddUserAsync(User? user)
        {
            try
            {
                context.Users.Add(user);
                await context.SaveChangesAsync().ConfigureAwait(true);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Erro ao salvar o usuário: {ex.Message}");
                Console.WriteLine($"Exceção interna: {ex.InnerException?.Message}");
            }
        }

        public async Task<IEnumerable<User?>> GetAllUserAsync()
        {
            return await context.Users.ToListAsync().ConfigureAwait(true);
        }

        public async Task<User?> GetByUserIdAsync(Guid id)
        {
            return await context.Users.FindAsync(id).ConfigureAwait(true);
        }

        public async Task<User> GetByUsernameAsync(string queryUser)
        {
            var users = await context.Users
                .AsNoTracking()
                .Where(u => u.NickName == queryUser || u.Email == queryUser || u.Name == queryUser)
                .ToListAsync();

            if (users.Count > 1)
            {
                Console.WriteLine($"Atenção: múltiplos usuários encontrados para '{queryUser}'");
            }

            return users.FirstOrDefault();
        }

        public async Task<UserWhitResultRegister> GetByUser(string nikeName, string email)
        {
            var user = await context.Users
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
            context.Users.Update(user);
            await context.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync().ConfigureAwait(true);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            User? idUser = await context.Users.FindAsync(id).ConfigureAwait(true);
            if (idUser != null)
            {
                context.Users.Remove(idUser);
                await context.SaveChangesAsync().ConfigureAwait(true);
            }
        }

        public async Task GetByUserAsync(LoginUserRequest loginUserRequest)
        {
            LoginUserRequest? loginRequest  =  (from l in context.Users
                                where l.NickName == loginUserRequest.NickNAme
                                select new LoginUserRequest
                                {
                                    NickNAme = loginUserRequest.NickNAme,
                                    Password = l.PasswordHash
                                }).FirstOrDefault();
        }

        public async Task<IEnumerable<User>> GetProducers()
        {
            return await context.Users.AsNoTracking()
                .Where(u => u.UserType == UserType.Producers)
                .ToListAsync();
        }

        public async Task<User> GetByProducersAsync(string name)
        {
            return await context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => EF.Functions.Like(u.Name.Trim(), name.Trim()));
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {

            return await context.Users.ToListAsync().ConfigureAwait(true);
        }

        public async Task<User> GetByUserForCodproducers(string codProducers)
        {
            return await context.Users.AsNoTracking().Where(u => u.CodProducers == codProducers).FirstAsync();
        }
    }
}
