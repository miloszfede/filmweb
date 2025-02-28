using FilmwebBackend.Services;
using FilmwebBackend.Models;
using FilmwebBackend.Data;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Filmweb Backend API", 
        Version = "v1",
        Description = "API for accessing movie data from TMDB"
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=filmweb.db"));

builder.Services.AddHttpClient<TMDBApiClient>(client =>
{
    var baseUrl = builder.Configuration["TMDBApi:BaseUrl"] ?? "https://api.themoviedb.org/3/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<AuthService>();

builder.Services.AddLogging();
builder.Services.AddCors();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Filmweb API V1");
    });
}

app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.MapPost("/api/auth/register", async (RegisterRequest request, AuthService authService) =>
{
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest("Username, email and password are required");
        
    var user = await authService.RegisterUserAsync(request.Username, request.Email, request.Password);
    
    if (user == null)
        return Results.BadRequest("Username or email already exists");
        
    return Results.Ok(new AuthResponse(user.Id, user.Username, user.Email));
})
.WithName("RegisterUser");

app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
{
    var user = await authService.LoginAsync(request.Username, request.Password);
    
    if (user == null)
        return Results.BadRequest("Invalid username or password");
        
    return Results.Ok(new AuthResponse(user.Id, user.Username, user.Email));
})
.WithName("LoginUser");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/movies/search", async (string query, int? page, TMDBApiClient client) =>
{
    var result = await client.SearchMoviesAsync(query, page ?? 1);
    return result;
})
.WithName("SearchMovies");

app.MapGet("/api/movies/{id}", async (int id, TMDBApiClient client) =>
{
    try
    {
        var movie = await client.GetMovieDetailsAsync(id);
        return Results.Ok(movie);
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }
})
.WithName("GetMovieDetails");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
