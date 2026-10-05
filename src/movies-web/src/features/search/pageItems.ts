export type PageItem = number | 'gap'

/**
 * The page links to show: always the first and last page, plus the pages around the current one.
 * pageItems(6, 26) → [1, 'gap', 5, 6, 7, 'gap', 26]
 * A gap never hides just one page; that page is shown instead.
 */
export function pageItems(current: number, total: number): PageItem[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1)
  }

  const start = Math.max(2, Math.min(current - 1, total - 4))
  const end = Math.min(total - 1, Math.max(current + 1, 5))

  const items: PageItem[] = [1]
  if (start === 3) items.push(2)
  else if (start > 3) items.push('gap')

  for (let page = start; page <= end; page++) items.push(page)

  if (end === total - 2) items.push(total - 1)
  else if (end < total - 2) items.push('gap')
  items.push(total)

  return items
}
