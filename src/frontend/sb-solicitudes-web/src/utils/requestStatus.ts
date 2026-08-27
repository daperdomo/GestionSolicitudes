import type { RequestStatus } from '../types/api'

export const requestStatusLabels: Record<RequestStatus, string> = {
  Registrada: 'Registrada',
  EnAnalisis: 'En análisis',
  EnProgreso: 'En progreso',
  EnEsperaSolicitante: 'En espera del solicitante',
  Resuelta: 'Resuelta',
  Cerrada: 'Cerrada',
}

export const requestStatusOptions = Object.entries(requestStatusLabels) as [RequestStatus, string][]

export function getRequestStatusLabel(value: string | null): string {
  if (!value) return 'Sin valor'
  return value in requestStatusLabels
    ? requestStatusLabels[value as RequestStatus]
    : value
}
