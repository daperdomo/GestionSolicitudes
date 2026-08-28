import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from '../auth/AuthContext'
import { LoginPage } from '../features/auth/LoginPage'
import { DashboardPage } from '../features/dashboard/DashboardPage'
import { CatalogsPage } from '../features/catalogs/CatalogsPage'
import { GovernmentEntitiesPage } from '../features/government-entities/GovernmentEntitiesPage'
import { CreateRequestPage } from '../features/solicitudes/CreateRequestPage'
import { RequestDetailPage } from '../features/solicitudes/RequestDetailPage'
import { RequestsPage } from '../features/solicitudes/RequestsPage'
import { UsersPage } from '../features/users/UsersPage'
import { AppLayout } from '../layouts/AppLayout'
import { AdministratorRoute, ProtectedRoute, RequestCreatorRoute } from '../routes/ProtectedRoute'

export function AppRouter() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route index element={<DashboardPage />} />
              <Route path="solicitudes" element={<RequestsPage />} />
              <Route path="solicitudes/asignadas" element={<RequestsPage />} />
              <Route path="solicitudes/disponibles" element={<RequestsPage />} />
              <Route element={<RequestCreatorRoute />}>
                <Route path="solicitudes/nueva" element={<CreateRequestPage />} />
              </Route>
              <Route path="solicitudes/:id" element={<RequestDetailPage />} />
              <Route element={<AdministratorRoute />}>
                <Route path="catalogos" element={<CatalogsPage />} />
                <Route path="usuarios" element={<UsersPage />} />
                <Route path="entidades-gubernamentales" element={<GovernmentEntitiesPage />} />
              </Route>
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
