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

// Updated CORS configuration - adding named policy and allowing specific origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)  // Allow any origin if your policy gets complex
              .WithExposedHeaders("Content-Disposition", "Content-Length")
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); // Cache preflight for 24 hours
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Make sure CORS middleware is used early in the pipeline - BEFORE Swagger
app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Filmweb API V1");
    });
}

app.UseHttpsRedirection();

// Register endpoint with explicit CORS policy
app.MapPost("/api/auth/register", async (RegisterRequest request, AuthService authService) =>
{
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest("Username, email and password are required");
        
    var user = await authService.RegisterUserAsync(request.Username, request.Email, request.Password);
    
    if (user == null)
        return Results.BadRequest("Username or email already exists");
        
    return Results.Ok(new AuthResponse(user.Id, user.Username, user.Email));
})
.WithName("RegisterUser")
.RequireCors("AllowReactApp");  // Apply CORS policy to this endpoint

// Login endpoint with explicit CORS policy
app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
{
    var user = await authService.LoginAsync(request.Username, request.Password);
    
    if (user == null)
        return Results.BadRequest("Invalid username or password");
        
    return Results.Ok(new AuthResponse(user.Id, user.Username, user.Email));
})
.WithName("LoginUser")
.RequireCors("AllowReactApp");  // Apply CORS policy to this endpoint

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
.WithName("GetWeatherForecast")
.RequireCors("AllowReactApp");

app.MapGet("/api/movies/search", async (string query, int? page, TMDBApiClient client) =>
{
    var result = await client.SearchMoviesAsync(query, page ?? 1);
    return result;
})
.WithName("SearchMovies")
.RequireCors("AllowReactApp");

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
.WithName("GetMovieDetails")
.RequireCors("AllowReactApp");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
