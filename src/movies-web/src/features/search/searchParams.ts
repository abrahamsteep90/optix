import type { MovieQuery, SortBy, SortDirection } from '../../api/types'

// The page's URL uses the same parameter names as the API, e.g. /?search=batman&genres=Action&page=2,
// so every search can be bookmarked or shared, and the back button works.

export const pageSizes = [12, 24, 48, 96] as const
export const defaultPageSize = 24

export const sortOptions = [
  { label: 'Most popular', sortBy: 'popularity' },
  { label: 'Highest rated', sortBy: 'rating' },
  { label: 'Newest first', sortBy: 'releaseDate' },
  { label: 'Oldest first', sortBy: 'releaseDate', sortDirection: 'asc' },
  { label: 'Title A–Z', sortBy: 'title' },
  { label: 'Title Z–A', sortBy: 'title', sortDirection: 'desc' },
] as const satisfies readonly { label: string; sortBy: SortBy; sortDirection?: SortDirection }[]

const sortByValues: readonly string[] = ['popularity', 'title', 'releaseDate', 'rating']

export function readQuery(params: URLSearchParams): MovieQuery {
  const sortBy = params.get('sortBy') ?? ''
  const sortDirection = params.get('sortDirection')
  const page = Number(params.get('page'))
  const pageSize = Number(params.get('pageSize'))

  return {
    search: params.get('search')?.trim() ?? '',
    genres: params.getAll('genres'),
    actors: params.getAll('actors'),
    sortBy: sortByValues.includes(sortBy) ? (sortBy as SortBy) : 'popularity',
    sortDirection: sortDirection === 'asc' || sortDirection === 'desc' ? sortDirection : undefined,
    page: Number.isInteger(page) && page >= 1 ? page : 1,
    pageSize: (pageSizes as readonly number[]).includes(pageSize) ? pageSize : defaultPageSize,
  }
}

/** The opposite of readQuery. Leaves out default values to keep URLs short. */
export function writeQuery(query: MovieQuery): URLSearchParams {
  const params = new URLSearchParams()
  if (query.search) params.set('search', query.search)
  query.genres.forEach((genre) => params.append('genres', genre))
  query.actors.forEach((actor) => params.append('actors', actor))
  if (query.sortBy !== 'popularity') params.set('sortBy', query.sortBy)
  if (query.sortDirection) params.set('sortDirection', query.sortDirection)
  if (query.page !== 1) params.set('page', String(query.page))
  if (query.pageSize !== defaultPageSize) params.set('pageSize', String(query.pageSize))
  return params
}

export function hasFilters(query: MovieQuery): boolean {
  return query.search !== '' || query.genres.length > 0 || query.actors.length > 0
}
