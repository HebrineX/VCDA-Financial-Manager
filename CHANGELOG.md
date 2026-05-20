# Changelog

Todos los cambios relevantes del proyecto se documentan en este archivo.

El formato sigue versiones semanticas: `MAJOR.MINOR.PATCH`.

## Unreleased

- Planificado: Fase 7 como `2.2.0`.

## 2.1.0 - 2026-05-19

- Agregado 2FA real con QR local para apps autenticadoras.
- 2FA recomendado para usuarios comunes y obligatorio para operar el panel Admin.
- Agregado nickname obligatorio como identificador visible agnostico del email.
- Login compatible con email o nickname.
- Panel Admin muestra nickname y permite desactivar/reactivar usuarios sin borrar datos financieros.
- Proteccion para no desactivar el ultimo administrador activo.
- Endurecido SMTP Identity con dedupe por proposito, validacion de remitente y logs con email enmascarado.
- Auditoria de dependencias ejecutada: sin paquetes vulnerables detectados.
- Imagen Docker actualizada a `vcda-financial-manager:2.1.0`.

## 2.0.0 - 2026-05-19

- Preparacion estructural para produccion con Docker y Nginx.
- Configuracion HTTPS con dominio externo `finanzas.vircomdelan.com.ar`.
- Certificados Let's Encrypt mediante DNS challenge de Cloudflare.
- Hardening inicial de Nginx y exposicion publica solo por puerto 443.
- Health endpoint operativo para verificacion.
- Documentacion publica actualizada para despliegue y operacion.

## 1.0.0 - 2026-05-19

- Branding consolidado como `VCDA-Financial-Manager`.
- Imagen Docker versionada `vcda-financial-manager:1.0.0`.
- Nginx local con HTTPS autofirmado.
- SMTP operativo para confirmacion de email y recuperacion de contrasena.
- Confirmacion de cuenta por email.
- Recuperacion de contrasena por email.
- UI base con dashboard, navegacion y marca visual.

## 0.5.0 - 2026-05-19

- Fase 5 completada: graficos avanzados, branding UI y estrategia Docker.
- Dashboard y reportes con visualizaciones SVG.
- Presupuestos con progreso visual.
- Documentacion para exposicion LAN/Internet y variables de entorno.
- Hardening base: email unico, lockout, password fuerte, rate limiting, cookies y headers de seguridad.

## 0.4.0 - 2026-05-19

- Fase 4 completada: multiusuario, presupuestos, reportes, importacion CSV y documentacion.
- Aislamiento multiusuario por `UserId`.
- Roles `Admin` y `User`.
- Panel admin de usuarios.
- Presupuestos por categoria, mes y anio.
- Reportes con filtros, paginacion, totales y grafico de seis meses.
- Exportacion CSV.
- Importacion CSV con vista previa e insercion atomica.
- Tests principales de dominio y servicios financieros.

## 0.3.0 - 2026-05-19

- Fase 3 completada: reportes financieros, dashboard y persistencia.
- Persistencia de datos financieros.
- Dashboard inicial de resumen financiero.
- Reportes base.

## 0.2.0 - 2026-05-18

- Fase 2 completada: modelos financieros y logica de negocio.
- Entidades base de cuentas, categorias y transacciones.
- Reglas iniciales de saldos y movimientos.

## 0.1.0 - 2026-05-18

- Fase 1 completada: setup inicial.
- Solucion .NET 10.
- Proyecto Blazor Server.
- Estructura inicial Docker.
