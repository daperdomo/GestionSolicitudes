import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { apiRequest } from '../../services/apiClient'
import type { CatalogAdministration, CatalogAdminItem } from '../../types/api'
import styles from '../../styles/ui.module.css'

interface CatalogSectionProps {
  title: string
  description: string
  items: CatalogAdminItem[]
  endpoint: string
  onChanged: () => Promise<void>
}

export function CatalogsPage() {
  const [catalogs, setCatalogs] = useState<CatalogAdministration | null>(null)
  const [error, setError] = useState('')

  const load = useCallback(async () => {
    try {
      setCatalogs(await apiRequest<CatalogAdministration>('/api/catalogos/administracion'))
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    }
  }, [])

  useEffect(() => { void load() }, [load])

  return (
    <section>
      <div className={styles.pageHeader}>
        <p>Administre los valores utilizados para clasificar y dirigir las solicitudes.</p>
      </div>
      {error && <div className={styles.error}>{error}</div>}
      {!catalogs ? <div className={styles.loading}>Cargando catálogos…</div> : (
        <div className={styles.gridTwo}>
          <CatalogSection
            title="Áreas"
            description="Unidades responsables disponibles en los formularios y filtros."
            items={catalogs.areas}
            endpoint="/api/catalogos/areas"
            onChanged={load}
          />
          <CatalogSection
            title="Tipos de solicitud"
            description="Clasificaciones funcionales disponibles para cada solicitud."
            items={catalogs.tiposSolicitud}
            endpoint="/api/catalogos/tipos-solicitud"
            onChanged={load}
          />
        </div>
      )}
    </section>
  )
}

function CatalogSection({ title, description, items, endpoint, onChanged }: CatalogSectionProps) {
  const [name, setName] = useState('')
  const [editing, setEditing] = useState<CatalogAdminItem | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError('')
    setSuccess('')
    try {
      await apiRequest(editing ? `${endpoint}/${editing.id}` : endpoint, {
        method: editing ? 'PUT' : 'POST',
        body: JSON.stringify(editing ? { nombre: name, activo: editing.activo } : { nombre: name }),
      })
      setSuccess(editing ? 'Registro actualizado.' : 'Registro agregado.')
      cancelEditing()
      await onChanged()
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setSaving(false)
    }
  }

  async function toggle(item: CatalogAdminItem) {
    setError('')
    setSuccess('')
    try {
      await apiRequest(`${endpoint}/${item.id}`, {
        method: 'PUT',
        body: JSON.stringify({ nombre: item.nombre, activo: !item.activo }),
      })
      setSuccess(item.activo ? 'Registro desactivado.' : 'Registro activado.')
      if (editing?.id === item.id) cancelEditing()
      await onChanged()
    } catch (reason) {
      setError((reason as Error).message)
    }
  }

  function beginEditing(item: CatalogAdminItem) {
    setEditing(item)
    setName(item.nombre)
    setError('')
    setSuccess('')
  }

  function cancelEditing() {
    setEditing(null)
    setName('')
  }

  return (
    <article className={styles.panel}>
      <div className={styles.catalogHeader}>
        <div><h2>{title}</h2><p>{description}</p></div>
        <span className={styles.catalogCount}>{items.length}</span>
      </div>
      {error && <div className={styles.error}>{error}</div>}
      {success && <div className={styles.success}>{success}</div>}
      <form className={styles.catalogForm} onSubmit={submit}>
        <label>{editing ? 'Editar nombre' : 'Nuevo registro'}
          <input value={name} maxLength={120} onChange={(event) => setName(event.target.value)} required />
        </label>
        <div className={styles.formActions}>
          <button className={styles.primaryButton} disabled={saving}>{saving ? 'Guardando…' : editing ? 'Guardar' : 'Agregar'}</button>
          {editing && <button className={styles.secondaryButton} type="button" onClick={cancelEditing}>Cancelar</button>}
        </div>
      </form>
      <div className={styles.catalogList}>
        {items.length === 0 ? <p>No hay registros.</p> : items.map((item) => (
          <div className={!item.activo ? styles.catalogItemInactive : undefined} key={item.id}>
            <div>
              <strong>{item.nombre}</strong>
              <span className={item.activo ? styles.statusActive : styles.statusInactive}>{item.activo ? 'Activo' : 'Inactivo'}</span>
            </div>
            <div className={styles.actions}>
              <button type="button" onClick={() => beginEditing(item)}>Editar</button>
              <button type="button" onClick={() => void toggle(item)}>{item.activo ? 'Desactivar' : 'Activar'}</button>
            </div>
          </div>
        ))}
      </div>
    </article>
  )
}
