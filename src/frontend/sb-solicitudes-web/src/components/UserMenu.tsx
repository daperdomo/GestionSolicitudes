import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import styles from '../styles/ui.module.css'

export function UserMenu() {
  const { session, logout } = useAuth()
  const navigate = useNavigate()
  const rootRef = useRef<HTMLDivElement>(null)
  const [open, setOpen] = useState(false)
  const isAdministrator = session?.rol === 'Administrador'
  const initials = session?.nombre
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || 'U'

  useEffect(() => {
    function closeOnOutsideClick(event: PointerEvent) {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false)
    }
    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') setOpen(false)
    }
    document.addEventListener('pointerdown', closeOnOutsideClick)
    document.addEventListener('keydown', closeOnEscape)
    return () => {
      document.removeEventListener('pointerdown', closeOnOutsideClick)
      document.removeEventListener('keydown', closeOnEscape)
    }
  }, [])

  function signOut() {
    setOpen(false)
    logout()
    navigate('/login')
  }

  return (
    <div className={styles.userMenuRoot} ref={rootRef}>
      <button
        className={styles.userMenuButton}
        type="button"
        aria-label="Abrir menú de usuario"
        aria-haspopup="menu"
        aria-expanded={open}
        onClick={() => setOpen((current) => !current)}
      >
        <span className={styles.userIdentity}><strong>{session?.nombre}</strong><small>{session?.rol}</small></span>
        <span className={styles.avatar} aria-hidden="true">{initials}</span>
        <svg className={open ? styles.chevronOpen : ''} viewBox="0 0 20 20" aria-hidden="true"><path d="m5.5 7.5 4.5 4.5 4.5-4.5" /></svg>
      </button>

      {open && <div className={styles.userDropdown} role="menu">
        <div className={styles.userDropdownHeader}>
          <span className={styles.avatar} aria-hidden="true">{initials}</span>
          <span><strong>{session?.nombre}</strong><small>{session?.correo}</small><em>{session?.rol}</em></span>
        </div>
        {isAdministrator && <Link role="menuitem" to="/usuarios" onClick={() => setOpen(false)}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8ZM19 8v6M22 11h-6" /></svg>
          <span><strong>Administración</strong><small>Usuarios y permisos</small></span>
        </Link>}
        <div className={styles.userMenuDivider} />
        <button className={styles.logoutMenuItem} role="menuitem" type="button" onClick={signOut}>
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9" /></svg>
          <span><strong>Cerrar sesión</strong><small>Salir de la plataforma</small></span>
        </button>
      </div>}
    </div>
  )
}
