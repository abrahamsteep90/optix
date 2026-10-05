/** Widths TMDB's image CDN serves. */
export type ImageSize = 'w185' | 'w342' | 'w500' | 'w780'

/**
 * The dataset links to full-size posters (/t/p/original/…, often 1–2 MB each).
 * TMDB serves the same image at fixed widths, e.g. /t/p/w342/… at around 60 KB.
 */
export function tmdbImage(url: string, size: ImageSize): string {
  return url.replace('/t/p/original/', `/t/p/${size}/`)
}

/** srcset for a poster, so browsers pick the right size for the screen. */
export function posterSrcSet(url: string): string {
  return (['w185', 'w342', 'w500'] as const)
    .map((size) => `${tmdbImage(url, size)} ${size.slice(1)}w`)
    .join(', ')
}
