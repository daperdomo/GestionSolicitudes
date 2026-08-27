import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import homeIcon from '../assets/icons/home.svg'
import { useAuth } from '../auth/useAuth'
import { NotificationBell } from '../features/notifications/NotificationBell'
import styles from '../styles/ui.module.css'

export function AppLayout() {
  const { session, logout } = useAuth()
  const location = useLocation()
  const navigate = useNavigate()
  const pageTitle = getPageTitle(location.pathname)
  const isAdministrator = session?.rol === 'Administrador'

  return (
    <div className={styles.shell}>
      <aside className={styles.sidebar}>
        <div className={styles.brand}>
          <img src="/assets/branding/sb-logo.png" alt="Superintendencia de Bancos de la República Dominicana" />
          <small>Gestión de solicitudes</small>
        </div>
        <nav className={styles.nav} aria-label="Navegación principal">
          <NavLink to="/" end><img src={homeIcon} alt="" /> Inicio</NavLink>
          <div className={styles.navGroup}>
            <span className={styles.navGroupTitle}>Solicitudes</span>
            <div className={styles.navSubmenu}>
              <NavLink to="/solicitudes" end>Consultar</NavLink>
              <NavLink to="/solicitudes/nueva">Crear solicitud</NavLink>
            </div>
          </div>
          {isAdministrator ? <div className={styles.navGroup}>
            <span className={styles.navGroupTitle}>Administración</span>
            <div className={styles.navSubmenu}>
              <NavLink to="/usuarios">Usuarios</NavLink>
              <NavLink to="/entidades-gubernamentales">Entidades gubernamentales</NavLink>
            </div>
          </div> : <NavLink to="/entidades-gubernamentales">Entidades gubernamentales</NavLink>}
        </nav>
      </aside>
      <div className={styles.contentArea}>
        <header className={styles.topbar}>
          <h1>{pageTitle}</h1>
          <div className={styles.userArea}>
            <NotificationBell />
            <div><strong>{session?.nombre}</strong><small>{session?.rol}</small></div>
            <button className={styles.secondaryButton} onClick={() => { logout(); navigate('/login') }}>Cerrar sesión</button>
          </div>
        </header>
        <main className={styles.mainSurface}>
          <div className={styles.content}><Outlet /></div>
        </main>
      </div>
    </div>
  )
}

function getPageTitle(pathname: string): string {
  if (pathname === '/') return 'Inicio'
  if (pathname === '/solicitudes') return 'Consulta de solicitudes'
  if (pathname === '/solicitudes/nueva') return 'Crear solicitud'
  if (pathname.startsWith('/solicitudes/')) return 'Detalle de solicitud'
  if (pathname === '/entidades-gubernamentales') return 'Entidades gubernamentales'
  if (pathname === '/usuarios') return 'Registro de usuarios'
  return 'Gestión de solicitudes'
}
