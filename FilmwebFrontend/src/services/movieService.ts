import axios from 'axios';
import { MovieSearchResponse, MovieDetail } from '../types/movie';

const API_URL = "http://localhost:5112/api/movies";

/**
 * Search for movies by query string
 */
const searchMovies = async (query: string, page: number = 1): Promise<MovieSearchResponse> => {
  const response = await axios.get(`${API_URL}/search`, {
    params: {
      query,
      page
    }
  });
  return response.data;
};

/**
 * Get details for a specific movie
 */
const getMovieDetails = async (movieId: number): Promise<MovieDetail> => {
  const response = await axios.get(`${API_URL}/${movieId}`);
  return response.data;
};

const MovieService = {
  searchMovies,
  getMovieDetails
};

export default MovieService;
