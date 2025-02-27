using FilmwebBackend.Services;
using FilmwebBackend.Models;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Filmweb Backend API", 
        Version = "v1",
        Description = "API for accessing movie data from TMDB"
    });
});

builder.Services.AddHttpClient<TMDBApiClient>(client =>
{
    
    var baseUrl = builder.Configuration["TMDBApi:BaseUrl"] ?? "https://api.themoviedb.org/3/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddLogging();
builder.Services.AddCors();

var app = builder.Build();

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

// Add TMDB API endpoints
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
