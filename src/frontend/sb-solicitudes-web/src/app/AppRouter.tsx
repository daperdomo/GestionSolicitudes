import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from '../auth/AuthContext'
import { LoginPage } from '../features/auth/LoginPage'
import { RegisterPage } from '../features/auth/RegisterPage'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { GovernmentEntitiesPage } from '../features/government-entities/GovernmentEntitiesPage'
import { CreateRequestPage } from '../features/solicitudes/CreateRequestPage'
import { RequestDetailPage } from '../features/solicitudes/RequestDetailPage'
import { RequestsPage } from '../features/solicitudes/RequestsPage'
import { UsersPage } from '../features/users/UsersPage'
import { AppLayout } from '../layouts/AppLayout'
import { AdministratorRoute, ProtectedRoute } from '../routes/ProtectedRoute'

export function AppRouter() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/registro" element={<RegisterPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route index element={<DashboardPage />} />
              <Route path="solicitudes" element={<RequestsPage />} />
              <Route path="solicitudes/nueva" element={<CreateRequestPage />} />
              <Route path="solicitudes/:id" element={<RequestDetailPage />} />
              <Route path="entidades-gubernamentales" element={<GovernmentEntitiesPage />} />
              <Route element={<AdministratorRoute />}>
                <Route path="usuarios" element={<UsersPage />} />
              </Route>
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
