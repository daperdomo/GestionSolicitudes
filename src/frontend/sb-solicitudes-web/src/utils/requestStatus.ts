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

export function findRequestStatus(text: string): RequestStatus | null {
  return requestStatusOptions.find(([value, label]) => text.includes(value) || text.includes(label))?.[0] ?? null
}

export function formatRequestStatusText(text: string): string {
  return [...requestStatusOptions]
    .sort(([left], [right]) => right.length - left.length)
    .reduce((result, [value, label]) => result.replaceAll(value, label), text)
}
