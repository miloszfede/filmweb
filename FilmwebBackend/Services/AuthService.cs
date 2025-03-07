using FilmwebBackend.Data;
using FilmwebBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FilmwebBackend.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ApplicationDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

public async Task<(User? User, string? Token)> RegisterUserAsync(string username, string email, string password)
{
    if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
    {
        return (null, null);
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

    // Generate a token right after registration
    var token = GenerateJwtToken(user);
    return (user, token);
}

    public async Task<(User? User, string? Token)> LoginAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return (null, null);

        bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!verified)
            return (null, null);
            
        // Generate JWT token
        var token = GenerateJwtToken(user);
        return (user, token);
    }
    
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
        
        var claims = new List<Claim> 
        { 
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}