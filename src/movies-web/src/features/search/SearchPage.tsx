import { useEffect, useMemo } from 'react'
import { useSearchParams } from 'react-router'
import { useMovies } from '../../api/queries'
import type { MovieQuery } from '../../api/types'
import { StatusMessage } from '../../components/StatusMessage'
import { plural } from '../../lib/format'
import { ActorFilter } from './ActorFilter'
import { GenreFilter } from './GenreFilter'
import { MovieGrid } from './MovieGrid'
import { Pagination } from './Pagination'
import { SearchBox } from './SearchBox'
import { hasFilters, readQuery, writeQuery } from './searchParams'
import styles from './SearchPage.module.css'
import { SortAndPageSize } from './SortAndPageSize'

/** The home page: search, filter, sort and page through movies. All state lives in the URL. */
export function SearchPage() {
  const [params, setParams] = useSearchParams()
  const query = useMemo(() => readQuery(params), [params])
  const movies = useMovies(query)

  // Any change except the page number itself starts again from page 1.
  function update(changes: Partial<MovieQuery>, options: { replace?: boolean } = {}) {
    setParams(writeQuery({ ...query, page: 1, ...changes }), options)
  }

  const linkToPage = (page: number) => `?${writeQuery({ ...query, page })}`

  useEffect(() => {
    window.scrollTo({ top: 0 })
  }, [query.page])

  const result = movies.data

  return (
    <>
      <title>{query.search ? `“${query.search}” · Movies` : 'Movies'}</title>

      <section className={styles.controls} aria-labelledby="search-heading">
        <h1 id="search-heading" className={styles.heading}>
          Find a movie
        </h1>
        {/* Typing updates the URL without adding a history entry for every letter. */}
        <SearchBox value={query.search} onSearch={(search) => update({ search }, { replace: true })} />
        <GenreFilter selected={query.genres} onChange={(genres) => update({ genres })} />
        <div className={styles.toolbar}>
          <ActorFilter selected={query.actors} onChange={(actors) => update({ actors })} />
          <SortAndPageSize
            query={query}
            onSortChange={(sortBy, sortDirection) => update({ sortBy, sortDirection })}
            onPageSizeChange={(pageSize) => update({ pageSize })}
          />
        </div>
      </section>

      <section aria-labelledby="results-heading">
        <div className={styles.resultsBar}>
          <h2 id="results-heading" className={styles.count} aria-live="polite">
            {result ? plural(result.totalCount, 'movie') : 'Loading movies…'}
            {result && result.totalPages > 1 && (
              <span className={styles.pageOf}>
                {' '}
                · page {result.page} of {result.totalPages}
              </span>
            )}
          </h2>
          {hasFilters(query) && result?.totalCount !== 0 && (
            <button
              type="button"
              className="link-button"
              onClick={() => update({ search: '', genres: [], actors: [] })}
            >
              Clear filters
            </button>
          )}
        </div>

        {movies.isError && !result ? (
          <StatusMessage
            title="Couldn't load movies"
            isError
            action={
              <button type="button" className="button" onClick={() => movies.refetch()}>
                Try again
              </button>
            }
          >
            {movies.error.message} Check that the API is running.
          </StatusMessage>
        ) : result && result.totalCount === 0 ? (
          <StatusMessage
            title="No movies found"
            action={
              <button
                type="button"
                className="button"
                onClick={() => update({ search: '', genres: [], actors: [] })}
              >
                Clear filters
              </button>
            }
          >
            Nothing matches all of your filters. Try fewer genres or a shorter title.
          </StatusMessage>
        ) : (
          <>
            <MovieGrid
              movies={result?.items}
              skeletons={Math.min(query.pageSize, 12)}
              updating={movies.isPlaceholderData}
            />
            {result && <Pagination page={result.page} totalPages={result.totalPages} linkTo={linkToPage} />}
          </>
        )}
      </section>
    </>
  )
}
