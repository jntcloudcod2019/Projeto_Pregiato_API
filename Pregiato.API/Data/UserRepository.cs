using Microsoft.EntityFrameworkCore;
using Pregiato.API.Interface;
using Pregiato.API.Models;

namespace Pregiato.API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly ModelAgencyContext _context;

        public UserRepository(ModelAgencyContext context) 
        { 
            _context = context; 
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);   
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllUserAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetByUserIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Name == username);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var idUser = await _context.Users.FindAsync(id);
            if (idUser != null)
            {
                _context.Users.Remove(idUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
