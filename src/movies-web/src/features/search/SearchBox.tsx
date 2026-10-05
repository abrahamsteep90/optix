import { useEffect, useEffectEvent, useState } from 'react'
import styles from './SearchBox.module.css'

interface Props {
  /** The search currently in the URL. */
  value: string
  onSearch: (search: string) => void
}

/** Title search. Waits until typing pauses before searching, so the API isn't called on every key press. */
export function SearchBox({ value, onSearch }: Props) {
  const [text, setText] = useState(value)
  const [previousValue, setPreviousValue] = useState(value)

  // The URL can change without typing (back button, "Clear filters"): show what it says.
  // Typing "bat " searches for "bat", so don't throw away the trailing space the user just typed.
  if (value !== previousValue) {
    setPreviousValue(value)
    if (value !== text.trim()) setText(value)
  }

  const search = useEffectEvent((next: string) => onSearch(next))

  useEffect(() => {
    const next = text.trim()
    if (next === value) return
    const timer = setTimeout(() => search(next), 300)
    return () => clearTimeout(timer)
  }, [text, value])

  return (
    <div className={styles.box}>
      <label htmlFor="search" className="visually-hidden">
        Search by title
      </label>
      <svg className={styles.icon} viewBox="0 0 24 24" aria-hidden="true">
        <circle cx="11" cy="11" r="7" />
        <path d="M20 20l-3.5-3.5" />
      </svg>
      <input
        id="search"
        className={styles.input}
        type="search"
        value={text}
        onChange={(event) => setText(event.target.value)}
        placeholder="Search by title, e.g. batman"
        autoComplete="off"
        spellCheck={false}
        maxLength={200}
      />
    </div>
  )
}
