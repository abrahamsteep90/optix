// Shapes of the API's JSON responses (see /swagger for the full contract).

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface MovieSummary {
  id: number
  title: string
  /** "yyyy-MM-dd" */
  releaseDate: string
  overview: string
  popularity: number
  voteCount: number
  /** Out of 10; null when nobody has voted. */
  voteAverage: number | null
  originalLanguage: string
  posterUrl: string
  genres: string[]
}

export interface CastMember {
  actorId: number
  name: string
  character: string | null
  profileImageUrl: string | null
}

export interface MovieDetails extends MovieSummary {
  cast: CastMember[]
}

export interface Genre {
  id: number
  name: string
  movieCount: number
}

export interface Actor {
  id: number
  name: string
  movieCount: number
  profileImageUrl: string | null
}

export type SortBy = 'popularity' | 'title' | 'releaseDate' | 'rating'

export type SortDirection = 'asc' | 'desc'

/** The parameters of GET /api/movies. */
export interface MovieQuery {
  search: string
  genres: string[]
  actors: string[]
  sortBy: SortBy
  /** Undefined lets the API pick the natural direction for the field. */
  sortDirection?: SortDirection
  page: number
  pageSize: number
}
