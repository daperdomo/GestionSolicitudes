import { expireSession, getSession } from '../auth/sessionStorage'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL
  ?? (import.meta.env.PROD ? '' : 'http://localhost:5080')

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(
    status: number,
    problem?: ProblemDetails,
  ) {
    super(problem?.detail ?? problem?.title ?? 'No fue posible completar la operación.')
    this.status = status
    this.problem = problem
  }
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const session = getSession()
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json')
  if (session) headers.set('Authorization', `Bearer ${session.accessToken}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })
  } catch {
    throw new ApiError(0, {
      title: 'API no disponible',
      detail: `No fue posible conectar con la API en ${API_BASE_URL}. Verifique que el backend esté ejecutándose.`,
    })
  }
  if (response.status === 401) expireSession()

  if (!response.ok) {
    const problem = await response.json().catch(() => undefined) as ProblemDetails | undefined
    throw new ApiError(response.status, problem)
  }

  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
