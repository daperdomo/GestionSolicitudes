import { useMemo, useState, type PropsWithChildren } from 'react'
import { apiRequest } from '../services/apiClient'
import type { LoginResponse } from '../types/api'
import { clearSession, getSession, setSession } from './sessionStorage'
import { AuthContext, type AuthContextValue } from './useAuth'

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, updateSession] = useState<LoginResponse | null>(() => getSession())

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
