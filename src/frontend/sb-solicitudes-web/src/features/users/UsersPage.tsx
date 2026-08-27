import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { apiRequest } from '../../services/apiClient'
import type { CreateUserRequest, UpdateUserRequest, UserRecord, UserRole } from '../../types/api'
import { formatDate } from '../../utils/format'
import styles from '../../styles/ui.module.css'

interface UserForm {
  nombre: string
  correo: string
  password: string
  rol: UserRole
  activo: boolean
}

const emptyForm: UserForm = {
  nombre: '',
  correo: '',
  password: '',
  rol: 'Solicitante',
  activo: true,
}

const roles: UserRole[] = ['Administrador', 'Analista', 'Solicitante']

export function UsersPage() {
  const [users, setUsers] = useState<UserRecord[]>([])
  const [form, setForm] = useState<UserForm>(emptyForm)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      setUsers(await apiRequest<UserRecord[]>('/api/usuarios'))
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { void load() }, [load])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError('')
    setSuccess('')

    const request: CreateUserRequest | UpdateUserRequest = editingId === null
      ? { nombre: form.nombre, correo: form.correo, password: form.password, rol: form.rol }
      : { ...form }

    try {
      await apiRequest(editingId === null ? '/api/usuarios' : `/api/usuarios/${editingId}`, {
        method: editingId === null ? 'POST' : 'PUT',
        body: JSON.stringify(request),
      })
      setSuccess(editingId === null ? 'Usuario registrado correctamente.' : 'Usuario actualizado correctamente.')
      cancelEditing()
      await load()
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setSaving(false)
    }
  }

  function edit(user: UserRecord) {
    setEditingId(user.id)
    setForm({
      nombre: user.nombre,
      correo: user.correo,
      password: '',
      rol: user.rol,
      activo: user.activo,
    })
    setError('')
    setSuccess('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function cancelEditing() {
    setEditingId(null)
    setForm(emptyForm)
  }

  return (
    <section>
      <div className={styles.pageHeader}>
        <p>Administre los miembros, sus roles y el acceso a la plataforma.</p>
      </div>

      {error && <div className={styles.error}>{error}</div>}
      {success && <div className={styles.success}>{success}</div>}

      <form className={`${styles.panel} ${styles.formGrid}`} onSubmit={submit}>
        <div className={styles.full}><h2>{editingId === null ? 'Registrar usuario' : 'Editar usuario'}</h2></div>
        <label>Nombre completo<input value={form.nombre} maxLength={150} onChange={(event) => setForm({ ...form, nombre: event.target.value })} required /></label>
        <label>Correo<input type="email" value={form.correo} maxLength={254} onChange={(event) => setForm({ ...form, correo: event.target.value })} required /></label>
        <label>Rol<select value={form.rol} onChange={(event) => setForm({ ...form, rol: event.target.value as UserRole })}>{roles.map((role) => <option key={role}>{role}</option>)}</select></label>
        <label>{editingId === null ? 'Contraseña inicial' : 'Nueva contraseña (opcional)'}<input type="password" autoComplete="new-password" value={form.password} minLength={10} onChange={(event) => setForm({ ...form, password: event.target.value })} required={editingId === null} /></label>
        {editingId !== null && <label className={styles.checkboxLabel}><input type="checkbox" checked={form.activo} onChange={(event) => setForm({ ...form, activo: event.target.checked })} /> Usuario activo</label>}
        <div className={`${styles.full} ${styles.formActions}`}>
          <button className={styles.primaryButton} disabled={saving}>{saving ? 'Guardando…' : editingId === null ? 'Registrar usuario' : 'Guardar cambios'}</button>
          {editingId !== null && <button type="button" className={styles.secondaryButton} onClick={cancelEditing}>Cancelar</button>}
        </div>
        <small className={styles.full}>La contraseña debe contener al menos 10 caracteres, mayúscula, minúscula, número y símbolo.</small>
      </form>

      <article className={styles.panel}>
        <h2>Usuarios registrados</h2>
        {loading ? <div className={styles.loading}>Cargando usuarios…</div> : users.length === 0 ? <p>No hay usuarios registrados.</p> : (
          <div className={styles.tableWrap}>
            <table><thead><tr><th>Nombre</th><th>Correo</th><th>Rol</th><th>Estado</th><th>Registro</th><th>Acciones</th></tr></thead><tbody>{users.map((user) => <tr key={user.id}><td>{user.nombre}</td><td>{user.correo}</td><td>{user.rol}</td><td><span className={user.activo ? styles.statusActive : styles.statusInactive}>{user.activo ? 'Activo' : 'Inactivo'}</span></td><td>{formatDate(user.fechaCreacion)}</td><td className={styles.actions}><button type="button" onClick={() => edit(user)}>Editar</button></td></tr>)}</tbody></table>
          </div>
        )}
      </article>
    </section>
  )
}
