using BagiraWebApi.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace BagiraWebApi.Services.Auth.Services
{
    public class UserService
    {
        private readonly ApplicationContext _context;

        public UserService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            return user;
        }

        public async Task<User> CreateUserByEmailAsync(string login)
        {
            var normalizedEmail = NormalizeEmail(login);
            var user = await _context.Users.AddAsync(
                new User { Email = normalizedEmail });
            await _context.SaveChangesAsync();

            return user.Entity;
        }

        private static string NormalizeEmail(string email)
        {
            return email
                .Trim()
                .Replace(" ", "")
                .ToLower();
        }
    }
}
