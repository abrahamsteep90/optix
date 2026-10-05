import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { pageItems } from './pageItems'
import styles from './Pagination.module.css'

interface Props {
  page: number
  totalPages: number
  /** The link (search string) for a page, e.g. "?search=batman&page=3". */
  linkTo: (page: number) => string
}

/** Real links rather than buttons, so pages can be opened in a new tab or shared. */
export function Pagination({ page, totalPages, linkTo }: Props) {
  if (totalPages <= 1) return null

  return (
    <nav className={styles.pagination} aria-label="Pages">
      <PageLink page={page - 1} totalPages={totalPages} linkTo={linkTo} className={styles.step}>
        <span aria-hidden="true">‹</span> Previous
      </PageLink>

      <ol className={styles.pages}>
        {pageItems(page, totalPages).map((item, index) =>
          item === 'gap' ? (
            <li key={`gap-${index}`} className={styles.gap} aria-hidden="true">
              …
            </li>
          ) : (
            <li key={item}>
              <Link
                to={linkTo(item)}
                className={styles.page}
                aria-label={`Page ${item}`}
                aria-current={item === page ? 'page' : undefined}
              >
                {item}
              </Link>
            </li>
          ),
        )}
      </ol>

      <PageLink page={page + 1} totalPages={totalPages} linkTo={linkTo} className={styles.step}>
        Next <span aria-hidden="true">›</span>
      </PageLink>
    </nav>
  )
}

function PageLink({
  page,
  totalPages,
  linkTo,
  className,
  children,
}: Omit<Props, 'page'> & { page: number; className: string; children: ReactNode }) {
  if (page < 1 || page > totalPages) {
    return (
      <span className={className} aria-disabled="true">
        {children}
      </span>
    )
  }

  return (
    <Link to={linkTo(page)} className={className}>
      {children}
    </Link>
  )
}
