import { useEffect, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { apiRequest } from '../../services/apiClient'
import type { CatalogItem, RequestDetail, UserOption } from '../../types/api'
import styles from '../../styles/ui.module.css'

export function CreateRequestPage() {
  const navigate = useNavigate()
  const { session } = useAuth()
  const [areas, setAreas] = useState<CatalogItem[]>([])
  const [types, setTypes] = useState<CatalogItem[]>([])
  const [assignableUsers, setAssignableUsers] = useState<UserOption[]>([])
  const [assigneeText, setAssigneeText] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    const requests: [Promise<CatalogItem[]>, Promise<CatalogItem[]>] = [
      apiRequest<CatalogItem[]>('/api/catalogos/areas'),
      apiRequest<CatalogItem[]>('/api/catalogos/tipos-solicitud'),
    ]
    Promise.all(requests).then(([areaItems, typeItems]) => { setAreas(areaItems); setTypes(typeItems) }).catch((reason: Error) => setError(reason.message))
    if (session?.rol !== 'Solicitante') apiRequest<UserOption[]>('/api/usuarios/asignables').then(setAssignableUsers).catch(() => setAssignableUsers([]))
  }, [session?.rol])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const data = new FormData(event.currentTarget)
    setLoading(true)
    setError('')
    try {
      const created = await apiRequest<RequestDetail>('/api/solicitudes', {
        method: 'POST',
        body: JSON.stringify({
          titulo: data.get('titulo'), descripcion: data.get('descripcion'), prioridad: data.get('prioridad'),
          fechaCompromiso: data.get('fechaCompromiso') || null, areaId: Number(data.get('areaId')),
          tipoSolicitudId: Number(data.get('tipoSolicitudId')), evidenciaReferencia: data.get('evidenciaReferencia') || null,
          responsableId: assignableUsers.find((user) => `${user.nombre} <${user.correo}>` === assigneeText)?.id ?? null,
        }),
      })
      navigate(`/solicitudes/${created.id}`)
    } catch (reason) { setError((reason as Error).message) } finally { setLoading(false) }
  }

  return (
    <section><div className={styles.pageHeader}><p>Registre la necesidad tecnológica con información suficiente.</p></div>
      <form className={`${styles.panel} ${styles.formGrid}`} onSubmit={submit}>
        {error && <div className={`${styles.error} ${styles.full}`}>{error}</div>}
        <label className={styles.full}>Título<input name="titulo" maxLength={200} required /></label>
        <label className={styles.full}>Descripción<textarea name="descripcion" maxLength={4000} rows={6} required /></label>
        <label>Prioridad<select name="prioridad" defaultValue="Media"><option>Baja</option><option>Media</option><option>Alta</option><option>Critica</option></select></label>
        <label>Fecha compromiso<input name="fechaCompromiso" type="datetime-local" /></label>
        <label>Área<select name="areaId" required defaultValue=""><option value="" disabled>Seleccione</option>{areas.map((item) => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></label>
        <label>Tipo de solicitud<select name="tipoSolicitudId" required defaultValue=""><option value="" disabled>Seleccione</option>{types.map((item) => <option key={item.id} value={item.id}>{item.nombre}</option>)}</select></label>
        {session?.rol !== 'Solicitante' && <label>Asignado a<input list="create-assignable-users" value={assigneeText} onChange={(event) => setAssigneeText(event.target.value)} placeholder="Sin asignar o buscar usuario" /><datalist id="create-assignable-users">{assignableUsers.map((user) => <option key={user.id} value={`${user.nombre} <${user.correo}>`} />)}</datalist></label>}
        <label className={styles.full}>Referencia de evidencia<input name="evidenciaReferencia" maxLength={1000} placeholder="URL o referencia textual" /></label>
        <div className={styles.full}><button className={styles.primaryButton} disabled={loading}>{loading ? 'Registrando…' : 'Registrar solicitud'}</button></div>
      </form>
    </section>
  )
}
