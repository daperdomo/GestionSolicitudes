import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { apiRequest } from '../../services/apiClient'
import type { DashboardSummary } from '../../types/api'
import { formatDate } from '../../utils/format'
import { getRequestStatusLabel } from '../../utils/requestStatus'
import { RequestStatusBadge } from '../../components/RequestStatusBadge'
import styles from '../../styles/ui.module.css'

export function DashboardPage() {
  const [data, setData] = useState<DashboardSummary | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    apiRequest<DashboardSummary>('/api/dashboard/resumen').then(setData).catch((reason: Error) => setError(reason.message))
  }, [])

  if (error) return <div className={styles.error}>{error}</div>
  if (!data) return <div className={styles.loading}>Cargando dashboard…</div>

  return (
    <section>
      <div className={styles.pageHeader}><p>Resumen operativo de solicitudes.</p></div>
      <div className={styles.metrics}>
        <article><span>Abiertas</span><strong>{data.solicitudesAbiertas}</strong></article>
        <article><span>Cerradas</span><strong>{data.solicitudesCerradas}</strong></article>
        <article><span>Vencidas</span><strong>{data.solicitudesVencidas}</strong></article>
      </div>
      <div className={styles.gridTwo}>
        <article className={styles.panel}><h2>Por estado</h2>{data.porEstado.map((item) => <div className={styles.metricRow} key={item.nombre}><span>{getRequestStatusLabel(item.nombre)}</span><strong>{item.total}</strong></div>)}</article>
        <article className={styles.panel}><h2>Por prioridad</h2>{data.porPrioridad.map((item) => <div className={styles.metricRow} key={item.nombre}><span>{item.nombre}</span><strong>{item.total}</strong></div>)}</article>
      </div>
      <article className={styles.panel}><h2>Últimas solicitudes</h2>{data.ultimasSolicitudes.length === 0 ? <p>No hay solicitudes registradas.</p> : <div className={styles.tableWrap}><table><thead><tr><th>Código</th><th>Título</th><th>Estado</th><th>Responsable</th><th>Fecha</th></tr></thead><tbody>{data.ultimasSolicitudes.map((item) => <tr key={item.id}><td><Link to={`/solicitudes/${item.id}`}>{item.codigo}</Link></td><td>{item.titulo}</td><td><RequestStatusBadge status={item.estado} /></td><td>{item.responsable ?? 'Sin asignar'}</td><td>{formatDate(item.fechaCreacion)}</td></tr>)}</tbody></table></div>}</article>
    </section>
  )
}
