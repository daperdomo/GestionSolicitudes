# Evidencias de verificación

Verificación final ejecutada el 27 de agosto de 2026 en Windows, SDK .NET 10.0.204 compilando `net8.0`.

| Comando | Resultado |
|---|---|
| `dotnet restore SB.Solicitudes.slnx` | Restauración correcta |
| `dotnet build SB.Solicitudes.slnx -c Release --no-restore` | Correcto, 0 warnings, 0 errores |
| `dotnet test SB.Solicitudes.slnx -c Release --no-restore` | 9 unitarias + 6 integración aprobadas |
| `npm run lint` | Correcto, sin warnings |
| `npm run build` | Correcto, bundle Vite de producción |

Las pruebas unitarias cubren creación y reglas críticas de transición, espera, cierre y reapertura. La integración levanta la API, crea una LocalDB aislada, aplica migrations y seed, y valida salud, login, catálogos, creación, listado, transición con rol Analista, concurrencia optimista, asignación a cualquier usuario activo sin cambiar estado ni autorización, rechazo sin token, los 181 registros gubernamentales y el ciclo de alta/desactivación de usuarios.

Se verificó manualmente en `http://localhost:5080`:

- migration/seed idempotentes al reiniciar;
- `GET /health` = 200;
- login Administrador/Solicitante/Analista;
- creación, listado, detalle y transición de solicitud;
- dashboard contra SQL Server;
- alta, modificación y eliminación del catálogo gubernamental; el JSON volvió a `[]` después de la prueba.

La regla de dependencias se comprobó con `dotnet list <proyecto> reference`: Domain no referencia proyectos; Application solo Domain; Infrastructure y Services referencian Application/Domain; API referencia Application/Infrastructure/Services.

## Recursos fuente

Se incorporó `ListaEntidadesGubernamentales.xlsx` con SHA-256 `4A7467280A54DC61E169EDCB34C39932D52108846663756B3746E31DBA1ADDD8`. La conversión validó exactamente 181 registros, IDs únicos, cero campos requeridos vacíos, cero nombres duplicados y máximos 107/41/15/40.

También se verificaron y copiaron sin modificación:

- `home.svg`: SHA-256 `5289CFB02E693A5DEDB8A8403B5B5DF987B44DD2194CDDA495112F8D8794D11E`.
- Logo institucional: SHA-256 `2510E8E7DC4EF595DD8770D8BE1ED03AD507F4D9D2AD04D5813E0AF51C39FB94`.
