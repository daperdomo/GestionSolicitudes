import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { useAuth } from '../../auth/useAuth'
import { apiRequest } from '../../services/apiClient'
import type { GovernmentEntity, GovernmentEntityRequest } from '../../types/api'
import styles from '../../styles/ui.module.css'

const emptyForm: GovernmentEntityRequest = {
  nombre: '',
  categoria: '',
  poderEstado: 'Poder Ejecutivo',
  sector: '',
}

export function GovernmentEntitiesPage() {
  const { session } = useAuth()
  const [items, setItems] = useState<GovernmentEntity[]>([])
  const [form, setForm] = useState<GovernmentEntityRequest>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [error, setError] = useState('')
  const isAdmin = session?.rol === 'Administrador'

  const load = useCallback(async () => {
    try {
      setItems(await apiRequest<GovernmentEntity[]>('/api/entidades-gubernamentales'))
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    }
  }, [])

  useEffect(() => { void load() }, [load])

  async function submit(event: FormEvent) {
    event.preventDefault()
    const path = editingId === null
      ? '/api/entidades-gubernamentales'
      : `/api/entidades-gubernamentales/${editingId}`
    try {
      await apiRequest(path, {
        method: editingId === null ? 'POST' : 'PUT',
        body: JSON.stringify(form),
      })
      setForm(emptyForm)
      setEditingId(null)
      await load()
    } catch (reason) {
      setError((reason as Error).message)
    }
  }

  async function remove(id: number) {
    if (!window.confirm('¿Desea eliminar esta entidad?')) return
    try {
      await apiRequest(`/api/entidades-gubernamentales/${id}`, { method: 'DELETE' })
      await load()
    } catch (reason) {
      setError((reason as Error).message)
    }
  }

  return (
    <section>
      <div className={styles.pageHeader}>
        <p>Catálogo institucional almacenado en JSON.</p>
      </div>
      {error && <div className={styles.error}>{error}</div>}
      {isAdmin && (
        <form className={`${styles.panel} ${styles.formGrid}`} onSubmit={submit}>
          <label>Nombre<input value={form.nombre} maxLength={107} onChange={(event) => setForm({ ...form, nombre: event.target.value })} required /></label>
          <label>Categoría<input value={form.categoria} maxLength={41} onChange={(event) => setForm({ ...form, categoria: event.target.value })} required /></label>
          <label>Poder del Estado<input value={form.poderEstado} maxLength={15} onChange={(event) => setForm({ ...form, poderEstado: event.target.value })} required /></label>
          <label>Sector<input value={form.sector} maxLength={40} onChange={(event) => setForm({ ...form, sector: event.target.value })} required /></label>
          <div className={styles.full}>
            <button className={styles.primaryButton}>{editingId === null ? 'Agregar' : 'Guardar cambios'}</button>
            {editingId !== null && <button type="button" className={styles.secondaryButton} onClick={() => { setEditingId(null); setForm(emptyForm) }}>Cancelar</button>}
          </div>
        </form>
      )}
      <article className={styles.panel}>
        {items.length === 0 ? <p>No hay entidades cargadas. El archivo fuente debe incorporarse para importar los 181 registros.</p> : (
          <div className={styles.tableWrap}><table><thead><tr><th>Nombre</th><th>Categoría</th><th>Poder</th><th>Sector</th>{isAdmin && <th>Acciones</th>}</tr></thead><tbody>{items.map((item) => <tr key={item.id}><td>{item.nombre}</td><td>{item.categoria}</td><td>{item.poderEstado}</td><td>{item.sector}</td>{isAdmin && <td className={styles.actions}><button onClick={() => { setEditingId(item.id); setForm(item) }}>Editar</button><button onClick={() => void remove(item.id)}>Eliminar</button></td>}</tr>)}</tbody></table></div>
        )}
      </article>
    </section>
  )
}
