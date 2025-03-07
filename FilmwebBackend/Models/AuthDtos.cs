namespace FilmwebBackend.Models;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Username, string Password);

public record AuthResponse(int UserId, string Username, string Email, string Token);