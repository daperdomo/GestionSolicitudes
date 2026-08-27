# SB.Solicitudes

Plataforma interna para registrar, asignar y dar seguimiento a solicitudes de servicios tecnológicos de la Superintendencia de Bancos de la República Dominicana.

## Funcionalidad implementada

- Login JWT con roles `Administrador`, `Analista` y `Solicitante`.
- Auto-registro de Solicitantes desde el login y registro administrativo con roles, activación/desactivación y contraseña inicial segura.
- Autorización por rol y por recurso: un solicitante solo consulta y comenta sus propias solicitudes.
- Creación, detalle tipo Work Item, filtros, ordenamiento y paginación server-side.
- Edición inline de estado, responsable, prioridad, área, tipo y fecha compromiso mediante endpoints `PATCH` específicos.
- Asignación/reasignación a cualquier usuario activo, comentarios públicos/internos, actividad e historiales inmutables.
- Matriz explícita de transiciones; el cierre exige comentario y solo Administrador/Analista puede reabrir.
- Concurrencia optimista mediante `RowVersion` para evitar sobrescrituras silenciosas.
- Campana de notificaciones persistentes con contador, listado, lectura individual/masiva y actualización en tiempo real mediante SignalR.
- Dashboard con abiertas, cerradas, vencidas, agrupaciones y últimas solicitudes.
- CRUD de entidades gubernamentales respaldado por JSON UTF-8 con escritura atómica.
- React responsive con rutas protegidas, estados de carga/error/vacío y cliente HTTP centralizado.

## Stack y arquitectura

- .NET 8, ASP.NET Core, MediatR/CQRS, EF Core 8, SQL Server, JWT, Swagger y Serilog.
- React 19, TypeScript, React Router, Vite y CSS Modules.
- xUnit y `WebApplicationFactory` para pruebas unitarias y de integración.
- Onion Architecture: `Domain <- Application <- Infrastructure/Services <- Api`.

La solución está en `SB.Solicitudes.slnx`. No se incluye `global.json`: puede utilizarse el SDK 8 o uno posterior compatible con `net8.0`.

## Requisitos

- .NET SDK 8 o posterior.
- SQL Server LocalDB para la configuración Development incluida, o una instancia SQL Server propia.
- Node.js 22+ y npm.

## Configuración y ejecución

Development usa `(localdb)\MSSQLLocalDB` y la base `SbSolicitudes`. Para otra instancia, configure `ConnectionStrings__DefaultConnection` mediante variable de entorno o User Secrets. No coloque secretos de producción en `appsettings`.

```powershell
dotnet restore SB.Solicitudes.slnx
dotnet run --project src/backend/SB.Solicitudes.Api --launch-profile http
```

Al arrancar en Development, la API ejecuta `Database.MigrateAsync()`: crea la base si no existe, aplica migrations pendientes y ejecuta seeds idempotentes. Swagger queda en `http://localhost:5080/swagger` y salud en `http://localhost:5080/health`.

Serilog escribe en consola y en `src/backend/SB.Solicitudes.Api/logs/sb-solicitudes-AAAA-MM-DD.log`. Los archivos rotan diariamente o al alcanzar 10 MB y se conservan los 30 más recientes; la carpeta está excluida de Git.

En otra terminal:

```powershell
Set-Location src/frontend/sb-solicitudes-web
npm install
npm run dev
```

El frontend usa `http://localhost:5080` por defecto. Puede copiar `.env.example` a `.env` para cambiar `VITE_API_BASE_URL`.

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@sb.local` | `Admin1234!` |
| Analista | `analista@sb.local` | `Analista1234!` |
| Solicitante | `solicitante@sb.local` | `Solicita1234!` |

Son credenciales exclusivamente locales. Las contraseñas se persisten con `PasswordHasher<TUser>`, nunca como texto plano.

## Migrations

EF Core es la única fuente del esquema; no existen `schema.sql` ni `seed.sql`.

```powershell
dotnet tool restore
dotnet ef migrations add NombreMigration `
  --project src/backend/SB.Solicitudes.Infrastructure `
  --startup-project src/backend/SB.Solicitudes.Api `
  --output-dir Migrations
```

En entornos con permisos DDL restringidos, las migrations deben aplicarse durante el despliegue y `Database__ApplyMigrationsOnStartup` debe permanecer en `false`.

## Verificación

```powershell
dotnet build SB.Solicitudes.slnx --no-restore
dotnet test SB.Solicitudes.slnx --no-build

Set-Location src/frontend/sb-solicitudes-web
npm run lint
npm run build
```

Las pruebas de integración crean una base LocalDB aislada con nombre aleatorio, ejecutan la migration/seed y la eliminan al terminar.

## Entidades gubernamentales

Application expone una abstracción e Infrastructure implementa `TextFileGovernmentEntityRepository`. El archivo está en `src/backend/SB.Solicitudes.Api/App_Data/entidades-gubernamentales.json`; las escrituras se serializan y reemplazan el archivo mediante un temporal para evitar contenido parcial.

El Excel original se conserva en `docs/fuentes/` y sus 181 filas se convirtieron a JSON UTF-8 respetando el orden, acentos y valores de las cuatro columnas. Los IDs estables 1–181 corresponden al orden original. El logo institucional y `home.svg` también se incorporaron desde los recursos suministrados.

## Supuestos y límites actuales

- “Vencida” significa `FechaCompromiso < UTC actual` y estado distinto de `Cerrada`.
- Las notificaciones se persisten y SignalR las entrega en tiempo real al grupo privado del destinatario. `INotificationDispatcher` recibe un mensaje inmutable y puede sustituirse por un productor RabbitMQ; para producción distribuida se recomienda Outbox.
- Se evitó Identity completo para mantener el alcance: modelo propio pequeño, hashing estándar y JWT son suficientes para la prueba.
- Los catálogos operativos se crean mediante seeds; su mantenimiento visual puede incorporarse en un siguiente incremento.
- Los PDF, instrucciones adicionales y maqueta mencionados en el requerimiento no estuvieron disponibles; la UI usa los colores y recursos institucionales suministrados.

Consulte [arquitectura](docs/arquitectura.md), [decisiones](docs/decisiones.md) y [evidencias](docs/evidencias.md).
