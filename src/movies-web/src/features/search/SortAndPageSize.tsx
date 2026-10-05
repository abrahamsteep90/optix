import type { MovieQuery, SortBy, SortDirection } from '../../api/types'
import { pageSizes, sortOptions } from './searchParams'

interface Props {
  query: MovieQuery
  onSortChange: (sortBy: SortBy, sortDirection: SortDirection | undefined) => void
  onPageSizeChange: (pageSize: number) => void
}

// The API sorts titles A–Z and everything else highest/newest first unless told otherwise.
const effectiveDirection = (sortBy: SortBy, sortDirection?: SortDirection) =>
  sortDirection ?? (sortBy === 'title' ? 'asc' : 'desc')

export function SortAndPageSize({ query, onSortChange, onPageSizeChange }: Props) {
  const current = sortOptions.find(
    (option) =>
      option.sortBy === query.sortBy &&
      effectiveDirection(option.sortBy, 'sortDirection' in option ? option.sortDirection : undefined) ===
        effectiveDirection(query.sortBy, query.sortDirection),
  )

  return (
    <>
      <label className="field">
        <span className="field-label">Sort by</span>
        <select
          className="select"
          value={current?.label ?? 'custom'}
          onChange={(event) => {
            const option = sortOptions.find((o) => o.label === event.target.value)
            if (option) onSortChange(option.sortBy, 'sortDirection' in option ? option.sortDirection : undefined)
          }}
        >
          {!current && (
            <option value="custom" disabled>
              Custom order
            </option>
          )}
          {sortOptions.map((option) => (
            <option key={option.label} value={option.label}>
              {option.label}
            </option>
          ))}
        </select>
      </label>

      <label className="field">
        <span className="field-label">Per page</span>
        <select
          className="select"
          value={query.pageSize}
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
        >
          {pageSizes.map((size) => (
            <option key={size} value={size}>
              {size}
            </option>
          ))}
        </select>
      </label>
    </>
  )
}
