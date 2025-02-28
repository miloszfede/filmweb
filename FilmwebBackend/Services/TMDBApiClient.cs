using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using FilmwebBackend.Models;

namespace FilmwebBackend.Services;

public class TMDBApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<TMDBApiClient> _logger;

    public TMDBApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TMDBApiClient> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["TMDBApi:ApiKey"] 
            ?? throw new ArgumentNullException("TMDB API key not found in configuration");
        _httpClient.BaseAddress = new Uri(configuration["TMDBApi:BaseUrl"] ?? "https://api.themoviedb.org/3/");
        _logger = logger;
    }

    public async Task<MovieSearchResponse?> SearchMoviesAsync(string query, int page = 1)
    {
        try
        {
            var requestUrl = $"search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&page={page}";
            _logger.LogInformation($"Making request to: {_httpClient.BaseAddress}{requestUrl}");

            var response = await _httpClient.GetAsync(requestUrl);
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response status: {response.StatusCode}, Content: {content.Substring(0, Math.Min(content.Length, 500))}");
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MovieSearchResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SearchMoviesAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<MovieDetails?> GetMovieDetailsAsync(int movieId)
    {
        try
        {
            var requestUrl = $"movie/{movieId}?api_key={_apiKey}";
            _logger.LogInformation($"Making request to: {_httpClient.BaseAddress}{requestUrl}");
            
            var response = await _httpClient.GetAsync(requestUrl);
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response status: {response.StatusCode}, Content: {content.Substring(0, Math.Min(content.Length, 500))}");
            
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MovieDetails>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetMovieDetailsAsync: {ex.Message}");
            throw;
        }
    }
}