using System.Text.Json.Serialization;

namespace FilmwebBackend.Models
{
    public class MovieSearchResponse
    {
        public int Page { get; set; }
        public List<MovieSearchResult> Results { get; set; } = new();
        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }

    public class MovieSearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;
        [JsonPropertyName("vote_average")]
        public float VoteAverage { get; set; }
        public string Overview { get; set; } = string.Empty;
    }

    public class MovieDetails
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }
        [JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }
        public string Overview { get; set; } = string.Empty;
        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;
        [JsonPropertyName("vote_average")]
        public float VoteAverage { get; set; }
        public int Runtime { get; set; }
        public List<Genre> Genres { get; set; } = new();
        public string Status { get; set; } = string.Empty;
    }

    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}