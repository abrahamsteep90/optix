import type { MovieSummary } from '../../api/types'
import { MovieCard } from './MovieCard'
import styles from './MovieGrid.module.css'

interface Props {
  movies: MovieSummary[] | undefined
  /** Grey placeholder cards to show before the first results arrive. */
  skeletons: number
  /** True while a new page loads; the current one stays visible but dimmed. */
  updating: boolean
}

export function MovieGrid({ movies, skeletons, updating }: Props) {
  if (!movies) {
    return (
      <ul className={styles.grid} aria-busy="true" aria-label="Loading movies">
        {Array.from({ length: skeletons }, (_, index) => (
          <li key={index} className={styles.skeleton} />
        ))}
      </ul>
    )
  }

  return (
    <ul className={styles.grid} data-updating={updating} aria-busy={updating}>
      {movies.map((movie) => (
        <MovieCard key={movie.id} movie={movie} />
      ))}
    </ul>
  )
}
