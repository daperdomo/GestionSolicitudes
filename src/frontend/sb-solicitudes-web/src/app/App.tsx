import homeIcon from '../assets/icons/home.svg'
import styles from './App.module.css'

function App() {
  return (
    <main className={styles.page}>
      <section className={styles.card} aria-labelledby="page-title">
        <img
          className={styles.logo}
          src="/assets/branding/sb-logo.png"
          alt="Superintendencia de Bancos de la República Dominicana"
        />

        <div className={styles.heading}>
          <img src={homeIcon} alt="" width="24" height="24" />
          <h1 id="page-title">Gestión de solicitudes</h1>
        </div>

        <p>
          La estructura base de la plataforma está lista. Los módulos funcionales se
          incorporarán de manera incremental.
        </p>
      </section>
    </main>
  )
}

export default App
