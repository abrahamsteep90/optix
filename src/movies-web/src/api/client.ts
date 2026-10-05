import type { MovieQuery } from './types'

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function getJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(url, { headers: { Accept: 'application/json' }, signal })

  if (!response.ok) {
    // Errors come back as RFC 9457 problem details; their title is written for people.
    const problem = (await response.json().catch(() => null)) as { title?: string } | null
    throw new ApiError(response.status, problem?.title ?? `The request failed (${response.status}).`)
  }

  return (await response.json()) as T
}

/** The API URL for a movie query, e.g. /api/movies?search=batman&genres=Action&page=2&pageSize=24. */
export function moviesUrl(query: MovieQuery): string {
  const params = new URLSearchParams()
  if (query.search) params.set('search', query.search)
  query.genres.forEach((genre) => params.append('genres', genre))
  query.actors.forEach((actor) => params.append('actors', actor))
  params.set('sortBy', query.sortBy)
  if (query.sortDirection) params.set('sortDirection', query.sortDirection)
  params.set('page', String(query.page))
  params.set('pageSize', String(query.pageSize))
  return `/api/movies?${params}`
}
