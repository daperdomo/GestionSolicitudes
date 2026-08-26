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

**Por qué:** la estación actual solo tiene SDK 9 y 10, aunque posee runtimes .NET 8.

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
