import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { apiRequest } from '../../services/apiClient'
import type { RegisteredUser } from '../../types/api'
import styles from '../../styles/ui.module.css'

export function RegisterPage() {
  const { session } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  if (session) return <Navigate to="/" replace />

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)
    const password = String(data.get('password') ?? '')
    const confirmation = String(data.get('confirmation') ?? '')

    if (password !== confirmation) {
      setError('Las contraseñas no coinciden.')
      return
    }

    setLoading(true)
    setError('')
    try {
      const user = await apiRequest<RegisteredUser>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          nombre: data.get('nombre'),
          correo: data.get('correo'),
          password,
        }),
      })
      navigate('/login', { replace: true, state: { registeredEmail: user.correo } })
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <main className={styles.loginPage}>
      <form className={styles.loginCard} onSubmit={submit}>
        <img className={styles.loginLogo} src="/assets/branding/sb-logo.png" alt="Superintendencia de Bancos de la República Dominicana" />
        <h1>Crear cuenta</h1>
        <p>Regístrese para crear y dar seguimiento a sus solicitudes.</p>
        {error && <div className={styles.error}>{error}</div>}
        <label>Nombre completo<input name="nombre" maxLength={150} autoComplete="name" required /></label>
        <label>Correo<input name="correo" type="email" maxLength={254} autoComplete="email" required /></label>
        <label>Contraseña<input name="password" type="password" minLength={10} autoComplete="new-password" required /></label>
        <label>Confirmar contraseña<input name="confirmation" type="password" minLength={10} autoComplete="new-password" required /></label>
        <small>Use al menos 10 caracteres, mayúscula, minúscula, número y símbolo.</small>
        <button className={styles.primaryButton} disabled={loading}>{loading ? 'Registrando…' : 'Crear cuenta'}</button>
        <Link className={styles.loginLink} to="/login">Ya tengo una cuenta</Link>
      </form>
    </main>
  )
}
