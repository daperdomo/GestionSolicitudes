import type { RequestStatus } from '../types/api'
import { requestStatusLabels } from '../utils/requestStatus'
import styles from '../styles/ui.module.css'

const statusClasses: Record<RequestStatus, string> = {
  Registrada: styles.requestStatusRegistered,
  EnAnalisis: styles.requestStatusAnalysis,
  EnProgreso: styles.requestStatusProgress,
  EnEsperaSolicitante: styles.requestStatusWaiting,
  Resuelta: styles.requestStatusResolved,
  Cerrada: styles.requestStatusClosed,
}

interface RequestStatusBadgeProps {
  status: RequestStatus
}

export function RequestStatusBadge({ status }: RequestStatusBadgeProps) {
  return <span className={`${styles.requestStatusBadge} ${statusClasses[status]}`}>{requestStatusLabels[status]}</span>
}
