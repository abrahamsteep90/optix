import type { ReactNode } from 'react'
import styles from './StatusMessage.module.css'

interface Props {
  title: string
  children: ReactNode
  action?: ReactNode
  /** Errors are announced to screen readers straight away. */
  isError?: boolean
}

/** A centred message for empty results, errors and missing pages. */
export function StatusMessage({ title, children, action, isError = false }: Props) {
  return (
    <div className={styles.status} role={isError ? 'alert' : 'status'}>
      <h2 className={styles.title}>{title}</h2>
      <p className={styles.text}>{children}</p>
      {action}
    </div>
  )
}
