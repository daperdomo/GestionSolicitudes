import { createContext, useContext } from 'react'
import type { LoginResponse } from '../types/api'

export interface AuthContextValue {
  session: LoginResponse | null
  login: (correo: string, password: string) => Promise<void>
  logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth debe utilizarse dentro de AuthProvider.')
  return context
}
