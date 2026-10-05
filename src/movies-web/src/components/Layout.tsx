import { Link, Outlet } from 'react-router'
import tmdbLogo from '../assets/tmdb-logo.svg'
import styles from './Layout.module.css'

export function Layout() {
  return (
    <div className={styles.page}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <Link to="/" className={styles.brand}>
            <svg className={styles.logo} viewBox="0 0 24 24" aria-hidden="true">
              <rect x="3" y="5" width="18" height="14" rx="2" />
              <path d="M3 9.5h18M3 14.5h18M8 5v14M16 5v14" />
            </svg>
            Movies
          </Link>
          <a className={styles.headerLink} href="/swagger">
            API docs
          </a>
        </div>
      </header>

      <main className={styles.main}>
        <Outlet />
      </main>

      <footer className={styles.footer}>
        <div className={styles.footerInner}>
          <a href="https://www.themoviedb.org" className={styles.tmdb}>
            <img src={tmdbLogo} alt="TMDB" width={120} height={16} />
          </a>
          <p>
            This product uses TMDB and the TMDB APIs but is not endorsed, certified, or otherwise approved by
            TMDB.
          </p>
          <p>
            Movie data: <a href="https://www.kaggle.com/datasets/disham993/9000-movies-dataset">9000+ Movies Dataset</a>{' '}
            on Kaggle (CC0), a snapshot of TMDB from March 2022.
          </p>
        </div>
      </footer>
    </div>
  )
}
