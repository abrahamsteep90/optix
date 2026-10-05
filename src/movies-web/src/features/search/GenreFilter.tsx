import { useGenres } from '../../api/queries'
import { formatCount } from '../../lib/format'
import styles from './GenreFilter.module.css'

interface Props {
  selected: string[]
  onChange: (genres: string[]) => void
}

const same = (a: string, b: string) => a.toLowerCase() === b.toLowerCase()

/** One toggle per genre. Selecting several shows movies that have all of them. */
export function GenreFilter({ selected, onChange }: Props) {
  const genres = useGenres()

  if (!genres.data) {
    // Still loading, or failed: search works without the genre list, so just leave it out.
    return <div className={styles.placeholder} />
  }

  function toggle(name: string) {
    onChange(selected.some((s) => same(s, name)) ? selected.filter((s) => !same(s, name)) : [...selected, name])
  }

  return (
    <fieldset className={styles.genres}>
      <legend className="visually-hidden">Genres</legend>
      {genres.data.map((genre) => (
        <button
          key={genre.id}
          type="button"
          className="chip"
          aria-pressed={selected.some((s) => same(s, genre.name))}
          title={`${formatCount(genre.movieCount)} movies`}
          onClick={() => toggle(genre.name)}
        >
          {genre.name}
        </button>
      ))}
    </fieldset>
  )
}
