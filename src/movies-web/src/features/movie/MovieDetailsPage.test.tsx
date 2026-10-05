import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { defaultApi, mockApi, movie, renderApp } from '../../test/utils'

describe('MovieDetailsPage', () => {
  it('shows the movie with its genres and cast', async () => {
    mockApi((url) =>
      url.pathname === '/api/movies/2'
        ? movie({
            id: 2,
            title: 'The Batman',
            releaseDate: '2022-03-01',
            voteAverage: 8.1,
            voteCount: 1151,
            genres: ['Crime', 'Mystery'],
            cast: [
              { actorId: 7, name: 'Zoë Kravitz', character: 'Selina Kyle', profileImageUrl: null },
              { actorId: 5, name: 'Robert Pattinson', character: 'Bruce Wayne', profileImageUrl: null },
            ],
          })
        : defaultApi()(url),
    )

    renderApp('/movies/2')

    expect(await screen.findByRole('heading', { level: 1, name: /The Batman/ })).toHaveTextContent('(2022)')
    expect(screen.getByText('1 March 2022')).toBeInTheDocument()
    expect(screen.getByText('English')).toBeInTheDocument()
    expect(screen.getByText(/from 1,151 votes/)).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Crime' })).toHaveAttribute('href', '/?genres=Crime')
    expect(screen.getByRole('link', { name: 'Zoë Kravitz' })).toHaveAttribute('href', '/?actors=Zo%C3%AB%20Kravitz')
    expect(screen.getByText('Selina Kyle')).toBeInTheDocument()
  })

  it('keeps line breaks in the description', async () => {
    mockApi((url) =>
      url.pathname === '/api/movies/3' ? movie({ id: 3, overview: 'Plus shorts:\n - One\n - Two' }) : defaultApi()(url),
    )

    renderApp('/movies/3')

    const overview = await screen.findByText(/Plus shorts:/)
    expect(overview.textContent).toBe('Plus shorts:\n - One\n - Two')
  })

  it('says when there is no such movie', async () => {
    mockApi(defaultApi())

    renderApp('/movies/999999')

    expect(await screen.findByRole('heading', { name: 'Movie not found' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /All movies/ })).toHaveAttribute('href', '/')
  })
})
