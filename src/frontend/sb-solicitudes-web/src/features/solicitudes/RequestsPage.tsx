import { useCallback, useEffect, useState, type KeyboardEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { apiRequest } from '../../services/apiClient'
import type { CatalogItem, PagedResult, RequestListItem, UserOption } from '../../types/api'
import { formatDate } from '../../utils/format'
import { getRequestStatusLabel, requestStatusOptions } from '../../utils/requestStatus'
import styles from '../../styles/ui.module.css'

export function RequestsPage() {
  const navigate = useNavigate()
  const { session } = useAuth()
  const [page, setPage] = useState<PagedResult<RequestListItem> | null>(null)
  const [status, setStatus] = useState('')
  const [priority, setPriority] = useState('')
  const [areaId, setAreaId] = useState('')
  const [typeId, setTypeId] = useState('')
  const [assigneeId, setAssigneeId] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [sortBy, setSortBy] = useState('fechaCreacion')
  const [areas, setAreas] = useState<CatalogItem[]>([])
  const [types, setTypes] = useState<CatalogItem[]>([])
  const [users, setUsers] = useState<UserOption[]>([])
  const [pageNumber, setPageNumber] = useState(1)
  const [error, setError] = useState('')
  const [debouncedFilters, setDebouncedFilters] = useState({
    status: '',
    priority: '',
    areaId: '',
    typeId: '',
    assigneeId: '',
    dateFrom: '',
    dateTo: '',
    sortBy: 'fechaCreacion',
  })
  const canManage = session?.rol !== 'Solicitante'

  const load = useCallback(async () => {
    const query = new URLSearchParams({ pageNumber: String(pageNumber), pageSize: '20', sortBy: debouncedFilters.sortBy })
    if (debouncedFilters.status) query.set('estado', debouncedFilters.status)
    if (debouncedFilters.priority) query.set('prioridad', debouncedFilters.priority)
    if (debouncedFilters.areaId) query.set('areaId', debouncedFilters.areaId)
    if (debouncedFilters.typeId) query.set('tipoSolicitudId', debouncedFilters.typeId)
    if (debouncedFilters.assigneeId) query.set('responsableId', debouncedFilters.assigneeId)
    if (debouncedFilters.dateFrom) query.set('fechaDesde', new Date(`${debouncedFilters.dateFrom}T00:00:00`).toISOString())
    if (debouncedFilters.dateTo) query.set('fechaHasta', new Date(`${debouncedFilters.dateTo}T23:59:59`).toISOString())

    try {
      setError('')
      setPage(await apiRequest(`/api/solicitudes?${query}`))
    } catch (reason) {
      setError((reason as Error).message)
    }
  }, [debouncedFilters, pageNumber])

  useEffect(() => { void load() }, [load])
  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setPageNumber(1)
      setDebouncedFilters({ status, priority, areaId, typeId, assigneeId, dateFrom, dateTo, sortBy })
    }, 300)

    return () => window.clearTimeout(timeoutId)
  }, [areaId, assigneeId, dateFrom, dateTo, priority, sortBy, status, typeId])

  useEffect(() => {
    Promise.all([
      apiRequest<CatalogItem[]>('/api/catalogos/areas'),
      apiRequest<CatalogItem[]>('/api/catalogos/tipos-solicitud'),
    ]).then(([areaItems, typeItems]) => {
      setAreas(areaItems)
      setTypes(typeItems)
    }).catch(() => {
      setAreas([])
      setTypes([])
    })

    if (canManage) {
      apiRequest<UserOption[]>('/api/usuarios/asignables').then(setUsers).catch(() => setUsers([]))
    }
  }, [canManage])

  function openFromKeyboard(event: KeyboardEvent<HTMLTableRowElement>, id: number) {
    if (event.key === 'Enter' || event.key === ' ') navigate(`/solicitudes/${id}`)
  }

  return (
    <section>
      <div className={styles.pageHeader}><p>Consulta y seguimiento.</p><Link className={styles.primaryButton} to="/solicitudes/nueva">Nueva solicitud</Link></div>
      <div className={styles.filters}>
        <select value={status} onChange={(event) => setStatus(event.target.value)}><option value="">Todos los estados</option>{requestStatusOptions.map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select>
        <select value={priority} onChange={(event) => setPriority(event.target.value)}><option value="">Todas las prioridades</option>{['Baja', 'Media', 'Alta', 'Critica'].map((value) => <option key={value}>{value}</option>)}</select>
        <select value={areaId} onChange={(event) => setAreaId(event.target.value)}><option value="">Todas las áreas</option>{areas.map((area) => <option value={area.id} key={area.id}>{area.nombre}</option>)}</select>
        <select value={typeId} onChange={(event) => setTypeId(event.target.value)}><option value="">Todos los tipos</option>{types.map((type) => <option value={type.id} key={type.id}>{type.nombre}</option>)}</select>
        {canManage && <select value={assigneeId} onChange={(event) => setAssigneeId(event.target.value)}><option value="">Todos los responsables</option>{users.map((user) => <option value={user.id} key={user.id}>{user.nombre}</option>)}</select>}
        <input aria-label="Fecha inicial" type="date" value={dateFrom} onChange={(event) => setDateFrom(event.target.value)} />
        <input aria-label="Fecha final" type="date" value={dateTo} onChange={(event) => setDateTo(event.target.value)} />
        <select aria-label="Ordenar por" value={sortBy} onChange={(event) => setSortBy(event.target.value)}><option value="fechaCreacion">Fecha de creación</option><option value="fechaCompromiso">Fecha compromiso</option><option value="codigo">Código</option><option value="prioridad">Prioridad</option><option value="estado">Estado</option></select>
      </div>
      {error && <div className={styles.error}>{error}</div>}
      {!page ? <div className={styles.loading}>Cargando…</div> : <article className={styles.panel}>
        {page.items.length === 0 ? <p>No se encontraron solicitudes.</p> : <div className={styles.tableWrap}><table><thead><tr><th>Código</th><th>Título</th><th>Estado</th><th>Asignado a</th><th>Prioridad</th><th>Área</th><th>Solicitante</th><th>Creada</th><th>Compromiso</th></tr></thead><tbody>{page.items.map((item) => <tr className={styles.clickableRow} key={item.id} tabIndex={0} onClick={() => navigate(`/solicitudes/${item.id}`)} onKeyDown={(event) => openFromKeyboard(event, item.id)}><td><Link to={`/solicitudes/${item.id}`}>{item.codigo}</Link></td><td>{item.titulo}</td><td>{getRequestStatusLabel(item.estado)}</td><td>{item.responsable ?? 'Sin asignar'}</td><td>{item.prioridad}</td><td>{item.area}</td><td>{item.solicitante}</td><td>{formatDate(item.fechaCreacion)}</td><td>{formatDate(item.fechaCompromiso)}</td></tr>)}</tbody></table></div>}
        <div className={styles.pagination}><button disabled={pageNumber <= 1} onClick={() => setPageNumber((value) => value - 1)}>Anterior</button><span>Página {page.pageNumber} de {Math.max(page.totalPages, 1)}</span><button disabled={pageNumber >= page.totalPages} onClick={() => setPageNumber((value) => value + 1)}>Siguiente</button></div>
      </article>}
    </section>
  )
}
