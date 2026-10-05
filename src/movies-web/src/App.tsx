import { Route, Routes } from 'react-router'
import { Layout } from './components/Layout'
import { NotFoundPage } from './components/NotFoundPage'
import { MovieDetailsPage } from './features/movie/MovieDetailsPage'
import { SearchPage } from './features/search/SearchPage'

export function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<SearchPage />} />
        <Route path="movies/:id" element={<MovieDetailsPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}
