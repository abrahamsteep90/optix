import { Link, useLocation, useParams } from 'react-router'
import { ApiError } from '../../api/client'
import { useHasCastData, useMovie } from '../../api/queries'
import type { CastMember } from '../../api/types'
import { Poster } from '../../components/Poster'
import { Rating } from '../../components/Rating'
import { StatusMessage } from '../../components/StatusMessage'
import { languageName, longDate, plural, releaseYear } from '../../lib/format'
import styles from './MovieDetailsPage.module.css'

export function MovieDetailsPage() {
  const { id } = useParams()
  const movieId = Number(id)
  const movie = useMovie(movieId)
  const hasCastData = useHasCastData()

  // Cards pass the search they came from, so "Back" returns to the same results and page.
  const from = (useLocation().state as { from?: string } | null)?.from
  const backLink = (
    <Link to={{ pathname: '/', search: from ?? '' }} className={styles.back}>
      <span aria-hidden="true">←</span> {from ? 'Back to results' : 'All movies'}
    </Link>
  )

  const notFound =
    !Number.isInteger(movieId) || movieId <= 0 || (movie.error instanceof ApiError && movie.error.status === 404)

  if (notFound) {
    return (
      <>
        {backLink}
        <StatusMessage title="Movie not found">There is no movie with this id.</StatusMessage>
      </>
    )
  }

  if (movie.isError) {
    return (
      <>
        {backLink}
        <StatusMessage
          title="Couldn't load this movie"
          isError
          action={
            <button type="button" className="button" onClick={() => movie.refetch()}>
              Try again
            </button>
          }
        >
          {movie.error.message}
        </StatusMessage>
      </>
    )
  }

  if (!movie.data) {
    return (
      <>
        {backLink}
        <div className={styles.loading} aria-busy="true" aria-label="Loading movie" />
      </>
    )
  }

  const details = movie.data

  return (
    <article>
      <title>{`${details.title} (${releaseYear(details.releaseDate)}) · Movies`}</title>
      {backLink}

      <div className={styles.layout}>
        <div className={styles.poster}>
          <Poster url={details.posterUrl} sizes="(max-width: 700px) 60vw, 320px" eager />
        </div>

        <div className={styles.info}>
          <h1 className={styles.title}>
            {details.title} <span className={styles.year}>({releaseYear(details.releaseDate)})</span>
          </h1>

          <dl className={styles.facts}>
            <div>
              <dt>Rating</dt>
              <dd>
                <Rating value={details.voteAverage} />
                {details.voteCount > 0 && <span className={styles.muted}> from {plural(details.voteCount, 'vote')}</span>}
              </dd>
            </div>
            <div>
              <dt>Released</dt>
              <dd>{longDate(details.releaseDate)}</dd>
            </div>
            <div>
              <dt>Language</dt>
              <dd>{languageName(details.originalLanguage)}</dd>
            </div>
          </dl>

          <ul className={styles.genres} aria-label="Genres">
            {details.genres.map((genre) => (
              <li key={genre}>
                <Link className="chip" to={`/?genres=${encodeURIComponent(genre)}`}>
                  {genre}
                </Link>
              </li>
            ))}
          </ul>

          <section className={styles.section}>
            <h2>Overview</h2>
            <p className={styles.overview}>{details.overview}</p>
          </section>

          <section className={styles.section}>
            <h2>Cast</h2>
            {details.cast.length > 0 ? (
              <CastList cast={details.cast} />
            ) : (
              <p className={styles.muted}>
                {hasCastData.data === false
                  ? 'Cast lists appear once the API has loaded them from TMDB (see the README).'
                  : 'No cast information for this movie.'}
              </p>
            )}
          </section>
        </div>
      </div>
    </article>
  )
}

function CastList({ cast }: { cast: CastMember[] }) {
  return (
    <ul className={styles.cast}>
      {cast.map((member) => (
        <li key={member.actorId} className={styles.castMember}>
          {member.profileImageUrl ? (
            <img className={styles.photo} src={member.profileImageUrl} alt="" loading="lazy" />
          ) : (
            <span className={styles.photo} aria-hidden="true" />
          )}
          <span className={styles.castText}>
            <Link to={`/?actors=${encodeURIComponent(member.name)}`} className={styles.actor}>
              {member.name}
            </Link>
            {member.character && <span className={styles.muted}>{member.character}</span>}
          </span>
        </li>
      ))}
    </ul>
  )
}
