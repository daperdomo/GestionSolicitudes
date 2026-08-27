import { useCallback, useEffect, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { apiRequest } from '../../services/apiClient'
import type { CatalogItem, RequestDetail, RequestPriority, RequestStatus, UserOption } from '../../types/api'
import { formatDate } from '../../utils/format'
import styles from '../../styles/ui.module.css'

const allowedTransitions: Record<RequestStatus, RequestStatus[]> = {
  Registrada: ['EnAnalisis'],
  EnAnalisis: ['EnProgreso', 'EnEsperaSolicitante'],
  EnProgreso: ['EnEsperaSolicitante', 'Resuelta'],
  EnEsperaSolicitante: ['EnAnalisis', 'EnProgreso'],
  Resuelta: ['EnProgreso', 'Cerrada'],
  Cerrada: [],
}

const statusLabels: Record<RequestStatus, string> = {
  Registrada: 'Registrada',
  EnAnalisis: 'En análisis',
  EnProgreso: 'En progreso',
  EnEsperaSolicitante: 'En espera del solicitante',
  Resuelta: 'Resuelta',
  Cerrada: 'Cerrada',
}

const priorities: RequestPriority[] = ['Baja', 'Media', 'Alta', 'Critica']
type DetailTab = 'activity' | 'comments' | 'history'

export function RequestDetailPage() {
  const { id } = useParams()
  const { session } = useAuth()
  const [request, setRequest] = useState<RequestDetail | null>(null)
  const [areas, setAreas] = useState<CatalogItem[]>([])
  const [types, setTypes] = useState<CatalogItem[]>([])
  const [assignableUsers, setAssignableUsers] = useState<UserOption[]>([])
  const [assigneeText, setAssigneeText] = useState('')
  const [pendingStatus, setPendingStatus] = useState<RequestStatus | null>(null)
  const [statusComment, setStatusComment] = useState('')
  const [activeTab, setActiveTab] = useState<DetailTab>('activity')
  const [savingField, setSavingField] = useState<string | null>(null)
  const [feedback, setFeedback] = useState('')
  const [error, setError] = useState('')
  const canManage = session?.rol !== 'Solicitante'

  const load = useCallback(async () => {
    try {
      const detail = await apiRequest<RequestDetail>(`/api/solicitudes/${id}`)
      setRequest(detail)
      setAssigneeText(detail.responsable ?? '')
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    }
  }, [id])

  useEffect(() => {
    void load()
    if (canManage) {
      Promise.all([
        apiRequest<CatalogItem[]>('/api/catalogos/areas'),
        apiRequest<CatalogItem[]>('/api/catalogos/tipos-solicitud'),
        apiRequest<UserOption[]>('/api/usuarios/asignables'),
      ]).then(([areaItems, typeItems, userItems]) => {
        setAreas(areaItems)
        setTypes(typeItems)
        setAssignableUsers(userItems)
      }).catch((reason: Error) => setError(reason.message))
    }
  }, [canManage, load])

  async function patchField(path: string, body: Record<string, unknown>, field: string) {
    if (!request) return
    setSavingField(field)
    setFeedback('')
    setError('')
    try {
      const updated = await apiRequest<RequestDetail>(`/api/solicitudes/${id}/${path}`, {
        method: 'PATCH',
        body: JSON.stringify({ ...body, rowVersion: request.rowVersion }),
      })
      setRequest(updated)
      setAssigneeText(updated.responsable ?? '')
      setFeedback('Guardado')
    } catch (reason) {
      setError((reason as Error).message)
      await load()
    } finally {
      setSavingField(null)
    }
  }

  async function confirmStatus() {
    if (!pendingStatus) return
    await patchField('estado', { estado: pendingStatus, comentario: statusComment || null }, 'estado')
    setPendingStatus(null)
    setStatusComment('')
  }

  async function assign() {
    if (!request) return
    const selected = assignableUsers.find((user) => user.nombre === assigneeText || `${user.nombre} <${user.correo}>` === assigneeText)
    if (assigneeText && !selected) {
      setError('Seleccione un usuario activo de la lista.')
      return
    }
    await patchField('asignacion', { responsableId: selected?.id ?? null, comentario: null }, 'asignacion')
  }

  async function comment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    setSavingField('comentario')
    try {
      const updated = await apiRequest<RequestDetail>(`/api/solicitudes/${id}/comentarios`, {
        method: 'POST',
        body: JSON.stringify({ texto: data.get('texto'), visibilidad: data.get('visibilidad') }),
      })
      setRequest(updated)
      form.reset()
      setFeedback('Comentario agregado')
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setSavingField(null)
    }
  }

  if (error && !request) return <div className={styles.error}>{error}</div>
  if (!request) return <div className={styles.loading}>Cargando solicitud…</div>
  const commentRequired = pendingStatus === 'Cerrada'
    || pendingStatus === 'EnEsperaSolicitante'
    || request.estado === 'Cerrada'

  return (
    <section className={styles.workItem}>
      <Link className={styles.backLink} to="/solicitudes">← Solicitudes</Link>
      <header className={styles.workItemHeader}>
        <div><span>{request.codigo}</span><h2>{request.titulo}</h2></div>
        {feedback && <small className={styles.savedFeedback}>{feedback}</small>}
      </header>
      {error && <div className={styles.error}>{error}</div>}

      <div className={styles.workItemGrid}>
        <div className={styles.workItemMain}>
          <article className={styles.panel}>
            <h3>Descripción</h3>
            <p className={styles.description}>{request.descripcion}</p>
            {request.evidenciaReferencia && <p><strong>Evidencia: </strong>{request.evidenciaReferencia.startsWith('http') ? <a href={request.evidenciaReferencia} target="_blank" rel="noreferrer">Abrir enlace</a> : request.evidenciaReferencia}</p>}
          </article>

          <article className={styles.panel}>
            <div className={styles.tabs} role="tablist">
              <button className={activeTab === 'activity' ? styles.activeTab : ''} onClick={() => setActiveTab('activity')}>Actividad</button>
              <button className={activeTab === 'comments' ? styles.activeTab : ''} onClick={() => setActiveTab('comments')}>Comentarios ({request.comentarios.length})</button>
              <button className={activeTab === 'history' ? styles.activeTab : ''} onClick={() => setActiveTab('history')}>Historial</button>
            </div>

            {activeTab === 'activity' && <div>{request.actividad.map((item, index) => <div className={styles.activityItem} key={`${item.fecha}-${index}`}><span className={styles.activityDot} /><div><strong>{item.usuario}</strong> {item.descripcion.toLowerCase()}<small>{formatDate(item.fecha)}</small>{(item.valorAnterior || item.valorNuevo) && <p><del>{item.valorAnterior ?? 'Sin valor'}</del> → <strong>{item.valorNuevo ?? 'Sin valor'}</strong></p>}</div></div>)}</div>}
            {activeTab === 'comments' && <div><form className={styles.stack} onSubmit={comment}><textarea name="texto" required maxLength={2000} placeholder="Agregar comentario…" /><select name="visibilidad" defaultValue="Publico"><option value="Publico">Público</option>{canManage && <option value="Interno">Interno</option>}</select><button className={styles.primaryButton} disabled={savingField === 'comentario'}>Comentar</button></form>{request.comentarios.map((item) => <div className={styles.comment} key={item.id}><strong>{item.usuario}</strong><small>{item.visibilidad} · {formatDate(item.fecha)}</small><p>{item.texto}</p></div>)}</div>}
            {activeTab === 'history' && <div>{request.historial.map((item, index) => <div className={styles.timeline} key={`${item.fecha}-${index}`}><strong>{item.estadoAnterior ? statusLabels[item.estadoAnterior] : 'Inicio'} → {statusLabels[item.estadoNuevo]}</strong><small>{item.usuario} · {formatDate(item.fecha)}</small>{item.comentario && <p>{item.comentario}</p>}</div>)}</div>}
          </article>
        </div>

        <aside className={`${styles.panel} ${styles.workItemFields}`}>
          <h3>Información</h3>
          <label>Estado
            {canManage && request.estado !== 'Cerrada' ? <select value={pendingStatus ?? request.estado} onChange={(event) => { const value = event.target.value as RequestStatus; setPendingStatus(value === request.estado ? null : value) }}><option value={request.estado}>{statusLabels[request.estado]}</option>{allowedTransitions[request.estado].map((status) => <option value={status} key={status}>{statusLabels[status]}</option>)}</select> : <span className={styles.readonlyField}>{statusLabels[request.estado]}</span>}
          </label>
          {canManage && request.estado === 'Cerrada' && <button className={styles.secondaryButton} onClick={() => setPendingStatus('EnAnalisis')}>Reabrir solicitud</button>}
          {pendingStatus && <div className={styles.transitionBox}><strong>{statusLabels[request.estado]} → {statusLabels[pendingStatus]}</strong><textarea value={statusComment} onChange={(event) => setStatusComment(event.target.value)} required={commentRequired} placeholder={commentRequired ? 'Comentario obligatorio' : 'Comentario opcional'} /><div><button className={styles.primaryButton} disabled={savingField === 'estado' || (commentRequired && !statusComment.trim())} onClick={() => void confirmStatus()}>Confirmar</button><button className={styles.secondaryButton} onClick={() => { setPendingStatus(null); setStatusComment('') }}>Cancelar</button></div></div>}

          <label>Asignado a
            {canManage ? <><input list="assignable-users" value={assigneeText} onChange={(event) => setAssigneeText(event.target.value)} placeholder="Sin asignar o buscar usuario" /><datalist id="assignable-users">{assignableUsers.map((user) => <option key={user.id} value={`${user.nombre} <${user.correo}>`} />)}</datalist><button className={styles.fieldSaveButton} disabled={savingField === 'asignacion'} onClick={() => void assign()}>Aplicar</button></> : <span className={styles.readonlyField}>{request.responsable ?? 'Sin asignar'}</span>}
          </label>

          <label>Prioridad{canManage ? <select value={request.prioridad} disabled={savingField === 'prioridad'} onChange={(event) => void patchField('prioridad', { prioridad: event.target.value }, 'prioridad')}>{priorities.map((priority) => <option key={priority}>{priority}</option>)}</select> : <span className={styles.readonlyField}>{request.prioridad}</span>}</label>
          <label>Área{canManage ? <select value={request.areaId} disabled={savingField === 'area'} onChange={(event) => void patchField('area', { areaId: Number(event.target.value) }, 'area')}>{areas.map((area) => <option value={area.id} key={area.id}>{area.nombre}</option>)}</select> : <span className={styles.readonlyField}>{request.area}</span>}</label>
          <label>Tipo{canManage ? <select value={request.tipoSolicitudId} disabled={savingField === 'tipo'} onChange={(event) => void patchField('tipo', { tipoSolicitudId: Number(event.target.value) }, 'tipo')}>{types.map((type) => <option value={type.id} key={type.id}>{type.nombre}</option>)}</select> : <span className={styles.readonlyField}>{request.tipoSolicitud}</span>}</label>
          <label>Fecha compromiso{canManage ? <input type="date" value={request.fechaCompromiso?.slice(0, 10) ?? ''} disabled={savingField === 'fecha-compromiso'} onChange={(event) => void patchField('fecha-compromiso', { fechaCompromiso: event.target.value ? new Date(`${event.target.value}T12:00:00Z`).toISOString() : null }, 'fecha-compromiso')} /> : <span className={styles.readonlyField}>{formatDate(request.fechaCompromiso)}</span>}</label>
          <dl className={styles.compactDetails}><dt>Solicitante</dt><dd>{request.solicitante}</dd><dt>Creada</dt><dd>{formatDate(request.fechaCreacion)}</dd></dl>
        </aside>
      </div>
    </section>
  )
}
