const languageNames = new Intl.DisplayNames(['en'], { type: 'language' })

/** "ja" → "Japanese". TMDB uses "cn" for Cantonese, which isn't a standard code. */
export function languageName(code: string): string {
  if (code === 'cn') return 'Cantonese'
  try {
    return languageNames.of(code) ?? code
  } catch {
    return code
  }
}

/** "2021-12-15" → "2021" */
export function releaseYear(date: string): string {
  return date.slice(0, 4)
}

/** "2021-12-15" → "15 December 2021" (read as a calendar date, so no time zone shifts). */
export function longDate(date: string): string {
  const [year, month, day] = date.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day)).toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    timeZone: 'UTC',
  })
}

export function formatCount(value: number): string {
  return value.toLocaleString('en-GB')
}

export function plural(count: number, one: string, many = `${one}s`): string {
  return `${formatCount(count)} ${count === 1 ? one : many}`
}
