import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import { MemoryRouter, useLocation } from 'react-router'
import { vi } from 'vitest'
import { App } from '../App'
import type { MovieDetails, MovieSummary, PagedResult } from '../api/types'

/** Renders the whole app at `url`, with a fresh cache and a probe that shows the current URL. */
export function renderApp(url = '/') {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[url]}>
        <App />
        <LocationProbe />
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

function LocationProbe() {
  const location = useLocation()
  return <output data-testid="location">{location.pathname + location.search}</output>
}

type Handler = (url: URL) => unknown

/**
 * Replaces fetch with a fake API. `handler` gets each request's URL and returns the JSON body,
 * or a Response for anything else (e.g. an error status).
 */
export function mockApi(handler: Handler) {
  return vi.spyOn(globalThis, 'fetch').mockImplementation(async (input) => {
    const url = new URL(String(input), 'http://localhost')
    const result = handler(url)
    return result instanceof Response ? result : Response.json(result)
  })
}

/** The URLs fetch has been called with so far. */
export function requestedUrls(fetchSpy: ReturnType<typeof mockApi>): URL[] {
  return fetchSpy.mock.calls.map(([input]) => new URL(String(input), 'http://localhost'))
}

export function page<T>(items: T[], totalCount = items.length, pageNumber = 1, pageSize = 24): PagedResult<T> {
  return { items, page: pageNumber, pageSize, totalCount, totalPages: Math.ceil(totalCount / pageSize) }
}

export function movie(overrides: Partial<MovieDetails> = {}): MovieDetails {
  return {
    id: 1,
    title: 'Spider-Man: No Way Home',
    releaseDate: '2021-12-15',
    overview: 'Peter Parker is unmasked.',
    popularity: 5083.954,
    voteCount: 8940,
    voteAverage: 8.3,
    originalLanguage: 'en',
    posterUrl: 'https://image.tmdb.org/t/p/original/spider.jpg',
    genres: ['Action', 'Adventure', 'Science Fiction'],
    cast: [],
    ...overrides,
  }
}

export const genres = [
  { id: 1, name: 'Action', movieCount: 2686 },
  { id: 7, name: 'Animation', movieCount: 1439 },
  { id: 4, name: 'Comedy', movieCount: 3031 },
]

/** A fake API with two movies, three genres and no cast data. */
export function defaultApi(movies: MovieSummary[] = [movie(), movie({ id: 2, title: 'The Batman', releaseDate: '2022-03-01', voteAverage: 8.1 })]): Handler {
  return (url) => {
    if (url.pathname === '/api/genres') return genres
    if (url.pathname === '/api/actors') return page([])
    if (url.pathname === '/api/movies') return page(movies)
    return new Response(null, { status: 404 })
  }
}
