# SB.Solicitudes

Base de una plataforma interna para administrar solicitudes de servicios tecnológicos
de la Superintendencia de Bancos de la República Dominicana.

## Estado actual

Este repositorio contiene únicamente el scaffold compilable. Todavía no implementa
entidades de dominio, persistencia, autenticación ni casos de uso productivos.

## Stack

- .NET 8 y ASP.NET Core Web API
- Onion Architecture compatible con Clean Architecture
- React 19, TypeScript y Vite
- xUnit para pruebas
- Serilog para logging estructurado
- Swagger/OpenAPI

## Requisitos locales

- SDK de .NET 8 o un SDK posterior capaz de compilar `net8.0`
- Node.js 22 o posterior
- npm

La máquina de desarrollo actual utiliza SDK .NET 10.0.204 para compilar proyectos
dirigidos a .NET 8. No se incluye `global.json` para no impedir el uso del SDK 8.

## Ejecución

```powershell
dotnet restore SB.Solicitudes.slnx
dotnet build SB.Solicitudes.slnx --no-restore
dotnet run --project src/backend/SB.Solicitudes.Api
```

En Development, Swagger estará disponible en `/swagger` y el diagnóstico mínimo en
`GET /health`.

```powershell
Set-Location src/frontend/sb-solicitudes-web
npm install
npm run dev
```

Para configurar el frontend, copie `.env.example` como `.env`. Las configuraciones
sensibles del backend deben establecerse mediante variables de entorno o User Secrets;
`appsettings.Example.json` contiene solamente marcadores.

## Verificación

```powershell
dotnet test SB.Solicitudes.slnx
Set-Location src/frontend/sb-solicitudes-web
npm run build
```

## Datos de entidades gubernamentales

La fuente suministrada contiene 181 entidades y cuatro columnas: `Nombre`,
`Categoría`, `Poder del Estado` y `Sector`. El scaffold contempla un JSON UTF-8 en
`App_Data` como persistencia de texto provisional. El repositorio con escritura
atómica y su CRUD se implementarán posteriormente.

## Base de datos relacional

SQL Server será administrado exclusivamente mediante EF Core migrations. No se
mantendrán scripts `schema.sql` o `seed.sql` paralelos al modelo.

- Las migrations se crearán durante el desarrollo después de definir las entidades y
  sus configuraciones Fluent API.
- Al iniciar la API se aplicarán automáticamente las migrations pendientes mediante
  `Database.MigrateAsync` y se creará la base si todavía no existe.
- Los datos iniciales se gestionarán desde Infrastructure con un inicializador
  idempotente, después de aplicar las migrations.

El `DbContext`, el inicializador y la primera migration pertenecen a la siguiente fase;
no se incluyen implementaciones vacías en este scaffold.

Consulte [docs/arquitectura.md](docs/arquitectura.md) y
[docs/decisiones.md](docs/decisiones.md) para las decisiones vigentes.
