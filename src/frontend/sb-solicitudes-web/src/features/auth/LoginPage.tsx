import { useState, type FormEvent } from 'react'
import { Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { ApiError } from '../../services/apiClient'
import styles from '../../styles/ui.module.css'

export function LoginPage() {
  const { session, login } = useAuth()
  const navigate = useNavigate()
  const [correo, setCorreo] = useState('solicitante@sb.local')
  const [password, setPassword] = useState('Solicita1234!')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  if (session) return <Navigate to="/" replace />

  async function submit(event: FormEvent) {
    event.preventDefault()
    setLoading(true)
    setError('')
    try {
      await login(correo, password)
      navigate('/')
    } catch (reason) {
      setError(reason instanceof ApiError ? reason.message : 'No fue posible iniciar sesión.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className={styles.loginPage}>
      <form className={styles.loginCard} onSubmit={submit}>
        <img className={styles.loginLogo} src="/assets/branding/sb-logo.png" alt="Superintendencia de Bancos de la República Dominicana" />
        <h1>Gestión de solicitudes</h1>
        <p>Superintendencia de Bancos de la República Dominicana</p>
        {error && <div className={styles.error}>{error}</div>}
        <label>Correo<input type="email" value={correo} onChange={(e) => setCorreo(e.target.value)} required /></label>
        <label>Contraseña<input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required /></label>
        <button className={styles.primaryButton} disabled={loading}>{loading ? 'Ingresando…' : 'Ingresar'}</button>
      </form>
    </main>
  )
}
