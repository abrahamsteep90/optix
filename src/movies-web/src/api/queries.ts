import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { getJson, moviesUrl } from './client'
import type { Actor, Genre, MovieDetails, MovieQuery, MovieSummary, PagedResult } from './types'

/** One page of movies. While the next page loads, the previous one stays on screen. */
export function useMovies(query: MovieQuery) {
  return useQuery({
    queryKey: ['movies', query],
    queryFn: ({ signal }) => getJson<PagedResult<MovieSummary>>(moviesUrl(query), signal),
    placeholderData: keepPreviousData,
  })
}

export function useMovie(id: number) {
  return useQuery({
    queryKey: ['movie', id],
    queryFn: ({ signal }) => getJson<MovieDetails>(`/api/movies/${id}`, signal),
    enabled: Number.isInteger(id) && id > 0,
  })
}

export function useGenres() {
  return useQuery({
    queryKey: ['genres'],
    queryFn: ({ signal }) => getJson<Genre[]>('/api/genres', signal),
    staleTime: Infinity,
  })
}

/** Actor names for the actor filter's suggestions. */
export function useActorSuggestions(search: string) {
  return useQuery({
    queryKey: ['actors', search],
    queryFn: ({ signal }) =>
      getJson<PagedResult<Actor>>(`/api/actors?search=${encodeURIComponent(search)}&pageSize=8`, signal),
    enabled: search.length >= 2,
    placeholderData: keepPreviousData,
  })
}

/** Whether any cast data has been loaded from TMDB yet (it is optional; see the README). */
export function useHasCastData() {
  return useQuery({
    queryKey: ['actors', 'any'],
    queryFn: ({ signal }) => getJson<PagedResult<Actor>>('/api/actors?pageSize=1', signal),
    select: (page) => page.totalCount > 0,
  })
}
