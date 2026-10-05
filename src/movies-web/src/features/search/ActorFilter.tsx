import { useId, useState, type KeyboardEvent } from 'react'
import { useActorSuggestions, useHasCastData } from '../../api/queries'
import { plural } from '../../lib/format'
import { useDebouncedValue } from '../../lib/useDebouncedValue'
import styles from './ActorFilter.module.css'

interface Props {
  selected: string[]
  onChange: (actors: string[]) => void
}

/**
 * Type part of a name and pick an actor from the suggestions (an ARIA combobox: arrow keys,
 * Enter and Escape work). Disabled until cast data has been loaded from TMDB.
 */
export function ActorFilter({ selected, onChange }: Props) {
  const hasCastData = useHasCastData()
  const [text, setText] = useState('')
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const search = useDebouncedValue(text.trim(), 250)
  const suggestions = useActorSuggestions(search)
  const listId = useId()

  const disabled = hasCastData.data === false
  const options = (suggestions.data?.items ?? []).filter((actor) => !selected.includes(actor.name))
  const showList = open && text.trim().length >= 2 && search.length >= 2

  function add(name: string) {
    onChange([...selected, name])
    setText('')
    setOpen(false)
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === 'Escape') {
      setOpen(false)
      return
    }
    if (!showList || options.length === 0) return

    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setActive((active + 1) % options.length)
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActive((active - 1 + options.length) % options.length)
    } else if (event.key === 'Enter') {
      event.preventDefault()
      add(options[Math.min(active, options.length - 1)].name)
    }
  }

  return (
    <div className={styles.filter}>
      <label htmlFor="actor" className="field-label">
        Actor
      </label>
      <div className={styles.combo}>
        <input
          id="actor"
          className={`input ${styles.input}`}
          role="combobox"
          aria-expanded={showList}
          aria-controls={listId}
          aria-autocomplete="list"
          aria-activedescendant={showList && options.length > 0 ? `${listId}-${active}` : undefined}
          aria-describedby={disabled ? `${listId}-hint` : undefined}
          value={text}
          disabled={disabled}
          placeholder={disabled ? 'No cast data yet' : 'e.g. Tom Hanks'}
          autoComplete="off"
          spellCheck={false}
          onChange={(event) => {
            setText(event.target.value)
            setOpen(true)
            setActive(0)
          }}
          onFocus={() => setOpen(true)}
          onBlur={() => setOpen(false)}
          onKeyDown={onKeyDown}
        />
        {showList && (
          <div className={styles.popup}>
            <ul id={listId} role="listbox" aria-label="Actors" className={styles.listbox}>
              {options.map((actor, index) => (
                <li
                  key={actor.id}
                  id={`${listId}-${index}`}
                  role="option"
                  aria-selected={index === active}
                  className={styles.option}
                  // Keep focus in the input, so the list doesn't close before the click lands.
                  onMouseDown={(event) => event.preventDefault()}
                  onMouseEnter={() => setActive(index)}
                  onClick={() => add(actor.name)}
                >
                  {actor.profileImageUrl ? (
                    <img className={styles.avatar} src={actor.profileImageUrl} alt="" loading="lazy" />
                  ) : (
                    <span className={styles.avatar} aria-hidden="true" />
                  )}
                  <span className={styles.name}>{actor.name}</span>
                  <span className={styles.count}>{plural(actor.movieCount, 'movie')}</span>
                </li>
              ))}
            </ul>
            {options.length === 0 && (
              <p className={styles.empty} role="status">
                {suggestions.isFetching ? 'Searching…' : 'No actors found'}
              </p>
            )}
          </div>
        )}
      </div>

      {disabled && (
        <p id={`${listId}-hint`} className={styles.hint}>
          Filtering by actor needs cast data from TMDB (see the README).
        </p>
      )}

      {selected.length > 0 && (
        <ul className={styles.selected} aria-label="Selected actors">
          {selected.map((name) => (
            <li key={name}>
              <button
                type="button"
                className="chip"
                aria-pressed="true"
                aria-label={`Remove ${name}`}
                onClick={() => onChange(selected.filter((actor) => actor !== name))}
              >
                {name} <span aria-hidden="true">×</span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
