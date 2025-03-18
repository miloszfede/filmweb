using FilmwebBackend.Services;
using FilmwebBackend.Models;
using FilmwebBackend.Data;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

builder.Services.AddAuthorization();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)  
              .WithExposedHeaders("Content-Disposition", "Content-Length")
              .SetPreflightMaxAge(TimeSpan.FromSeconds(86400)); 
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("AllowReactApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Filmweb API V1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapPost("/api/auth/register", async (RegisterRequest request, AuthService authService) =>
{
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        return Results.BadRequest("Username, email and password are required");
        
    var result = await authService.RegisterUserAsync(request.Username, request.Email, request.Password);
    
    if (result.User == null)
        return Results.BadRequest("Username or email already exists");
        
    if (string.IsNullOrEmpty(result.Token))
        return Results.BadRequest("Token generation failed");

    return Results.Ok(new AuthResponse(
        result.User.Id,
        result.User.Username,
        result.User.Email,
        result.Token
    ));
})
.WithName("RegisterUser")
.RequireCors("AllowReactApp");  

app.MapPost("/api/auth/login", async (LoginRequest request, AuthService authService) =>
{
    var result = await authService.LoginAsync(request.Username, request.Password);
    
    if (result.User == null)
        return Results.BadRequest("Invalid username or password");

    if (string.IsNullOrEmpty(result.Token))
        return Results.BadRequest("Token generation failed");
        
    return Results.Ok(new AuthResponse(
        result.User.Id,
        result.User.Username,
        result.User.Email,
        result.Token
    ));
})
.WithName("LoginUser")
.RequireCors("AllowReactApp");

app.MapGet("/api/user/profile", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var username = user.FindFirstValue(ClaimTypes.Name);
    var email = user.FindFirstValue(ClaimTypes.Email);
    
    return Results.Ok(new { userId, username, email });
})
.WithName("GetUserProfile")
.RequireAuthorization() 
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


