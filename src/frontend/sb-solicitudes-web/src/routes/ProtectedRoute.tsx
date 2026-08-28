import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function ProtectedRoute() {
  const { session } = useAuth()
  const location = useLocation()
  return session ? <Outlet /> : <Navigate to="/login" replace state={{ from: location }} />
}

export function AdministratorRoute() {
  const { session } = useAuth()
  return session?.rol === 'Administrador' ? <Outlet /> : <Navigate to="/" replace />
}

export function RequestCreatorRoute() {
  const { session } = useAuth()
  return session?.rol === 'Administrador' || session?.rol === 'Solicitante'
    ? <Outlet />
    : <Navigate to="/solicitudes/asignadas" replace />
}
