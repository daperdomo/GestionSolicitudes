import { NavLink, Outlet, useLocation } from 'react-router-dom'
import homeIcon from '../assets/icons/home.svg'
import { NotificationBell } from '../features/notifications/NotificationBell'
import { UserMenu } from '../components/UserMenu'
import { useAuth } from '../auth/useAuth'
import styles from '../styles/ui.module.css'

export function AppLayout() {
  const { session } = useAuth()
  const location = useLocation()
  const pageTitle = getPageTitle(location.pathname)
  const isAdministrator = session?.rol === 'Administrador'
  const isAnalyst = session?.rol === 'Analista'

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
            <span className={styles.navGroupTitle}>{isAnalyst ? 'Gestión de solicitudes' : isAdministrator ? 'Solicitudes' : 'Mis solicitudes'}</span>
            <div className={styles.navSubmenu}>
              {isAnalyst ? <>
                <NavLink to="/solicitudes/asignadas">Asignadas a mí</NavLink>
                <NavLink to="/solicitudes/disponibles">Disponibles</NavLink>
              </> : <>
                <NavLink to="/solicitudes" end>{isAdministrator ? 'Todas las solicitudes' : 'Consultar'}</NavLink>
                <NavLink to="/solicitudes/nueva">Crear solicitud</NavLink>
              </>}
            </div>
          </div>
          {isAdministrator && <div className={styles.navGroup}>
            <span className={styles.navGroupTitle}>Administración</span>
            <div className={styles.navSubmenu}>
              <NavLink to="/catalogos">Catálogos</NavLink>
              <NavLink to="/usuarios">Usuarios</NavLink>
            </div>
          </div>}
        </nav>
      </aside>
      <div className={styles.contentArea}>
        <header className={styles.topbar}>
          <h1>{pageTitle}</h1>
          <div className={styles.userArea}>
            <NotificationBell />
            <UserMenu />
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
  if (pathname === '/solicitudes/asignadas') return 'Solicitudes asignadas a mí'
  if (pathname === '/solicitudes/disponibles') return 'Solicitudes disponibles'
  if (pathname === '/solicitudes/nueva') return 'Crear solicitud'
  if (pathname.startsWith('/solicitudes/')) return 'Detalle de solicitud'
  if (pathname === '/catalogos') return 'Administración de catálogos'
  if (pathname === '/entidades-gubernamentales') return 'Entidades gubernamentales'
  if (pathname === '/usuarios') return 'Registro de usuarios'
  return 'Gestión de solicitudes'
}
