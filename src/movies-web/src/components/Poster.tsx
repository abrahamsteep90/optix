import { useState } from 'react'
import { posterSrcSet, tmdbImage } from '../lib/images'
import styles from './Poster.module.css'

interface Props {
  url: string
  /** How wide the poster is on screen, so the browser can pick an image size (the img "sizes" attribute). */
  sizes: string
  /** Load straight away instead of when scrolled into view. */
  eager?: boolean
}

/** A movie poster in a fixed 2:3 frame, with a placeholder if the image is missing. */
export function Poster({ url, sizes, eager = false }: Props) {
  const [failed, setFailed] = useState(false)

  if (failed) {
    return (
      <div className={styles.frame} aria-hidden="true">
        <svg className={styles.placeholder} viewBox="0 0 24 24">
          <rect x="3" y="5" width="18" height="14" rx="2" />
          <path d="M3 9.5h18M3 14.5h18M8 5v14M16 5v14" />
        </svg>
      </div>
    )
  }

  return (
    <div className={styles.frame}>
      {/* Decorative: the title is always shown next to the poster. */}
      <img
        className={styles.image}
        src={tmdbImage(url, 'w342')}
        srcSet={posterSrcSet(url)}
        sizes={sizes}
        alt=""
        width={342}
        height={513}
        loading={eager ? 'eager' : 'lazy'}
        decoding="async"
        onError={() => setFailed(true)}
      />
    </div>
  )
}
