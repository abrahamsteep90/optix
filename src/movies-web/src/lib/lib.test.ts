import { describe, expect, it } from 'vitest'
import { moviesUrl } from '../api/client'
import { languageName, longDate, plural } from './format'
import { posterSrcSet, tmdbImage } from './images'

const poster = 'https://image.tmdb.org/t/p/original/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg'

describe('tmdbImage', () => {
  it('asks TMDB for a smaller copy of the poster', () => {
    expect(tmdbImage(poster, 'w342')).toBe('https://image.tmdb.org/t/p/w342/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg')
  })

  it('offers three sizes to the browser', () => {
    expect(posterSrcSet(poster)).toBe(
      'https://image.tmdb.org/t/p/w185/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg 185w, ' +
        'https://image.tmdb.org/t/p/w342/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg 342w, ' +
        'https://image.tmdb.org/t/p/w500/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg 500w',
    )
  })
})

describe('format', () => {
  it('names languages, including TMDB’s "cn" for Cantonese', () => {
    expect(languageName('ja')).toBe('Japanese')
    expect(languageName('cn')).toBe('Cantonese')
  })

  it('formats dates without time zone surprises', () => {
    expect(longDate('2021-12-15')).toBe('15 December 2021')
  })

  it('pluralises counts', () => {
    expect(plural(1, 'movie')).toBe('1 movie')
    expect(plural(9827, 'movie')).toBe('9,827 movies')
  })
})

describe('moviesUrl', () => {
  it('turns a query into the API URL', () => {
    expect(
      moviesUrl({
        search: 'bat man',
        genres: ['Action', 'Science Fiction'],
        actors: [],
        sortBy: 'releaseDate',
        sortDirection: 'asc',
        page: 2,
        pageSize: 24,
      }),
    ).toBe(
      '/api/movies?search=bat+man&genres=Action&genres=Science+Fiction&sortBy=releaseDate&sortDirection=asc&page=2&pageSize=24',
    )
  })
})
