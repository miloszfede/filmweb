using FilmwebBackend.Data;
using FilmwebBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace FilmwebBackend.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> RegisterUserAsync(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
        {
            return null;
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return verified ? user : null;
    }
}