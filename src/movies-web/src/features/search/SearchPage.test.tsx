import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { defaultApi, genres, mockApi, movie, page, renderApp, requestedUrls } from '../../test/utils'

const movieRequests = (fetchSpy: ReturnType<typeof mockApi>) =>
  requestedUrls(fetchSpy).filter((url) => url.pathname === '/api/movies')

describe('SearchPage', () => {
  it('shows the first page of movies, most popular first', async () => {
    const fetchSpy = mockApi(defaultApi())

    renderApp('/')

    expect(await screen.findByRole('heading', { name: 'Spider-Man: No Way Home' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'The Batman' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /2 movies/ })).toBeInTheDocument()

    const [request] = movieRequests(fetchSpy)
    expect(request.searchParams.get('sortBy')).toBe('popularity')
    expect(request.searchParams.get('page')).toBe('1')
    expect(request.searchParams.get('pageSize')).toBe('24')
  })

  it('searches once typing pauses and puts the search in the URL', async () => {
    const fetchSpy = mockApi(defaultApi())
    renderApp('/')
    await screen.findByRole('heading', { name: 'The Batman' })

    await userEvent.type(screen.getByRole('searchbox', { name: 'Search by title' }), 'batman')

    await waitFor(() => expect(screen.getByTestId('location')).toHaveTextContent('/?search=batman'))
    const searches = movieRequests(fetchSpy).map((url) => url.searchParams.get('search'))
    expect(searches).toContain('batman')
    expect(searches).not.toContain('b') // not one request per key press
  })

  it('filters by genre and starts again from page 1', async () => {
    const fetchSpy = mockApi(defaultApi())
    renderApp('/?page=3')
    await screen.findByRole('heading', { name: 'The Batman' })

    const animation = screen.getByRole('button', { name: 'Animation' })
    await userEvent.click(animation)

    expect(animation).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByTestId('location')).toHaveTextContent('/?genres=Animation')
    await waitFor(() => {
      const last = movieRequests(fetchSpy).at(-1)!
      expect(last.searchParams.getAll('genres')).toEqual(['Animation'])
      expect(last.searchParams.get('page')).toBe('1')
    })
  })

  it('reads the filters from the URL', async () => {
    const fetchSpy = mockApi(defaultApi())

    renderApp('/?search=bat&genres=Action&sortBy=title&sortDirection=desc&page=2&pageSize=12')

    await screen.findByRole('heading', { name: 'The Batman' })
    expect(screen.getByRole('searchbox')).toHaveValue('bat')
    expect(screen.getByRole('button', { name: 'Action' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('combobox', { name: 'Sort by' })).toHaveValue('Title Z–A')
    expect(screen.getByRole('combobox', { name: 'Per page' })).toHaveValue('12')

    const request = movieRequests(fetchSpy)[0]
    expect(Object.fromEntries(request.searchParams)).toMatchObject({
      search: 'bat',
      genres: 'Action',
      sortBy: 'title',
      sortDirection: 'desc',
      page: '2',
      pageSize: '12',
    })
  })

  it('links to the other pages', async () => {
    mockApi((url) =>
      url.pathname === '/api/movies' ? page([movie()], 100, 1, 24) : defaultApi()(url),
    )

    renderApp('/?search=a')

    const pages = await screen.findByRole('navigation', { name: 'Pages' })
    expect(within(pages).getByRole('link', { name: 'Page 1' })).toHaveAttribute('aria-current', 'page')
    expect(within(pages).getByRole('link', { name: 'Page 2' })).toHaveAttribute('href', '/?search=a&page=2')
    expect(within(pages).getByRole('link', { name: 'Page 5' })).toBeInTheDocument()
    expect(within(pages).getByRole('link', { name: /Next/ })).toHaveAttribute('href', '/?search=a&page=2')
  })

  it('says so when nothing matches, and can clear the filters', async () => {
    mockApi((url) =>
      url.pathname === '/api/movies' && url.searchParams.has('search') ? page([]) : defaultApi()(url),
    )
    renderApp('/?search=zzzz')

    expect(await screen.findByRole('heading', { name: 'No movies found' })).toBeInTheDocument()

    await userEvent.click(screen.getByRole('button', { name: 'Clear filters' }))

    expect(await screen.findByRole('heading', { name: 'The Batman' })).toBeInTheDocument()
    expect(screen.getByTestId('location')).toHaveTextContent(/^\/$/)
  })

  it('says so when the page is past the last one, and links to the last page', async () => {
    // 30 movies at 24 a page make 2 pages, so page 9 is empty.
    mockApi((url) =>
      url.pathname === '/api/movies'
        ? page([], 30, Number(url.searchParams.get('page')), 24)
        : defaultApi()(url),
    )
    renderApp('/?search=bat&page=9')

    expect(await screen.findByRole('heading', { name: 'There is no page 9' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: '30 movies' })).toBeInTheDocument() // no "page 9 of 2"
    expect(screen.getByRole('link', { name: 'Go to page 2' })).toHaveAttribute('href', '/?search=bat&page=2')
  })

  it('shows the API error and lets the user try again', async () => {
    let fail = true
    mockApi((url) =>
      url.pathname === '/api/movies' && fail
        ? Response.json({ title: 'The database is unavailable' }, { status: 503 })
        : defaultApi()(url),
    )
    renderApp('/')

    expect(await screen.findByRole('alert')).toHaveTextContent('The database is unavailable')

    fail = false
    await userEvent.click(screen.getByRole('button', { name: 'Try again' }))

    expect(await screen.findByRole('heading', { name: 'The Batman' })).toBeInTheDocument()
  })

  it('disables the actor filter until cast data has been loaded', async () => {
    mockApi(defaultApi())
    renderApp('/')

    await waitFor(() => expect(screen.getByRole('combobox', { name: 'Actor' })).toBeDisabled())
    expect(screen.getByText(/needs cast data from TMDB/)).toBeInTheDocument()
  })

  it('suggests actors and filters by the one picked', async () => {
    const fetchSpy = mockApi((url) => {
      if (url.pathname === '/api/actors') {
        return url.searchParams.has('search')
          ? page([{ id: 9, name: 'Zoë Kravitz', movieCount: 3, profileImageUrl: null }])
          : page([{ id: 1, name: 'Anyone', movieCount: 1, profileImageUrl: null }], 500)
      }
      return defaultApi()(url)
    })
    renderApp('/')
    const actor = screen.getByRole('combobox', { name: 'Actor' })
    await waitFor(() => expect(actor).toBeEnabled())

    await userEvent.type(actor, 'zo')
    await userEvent.click(await screen.findByRole('option', { name: /Zoë Kravitz/ }))

    expect(screen.getByTestId('location')).toHaveTextContent('/?actors=Zo%C3%AB+Kravitz')
    expect(screen.getByRole('button', { name: 'Remove Zoë Kravitz' })).toBeInTheDocument()
    await waitFor(() =>
      expect(movieRequests(fetchSpy).at(-1)!.searchParams.getAll('actors')).toEqual(['Zoë Kravitz']),
    )
  })

  it('shows the genres from the API', async () => {
    mockApi(defaultApi())
    renderApp('/')

    const group = await screen.findByRole('group', { name: 'Genres' })

    expect(within(group).getAllByRole('button').map((button) => button.textContent)).toEqual(
      genres.map((genre) => genre.name),
    )
  })
})
