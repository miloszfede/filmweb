export interface MovieResult {
    id: number;
    title: string;
    poster_path: string;
    release_date: string;
    vote_average: number;
    overview: string;
}

export interface MovieSearchResponse {
    page: number;
    results: MovieResult[];
    total_results: number;
    total_pages: number;
}

export interface MovieDetail extends MovieResult {
    backdrop_path: string | null;
    runtime?: number;
    genres?: { id: number; name: string }[];
    status?: string;
}