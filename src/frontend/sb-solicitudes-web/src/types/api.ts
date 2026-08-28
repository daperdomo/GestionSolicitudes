export type UserRole = 'Administrador' | 'Analista' | 'Solicitante'
export type RequestStatus =
  | 'Registrada'
  | 'EnAnalisis'
  | 'EnProgreso'
  | 'EnEsperaSolicitante'
  | 'Resuelta'
  | 'Cerrada'
export type RequestPriority = 'Baja' | 'Media' | 'Alta' | 'Critica'

export interface LoginResponse {
  accessToken: string
  expiresAt: string
  usuarioId: string
  nombre: string
  correo: string
  rol: UserRole
}

export interface CatalogItem {
  id: number
  nombre: string
}

export interface CatalogAdminItem extends CatalogItem {
  activo: boolean
}

export interface CatalogAdministration {
  areas: CatalogAdminItem[]
  tiposSolicitud: CatalogAdminItem[]
}

export interface UserOption {
  id: string
  nombre: string
  correo: string
}

export interface NotificationItem {
  id: number
  solicitudId: number
  codigoSolicitud: string
  asunto: string
  mensaje: string
  fechaCreacion: string
  leida: boolean
  fechaLectura: string | null
}

export interface UnreadNotificationCount {
  total: number
}

export interface RequestListItem {
  id: number
  codigo: string
  titulo: string
  prioridad: RequestPriority
  estado: RequestStatus
  fechaCreacion: string
  fechaCompromiso: string | null
  area: string
  tipoSolicitud: string
  solicitante: string
  responsable: string | null
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface StateHistory {
  estadoAnterior: RequestStatus | null
  estadoNuevo: RequestStatus
  usuario: string
  fecha: string
  comentario: string | null
}

export interface RequestComment {
  id: number
  usuario: string
  texto: string
  visibilidad: 'Publico' | 'Interno'
  fecha: string
}

export interface RequestDetail extends RequestListItem {
  descripcion: string
  usuarioSolicitanteId: string
  responsableId: string | null
  areaId: number
  tipoSolicitudId: number
  evidenciaReferencia: string | null
  rowVersion: string
  historial: StateHistory[]
  comentarios: RequestComment[]
  actividad: RequestActivity[]
}

export interface RequestActivity {
  tipo: string
  usuario: string
  fecha: string
  descripcion: string
  valorAnterior: string | null
  valorNuevo: string | null
}

export interface MetricItem {
  nombre: string
  total: number
}

export interface DashboardSummary {
  solicitudesAbiertas: number
  solicitudesCerradas: number
  solicitudesVencidas: number
  porPrioridad: MetricItem[]
  porEstado: MetricItem[]
  ultimasSolicitudes: RequestListItem[]
}

export interface GovernmentEntity {
  id: number
  nombre: string
  categoria: string
  poderEstado: string
  sector: string
}

export type GovernmentEntityRequest = Omit<GovernmentEntity, 'id'>

export interface UserRecord {
  id: string
  nombre: string
  correo: string
  rol: UserRole
  activo: boolean
  fechaCreacion: string
}

export interface CreateUserRequest {
  nombre: string
  correo: string
  password: string
  rol: UserRole
}

export interface UpdateUserRequest extends CreateUserRequest {
  activo: boolean
}
