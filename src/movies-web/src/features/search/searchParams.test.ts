import { describe, expect, it } from 'vitest'
import { defaultPageSize, readQuery, writeQuery } from './searchParams'

describe('readQuery', () => {
  it('falls back to defaults for missing or invalid values', () => {
    const query = readQuery(new URLSearchParams('sortBy=banana&sortDirection=up&page=-3&pageSize=1000'))

    expect(query).toEqual({
      search: '',
      genres: [],
      actors: [],
      sortBy: 'popularity',
      sortDirection: undefined,
      page: 1,
      pageSize: defaultPageSize,
    })
  })

  it('reads repeated genres and actors', () => {
    const query = readQuery(new URLSearchParams('genres=Action&genres=Comedy&actors=Tom+Hanks'))

    expect(query.genres).toEqual(['Action', 'Comedy'])
    expect(query.actors).toEqual(['Tom Hanks'])
  })
})

describe('writeQuery', () => {
  it('leaves out default values, so URLs stay short', () => {
    expect(writeQuery(readQuery(new URLSearchParams())).toString()).toBe('')
  })

  it('round-trips through readQuery', () => {
    const url = 'search=bat&genres=Action&genres=Comedy&actors=Zo%C3%AB+Kravitz&sortBy=title&sortDirection=desc&page=3&pageSize=48'

    expect(writeQuery(readQuery(new URLSearchParams(url))).toString()).toBe(url)
  })
})
