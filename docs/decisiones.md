# Decisiones técnicas

## Cinco proyectos backend

**Decisión:** separar Domain, Application, Infrastructure, Services y API.

**Por qué:** respeta las capas solicitadas y mantiene las reglas centrales aisladas.

**Alternativa:** integrar Services dentro de Infrastructure.

**Trade-off:** existe una referencia adicional, pero las integraciones técnicas quedan
separadas de la persistencia.

## Archivo JSON para entidades gubernamentales

**Decisión:** representar el Excel como JSON UTF-8 con un identificador entero estable.

**Por qué:** JSON es texto plano, conserva acentos y permite evolucionar el contrato sin
exponer el formato a Application.

**Alternativa:** CSV.

**Trade-off:** JSON ocupa más espacio, pero evita ambigüedades con comas y caracteres
especiales presentes en los nombres y sectores.

## SDK

**Decisión:** dirigir todos los proyectos a `net8.0` sin fijar un SDK global.

**Por qué:** un SDK posterior compatible puede compilar `net8.0` y un evaluador puede
usar directamente el SDK 8.

**Alternativa:** agregar un `global.json` para SDK 8.

**Trade-off:** no fijar el SDK mejora la portabilidad inmediata, pero exige verificar
que cada SDK utilizado pueda compilar proyectos dirigidos a .NET 8.

## Esquema relacional administrado por EF Core

**Decisión:** utilizar EF Core migrations como única fuente del esquema y aplicarlas al
iniciar la API.

**Por qué:** mantiene la base sincronizada con las entidades y configuraciones Fluent
API, y permite crear automáticamente una base inexistente.

**Alternativa:** mantener scripts `schema.sql` y `seed.sql` manuales.

**Trade-off:** el arranque necesita permisos para aplicar DDL. En ambientes donde esos
permisos estén restringidos, el mismo artefacto de migration deberá aplicarse como paso
controlado de despliegue antes de levantar la API.

## Autenticación sin ASP.NET Core Identity completo

**Decisión:** modelo `Usuario` propio, `PasswordHasher<Usuario>` y JWT Bearer.

**Por qué:** existen tres roles y un flujo de login acotado; Identity agregaría tablas,
tokens y abstracciones que la prueba no utiliza.

**Alternativa:** ASP.NET Core Identity.

**Trade-off:** la solución actual es más fácil de explicar y mantiene hashing seguro,
pero recuperación de contraseña, MFA o proveedores externos justificarían migrar a
Identity.

## Notificaciones sin eventos de dominio

**Decisión:** persistir la notificación desde el caso de uso y despacharla mediante
`INotificationDispatcher`.

**Por qué:** desacopla el canal sin introducir un bus interno para cuatro eventos.

**Alternativa:** domain events más handlers y patrón outbox.

**Trade-off:** hay menos infraestructura, pero el envío no tiene garantía transaccional
ante un proveedor externo. Un outbox sería apropiado al integrar mensajería real.

## Cliente HTTP con fetch nativo

**Decisión:** encapsular `fetch` en `apiClient`.

**Por qué:** centraliza base URL, JWT y Problem Details sin agregar Axios para un uso
que cubre la plataforma nativa.

**Alternativa:** Axios.

**Trade-off:** se reduce una dependencia; funciones avanzadas como interceptores o
cancelación coordinada requieren código propio si el cliente crece.

## Detalle de Solicitud estilo Work Item

**Decisión:** centralizar la gestión en `/solicitudes/{id}` con edición inline y endpoints `PATCH` por campo.

**Por qué:** Estado, Asignado a y los demás campos operativos evolucionan de forma independiente y deben conservar actividad auditable sin reenviar la entidad completa.

**Alternativa:** formularios separados o un `PUT` general.

**Trade-off:** existen más contratos pequeños, pero cada operación expresa mejor su autorización, validación y efecto. `RowVersion` protege cambios incompatibles; la asignación se mantiene compatible con cambios concurrentes de otros campos y siempre lee la versión vigente.

La matriz de estados centralizada es:

| Desde | Hacia |
|---|---|
| Registrada | En análisis |
| En análisis | En progreso, En espera del solicitante |
| En progreso | En espera del solicitante, Resuelta |
| En espera del solicitante | En análisis, En progreso |
| Resuelta | En progreso, Cerrada |
| Cerrada | En análisis, solo mediante reapertura con motivo |

Pasar a espera exige comentario público; cerrar exige comentario de resolución; reabrir exige motivo. Administrador y Analista pueden editar campos y transiciones. Solicitante puede consultar y comentar públicamente sus propias solicitudes, pero no gestionar campos aunque figure como responsable.

## MediatR y CQRS en la API

**Decisión:** los controllers envían Commands y Queries mediante `ISender`; sus handlers llaman a los servicios de Application.

**Por qué:** desacopla HTTP de la ejecución de casos de uso, establece un punto uniforme para comportamientos transversales futuros y evita inyectar múltiples servicios en cada controller.

**Alternativa:** invocar directamente servicios de Application desde los controllers.

**Trade-off:** se agregan mensajes y handlers pequeños. La lógica no se duplicó: los handlers actúan como adaptadores CQRS y los servicios existentes conservan las reglas ya probadas.

## Notificaciones internas, SignalR y futura mensajería

**Decisión:** persistir primero la notificación y actualizar la campana mediante un Hub SignalR autenticado. Entrega y lectura se modelan por separado.

**Por qué:** la interfaz funciona inmediatamente en una sola instancia y conserva historial aun cuando el cliente está desconectado.

**Alternativa:** introducir RabbitMQ desde el primer incremento.

**Trade-off:** el despacho actual es directo y no ofrece entrega distribuida garantizada. `INotificationDispatcher` usa un mensaje inmutable preparado para un productor RabbitMQ. Antes de habilitar el broker se incorporará Outbox, consumo idempotente, reintentos y dead-letter queue.
