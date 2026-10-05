import { Link } from 'react-router'
import { StatusMessage } from './StatusMessage'

export function NotFoundPage() {
  return (
    <StatusMessage title="Page not found" action={<Link to="/" className="button">Browse all movies</Link>}>
      There is nothing at this address.
    </StatusMessage>
  )
}
