import { useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import { apiRequest } from '../services/apiClient'
import type { LoginResponse } from '../types/api'
import { clearSession, expireSession, getSession, SESSION_EXPIRED_EVENT, SESSION_KEY, setSession } from './sessionStorage'
import { AuthContext, type AuthContextValue } from './useAuth'

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, updateSession] = useState<LoginResponse | null>(() => getSession())

  useEffect(() => {
    function handleSessionExpired() {
      updateSession(null)
    }

    function handleStorageChange(event: StorageEvent) {
      if (event.key === SESSION_KEY) updateSession(getSession())
    }

    window.addEventListener(SESSION_EXPIRED_EVENT, handleSessionExpired)
    window.addEventListener('storage', handleStorageChange)
    return () => {
      window.removeEventListener(SESSION_EXPIRED_EVENT, handleSessionExpired)
      window.removeEventListener('storage', handleStorageChange)
    }
  }, [])

  useEffect(() => {
    if (!session) return
    const expirationTime = new Date(session.expiresAt).getTime()
    const remainingTime = expirationTime - Date.now()
    if (!Number.isFinite(remainingTime) || remainingTime <= 0) {
      expireSession()
      return
    }

    const timeoutId = window.setTimeout(expireSession, remainingTime)
    return () => window.clearTimeout(timeoutId)
  }, [session])

  const value = useMemo<AuthContextValue>(() => ({
    session,
    login: async (correo, password) => {
      const response = await apiRequest<LoginResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ correo, password }),
      })
      setSession(response)
      updateSession(response)
    },
    logout: () => {
      clearSession()
      updateSession(null)
    },
  }), [session])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
