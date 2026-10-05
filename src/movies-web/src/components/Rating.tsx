import styles from './Rating.module.css'

/** "★ 8.1", or a note when nobody has voted (100 movies in the dataset have no votes). */
export function Rating({ value }: { value: number | null }) {
  if (value === null) {
    return <span className={styles.unrated}>No votes yet</span>
  }

  return (
    <span className={styles.rating}>
      <svg className={styles.star} viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 2.5l2.9 6.2 6.6.7-5 4.5 1.4 6.6L12 17.2l-5.9 3.3 1.4-6.6-5-4.5 6.6-.7z" />
      </svg>
      {value.toFixed(1)}
      <span className="visually-hidden"> out of 10</span>
    </span>
  )
}
