import { Link, useLocation } from 'react-router'
import type { MovieSummary } from '../../api/types'
import { Poster } from '../../components/Poster'
import { Rating } from '../../components/Rating'
import { releaseYear } from '../../lib/format'
import styles from './MovieCard.module.css'

export function MovieCard({ movie }: { movie: MovieSummary }) {
  const location = useLocation()

  return (
    <li className={styles.card}>
      {/* Remember the search, so the details page can link back to the same results. */}
      <Link to={`/movies/${movie.id}`} state={{ from: location.search }} className={styles.link}>
        <Poster url={movie.posterUrl} sizes="(max-width: 520px) 45vw, (max-width: 900px) 30vw, 200px" />
        <div className={styles.body}>
          <h3 className={styles.title}>{movie.title}</h3>
          <p className={styles.meta}>
            <span>{releaseYear(movie.releaseDate)}</span>
            <Rating value={movie.voteAverage} />
          </p>
          <p className={styles.genres}>{movie.genres.join(' · ')}</p>
        </div>
      </Link>
    </li>
  )
}
