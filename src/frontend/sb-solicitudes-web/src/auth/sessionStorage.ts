import type { LoginResponse } from '../types/api'

const SESSION_KEY = 'sb.solicitudes.session'

export function getSession(): LoginResponse | null {
  const value = sessionStorage.getItem(SESSION_KEY)
  if (!value) return null

  try {
    const session = JSON.parse(value) as LoginResponse
    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      clearSession()
      return null
    }
    return session
  } catch {
    clearSession()
    return null
  }
}

export function setSession(session: LoginResponse): void {
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(session))
}

export function clearSession(): void {
  sessionStorage.removeItem(SESSION_KEY)
}
