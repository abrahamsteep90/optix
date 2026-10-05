import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// jsdom doesn't implement scrolling; the search page scrolls to the top when the page changes.
window.scrollTo = () => {}

afterEach(() => {
  cleanup()
  vi.restoreAllMocks()
})
