import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../../auth/useAuth'
import { API_BASE_URL, apiRequest } from '../../services/apiClient'
import type { NotificationItem, UnreadNotificationCount } from '../../types/api'
import { formatDate } from '../../utils/format'
import styles from '../../styles/ui.module.css'

export function NotificationBell() {
  const { session } = useAuth()
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const [items, setItems] = useState<NotificationItem[]>([])
  const [unread, setUnread] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  const refreshCount = useCallback(async () => {
    const result = await apiRequest<UnreadNotificationCount>('/api/notificaciones/no-leidas/count')
    setUnread(result.total)
  }, [])

  const refreshList = useCallback(async () => {
    setLoading(true)
    try {
      setItems(await apiRequest<NotificationItem[]>('/api/notificaciones?limit=20'))
      setError('')
    } catch (reason) {
      setError((reason as Error).message)
    } finally {
      setLoading(false)
    }
  }, [])

  const refresh = useCallback(async () => {
    await Promise.all([refreshCount(), refreshList()])
  }, [refreshCount, refreshList])

  useEffect(() => {
    if (!session) return
    void refreshCount().catch(() => setUnread(0))
    const intervalId = window.setInterval(() => void refreshCount().catch(() => undefined), 60_000)
    return () => window.clearInterval(intervalId)
  }, [refreshCount, session])

  useEffect(() => {
    if (!session) return
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/notificaciones`, {
        accessTokenFactory: () => session.accessToken,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('notificationReceived', () => void refresh())
    void connection.start().catch(() => undefined)
    return () => { void connection.stop() }
  }, [refresh, session])

  async function toggle() {
    const nextOpen = !open
    setOpen(nextOpen)
    if (nextOpen) await refresh()
  }

  async function openNotification(item: NotificationItem) {
    if (!item.leida) {
      await apiRequest<void>(`/api/notificaciones/${item.id}/leida`, { method: 'PATCH' })
    }
    setOpen(false)
    await refreshCount()
    navigate(`/solicitudes/${item.solicitudId}`)
  }

  async function markAllAsRead() {
    await apiRequest<void>('/api/notificaciones/leidas', { method: 'PATCH' })
    setItems((current) => current.map((item) => ({ ...item, leida: true })))
    setUnread(0)
  }

  return (
    <div className={styles.notificationRoot}>
      <button
        className={styles.notificationButton}
        type="button"
        aria-label={`Notificaciones${unread > 0 ? `, ${unread} no leídas` : ''}`}
        aria-expanded={open}
        onClick={() => void toggle()}
      >
        <span aria-hidden="true">🔔</span>
        {unread > 0 && <span className={styles.notificationBadge}>{unread > 99 ? '99+' : unread}</span>}
      </button>

      {open && <section className={styles.notificationPanel} aria-label="Notificaciones">
        <header><strong>Notificaciones</strong>{unread > 0 && <button type="button" onClick={() => void markAllAsRead()}>Marcar todas como leídas</button>}</header>
        {loading && <p className={styles.notificationEmpty}>Cargando…</p>}
        {error && <p className={styles.notificationError}>{error}</p>}
        {!loading && !error && items.length === 0 && <p className={styles.notificationEmpty}>No tienes notificaciones.</p>}
        {!loading && items.map((item) => <button
          className={`${styles.notificationItem} ${item.leida ? '' : styles.notificationUnread}`}
          type="button"
          key={item.id}
          onClick={() => void openNotification(item)}
        >
          <span><strong>{item.asunto}</strong><small>{item.codigoSolicitud} · {formatDate(item.fechaCreacion)}</small></span>
          <span>{item.mensaje}</span>
        </button>)}
      </section>}
    </div>
  )
}
