# VCDA-Financial-Manager

VCDA-Financial-Manager es una aplicacion web de finanzas personales construida con Blazor Server y ASP.NET Core. Permite administrar cuentas, transacciones, categorias, presupuestos mensuales, reportes, exportacion/importacion CSV y usuarios con roles `Admin` y `User`.

La version vigente es `2.2.2`. El objetivo operativo de la linea `2.2.x` es entregar una app autocontenida, ejecutable localmente o con Docker, con SQLite persistido, SMTP configurable, HTTPS detras de Nginx, configuracion publica segura mediante `.env` y una linea de hardening/release hygiene mantenible.

La Fase 8 en curso se concentra en cuatro frentes: auditoria de dependencias e imagenes, hardening del origen productivo, observabilidad/log sanitizado y reproducibilidad/versionado de release.

En el estado actual de `2.2.2` tambien quedaron cerrados dos ajustes de UX operativa:

- `/cuentas` ya expone acciones de editar y eliminar sin superposicion de botones.
- `/transacciones` usa estilos de tabla y contraste consistentes entre modo claro y oscuro.

## Stack

| Capa | Tecnologia |
| --- | --- |
| UI | Blazor Web App, Interactive Server |
| Backend | ASP.NET Core / .NET |
| Identidad | ASP.NET Core Identity |
| Base de datos | SQLite |
| Email | SMTP configurable |
| Reverse proxy | Nginx |
| Contenedores | Docker + Docker Compose |
| Tests | xUnit |

## Fases del producto

- **Fase 1 - Base funcional**: dominio financiero, cuentas, categorias y transacciones.
- **Fase 2 - Presupuestos y reportes**: presupuestos mensuales, filtros, totales, graficos y CSV.
- **Fase 3 - Importacion**: carga masiva CSV con vista previa, validacion por fila e insercion atomica.
- **Fase 4 - Identity/Admin**: registro, login, roles, panel admin, bloqueo de usuarios y aislamiento por usuario.
- **Fase 5 - Docker/HTTPS/operacion**: Docker Compose, Nginx, HTTPS local, SMTP, SQLite persistido, reset controlado y DataProtection.
- **Fase 6 - Docs/config publica**: README autosuficiente y `.env.example` sin secretos reales.

## Prerrequisitos

- .NET SDK compatible con el proyecto.
- Docker Desktop, si vas a ejecutar la version contenerizada.
- PowerShell 7 o Windows PowerShell para los comandos de certificados en Windows.

## Ejecucion local sin Docker

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project VCDA.FinancialManager.Web
```

La consola indica la URL local de Kestrel. Para una experiencia equivalente a produccion, usa Docker Compose con Nginx y HTTPS.

## Ejecucion con Docker

1. Crea tu archivo de entorno local:

```powershell
Copy-Item .env.example .env
```

2. Edita `.env` y reemplaza todos los placeholders por valores propios. No guardes secretos reales en Git.

3. Genera o copia los certificados requeridos en `docker-data/certs/`.

4. Levanta la app:

```powershell
docker compose up --build
```

La app queda disponible en `https://localhost:8443`. El puerto `8080` publica HTTP solo para redirigir a HTTPS.

### Puertos y URL publica

Docker Compose usa estas variables:

```env
APP_HTTP_PORT=8080
APP_HTTPS_PORT=8443
APP_PUBLIC_BASE_URL=https://localhost:8443
```

`APP_PUBLIC_BASE_URL` es la URL absoluta que la app usa para links enviados por email, como confirmacion de cuenta y recuperacion de contrasena. En local con Nginx debe coincidir con el host y puerto HTTPS publicados. En produccion o tunel debe ser el dominio publico real, por ejemplo `https://finanzas.tudominio.com`.

Si expones la app en LAN, usa la IP o nombre DNS accesible por otros equipos:

```env
APP_PUBLIC_BASE_URL=https://192.168.1.50:8443
```

Si expones Internet, termina TLS con un certificado confiable y usa el dominio final:

```env
APP_PUBLIC_BASE_URL=https://finanzas.tudominio.com
```

## SMTP

Las funciones de confirmacion de email y recuperacion de contrasena dependen de SMTP. Completa estas variables en `.env`:

```env
Smtp__Host=smtp.tu-proveedor.com
Smtp__Port=587
Smtp__EnableSsl=true
Smtp__FromEmail=notificaciones@tudominio.com
Smtp__FromName=VCDA-Financial-Manager
Smtp__Username=notificaciones@tudominio.com
Smtp__Password=usa-un-secreto-local
```

Para Gmail u otros proveedores con MFA, normalmente necesitas una contrasena de aplicacion. No uses tu contrasena personal.

## HTTPS con Nginx

El servicio `nginx` actua como reverse proxy:

- Publica `https://localhost:8443` hacia el servicio `web`.
- Mantiene `http://localhost:8080` solo para redireccionar a HTTPS.
- Reenvia WebSockets para Blazor Server.
- Envia `X-Forwarded-Proto=https` y `X-Forwarded-Host`.
- Limita el tamano de request a `5m`.
- Agrega headers basicos de seguridad.

Nginx espera estos archivos:

```text
docker-data/certs/nginx.crt
docker-data/certs/nginx.key
```

Para desarrollo local puedes crear un certificado autofirmado:

```powershell
$certDir = Join-Path (Get-Location) 'docker-data\certs'
New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$rsa = [System.Security.Cryptography.RSA]::Create(2048)
$req = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new(
  'CN=localhost',
  $rsa,
  [System.Security.Cryptography.HashAlgorithmName]::SHA256,
  [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)

$san = [System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder]::new()
$san.AddDnsName('localhost')
$san.AddIpAddress([System.Net.IPAddress]::Parse('127.0.0.1'))
$san.AddIpAddress([System.Net.IPAddress]::Parse('::1'))
$req.CertificateExtensions.Add($san.Build())

$cert = $req.CreateSelfSigned([DateTimeOffset]::Now.AddDays(-1), [DateTimeOffset]::Now.AddYears(2))
$certPem = [System.Security.Cryptography.PemEncoding]::WriteString(
  'CERTIFICATE',
  $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
$keyPem = [System.Security.Cryptography.PemEncoding]::WriteString('PRIVATE KEY', $rsa.ExportPkcs8PrivateKey())

Set-Content -LiteralPath (Join-Path $certDir 'nginx.crt') -Value $certPem -NoNewline -Encoding ascii
Set-Content -LiteralPath (Join-Path $certDir 'nginx.key') -Value $keyPem -NoNewline -Encoding ascii
```

El navegador puede mostrar advertencia por ser autofirmado. Para uso real, usa Let's Encrypt, Cloudflare Tunnel, ngrok, un proxy corporativo o cualquier certificado emitido por una CA confiable.

## DataProtection PFX

ASP.NET Core DataProtection protege cookies, tokens de confirmacion y tokens de recuperacion. En Docker, las claves se persisten en:

```text
docker-data/dataprotection/
```

El proyecto tambien permite cifrar esas claves en reposo con un certificado X.509 PFX montado como:

```text
docker-data/certs/dataprotection.pfx
```

La contrasena del PFX se configura con:

```env
DATAPROTECTION_CERT_PASSWORD=usa-un-secreto-local-largo
```

Genera un PFX local para desarrollo:

```powershell
$certDir = Join-Path (Get-Location) 'docker-data\certs'
New-Item -ItemType Directory -Force -Path $certDir | Out-Null

$password = Read-Host 'Password para DATAPROTECTION_CERT_PASSWORD' -AsSecureString
$cert = New-SelfSignedCertificate `
  -Subject 'CN=VCDA DataProtection' `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -KeyExportPolicy Exportable `
  -CertStoreLocation 'Cert:\CurrentUser\My' `
  -NotAfter (Get-Date).AddYears(5)

Export-PfxCertificate `
  -Cert $cert `
  -FilePath (Join-Path $certDir 'dataprotection.pfx') `
  -Password $password
```

Usa el mismo valor escrito en `Read-Host` para `DATAPROTECTION_CERT_PASSWORD` dentro de `.env`. En produccion, rota y custodia este secreto fuera del repositorio.

## SQLite y backup

La base SQLite se persiste en:

```text
docker-data/app/
```

Antes de actualizar la imagen o hacer mantenimiento, deten la app y copia el directorio completo:

```powershell
docker compose down
Copy-Item -Recurse docker-data\app docker-data\backup-app-$(Get-Date -Format yyyyMMdd-HHmmss)
```

Para restaurar, deten los contenedores, reemplaza `docker-data/app/` por el backup y vuelve a levantar:

```powershell
docker compose up --build -d
```

## Produccion 443 con dominio y Let's Encrypt

Dominio objetivo:

```text
https://finanzas.vircomdelan.com.ar
```

La configuracion productiva usa:

- Solo puerto publico `443`.
- Nginx con TLS y HTTP/2.
- Certificado Let's Encrypt emitido por DNS-01 usando Cloudflare.
- App `web` interna en Docker, sin publicar `8080` a Internet.
- Politica de exposicion `443-only`: no se publica `80` y no hay redireccion `80->443` en produccion.
- El borde acepta trafico valido solo para `finanzas.vircomdelan.com.ar`; acceso por IP directa o `Host` invalido se rechaza en Nginx.

### DNS

En Cloudflare:

```text
Type: A
Name: finanzas
Content: 186.23.239.60
Proxy status: DNS only
TTL: Auto
```

Para emitir el certificado con Certbot DNS-01, el proxy naranja de Cloudflare no es necesario. Usar `DNS only` simplifica la validacion.

### Router

Abrir solo:

```text
TCP 443 externo -> IP_LOCAL_DE_LA_PC_DOCKER:443
```

No abrir `80`, `8080` ni `8443` para produccion.

### Variables `.env`

Agregar o ajustar:

```env
APP_PUBLIC_BASE_URL=https://finanzas.vircomdelan.com.ar
APP_RESET_DATABASE_ON_START=false
LETSENCRYPT_EMAIL=tu-email-real@example.com
```

Mantener tambien `DATAPROTECTION_CERT_PASSWORD`, SMTP y demas secretos ya configurados.

### Token Cloudflare

Crear un API Token en Cloudflare con permisos minimos:

```text
Zone.Zone: Read
Zone.DNS: Edit
Scope: vircomdelan.com.ar
```

Crear el archivo local:

```powershell
New-Item -ItemType Directory -Force -Path .\certbot | Out-Null
Copy-Item .\certbot\cloudflare.ini.example .\certbot\cloudflare.ini
```

Editar `certbot/cloudflare.ini` y reemplazar:

```text
dns_cloudflare_api_token = REEMPLAZAR_CON_TOKEN_CLOUDFLARE_DNS_EDIT
```

No subir `certbot/cloudflare.ini` al repositorio.

### Emitir certificado

Antes de levantar produccion por primera vez:

```powershell
docker compose -p vcda-prod -f docker-compose.prod.yml --profile certbot run --rm certbot
```

Esto crea certificados en:

```text
docker-data/letsencrypt/live/finanzas.vircomdelan.com.ar/
```

### Levantar produccion

Detener el stack local para no usar la misma base SQLite desde dos stacks:

```powershell
docker compose down
```

Levantar produccion:

```powershell
docker compose -p vcda-prod -f docker-compose.prod.yml up --build -d web nginx
```

`docker-compose.prod.yml` publica solo `443:443`. El servicio `web` queda interno en la red Docker mediante `expose: 8080` y no debe abrirse al host ni a Internet.

Validar:

```powershell
docker compose -p vcda-prod -f docker-compose.prod.yml ps
curl.exe -k https://localhost/health
```

Luego abrir:

```text
https://finanzas.vircomdelan.com.ar
```

El navegador no deberia mostrar advertencia de certificado si DNS, puerto 443 y Let's Encrypt estan correctos.

### Smoke publico F8-B

Checklist minimo a ejecutar contra `https://finanzas.vircomdelan.com.ar` antes de cerrar el slice:

1. `GET /Account/Login` responde `200`.
2. `GET /Account/ForgotPassword` responde `200`.
3. `GET /admin/usuarios` con sesion Admin valida responde `200`; sin sesion debe redirigir a login o rechazar segun Identity.
4. `GET /health` responde desde el origen previsto y no debe quedar abierto por IP publica fuera del borde esperado.
5. Headers de seguridad presentes en respuestas HTML: `strict-transport-security`, `x-content-type-options`, `x-frame-options`, `referrer-policy`, `permissions-policy`.
6. Acceso por IP directa o `Host` invalido debe ser rechazado por Nginx.

Este README documenta la politica y el checklist. La evidencia ejecutada de smoke debe registrarse aparte en `.hebrinex/orquestador/sdd/progress/cycles/`.

### Renovar certificado

Let's Encrypt vence cada 90 dias. Renovar manualmente con:

```powershell
docker compose -p vcda-prod -f docker-compose.prod.yml --profile certbot run --rm certbot renew `
  --dns-cloudflare `
  --dns-cloudflare-credentials /run/secrets/cloudflare.ini `
  --dns-cloudflare-propagation-seconds 60

docker compose -p vcda-prod -f docker-compose.prod.yml exec nginx nginx -s reload
```

Mas adelante se puede automatizar esta renovacion con una tarea programada de Windows.

## Reset controlado de base de datos

Para borrar y recrear la base en el proximo arranque Docker:

```env
APP_RESET_DATABASE_ON_START=true
```

Luego ejecuta:

```powershell
docker compose up --build
```

Vuelve a dejar `APP_RESET_DATABASE_ON_START=false` despues del reset. Si queda en `true`, cada arranque puede destruir datos.

## Admin seed seguro

El seed administrador se controla desde `.env`. Docker Compose mapea estas variables a `AdminSeed__...` dentro del contenedor. Para una DB nueva, habilitalo explicitamente con valores propios; para una DB existente o registro manual, dejalo apagado:

```env
ADMIN_SEED_ENABLED=true
ADMIN_SEED_USERNAME=admin-local
ADMIN_SEED_EMAIL=admin@example.local
ADMIN_SEED_PASSWORD=REEMPLAZAR_CON_PASSWORD_LARGO_UNICO
```

Notas de seguridad:

- Cambia siempre el password inicial antes de exponer la app.
- Usa un email controlado por el operador del despliegue.
- No reutilices passwords personales ni passwords de otros sistemas.
- Si el build aun conserva credenciales de fallback, tratalas como deuda critica y rota el admin apenas inicie.
- El archivo `.env` local no debe subirse al repositorio.

## Comandos de build y test

```powershell
dotnet restore
dotnet build
dotnet test
docker compose build
docker compose up -d
docker compose logs -f web
docker compose down
```

Para validar una imagen versionada:

```powershell
docker build -t vcda-financial-manager:2.2.2 .
docker run --rm vcda-financial-manager:2.2.2
```

En el flujo Compose, la imagen local esperada es `vcda-financial-manager:2.2.2`.

## Versionado y publicacion

### Fuente canonica de version

La fuente canonica de version para runtime y UI es `VCDA.FinancialManager.Web/VCDA.FinancialManager.Web.csproj`, usando:

- `Version`
- `InformationalVersion`

`AppVersionProvider` resuelve primero `App:Version` y, si no existe, usa `AssemblyInformationalVersion`. Por politica de release, Compose no debe sobreescribir `App__Version` salvo un caso excepcional y explicitamente documentado.

Los artefactos de despliegue deben seguir esa misma version:

- `docker-compose.yml` y `docker-compose.prod.yml` deben apuntar al tag `vcda-financial-manager:<version>`.
- `CHANGELOG.md` debe abrir una entrada para la version publicada.
- `README.md` solo debe mencionar la version vigente o ejemplos historicos claramente marcados.

### Politica SemVer

El proyecto usa `MAJOR.MINOR.PATCH`.

- `MAJOR`: cambios incompatibles de contrato, operacion o despliegue.
- `MINOR`: capacidades nuevas compatibles hacia atras.
- `PATCH`: correcciones, hardening, release hygiene y cambios operativos sin features nuevas.

Fase 8 se publica como trabajo de `PATCH` sobre la linea `2.2.x`, salvo decision humana distinta.

### Checklist manual de publicacion

1. Definir la version objetivo en `VCDA.FinancialManager.Web.csproj`.
2. Alinear el tag de imagen en `docker-compose.yml` y `docker-compose.prod.yml`.
3. Confirmar que no exista `App__Version` en Compose salvo excepcion aprobada.
4. Actualizar `CHANGELOG.md` con fecha, alcance y evidencia minima.
5. Revisar `README.md` para remover referencias obsoletas de version y comandos de imagen.
6. Ejecutar `dotnet build` y `dotnet test`.
7. Construir la imagen versionada que corresponda al release.
8. Publicar solo si docs, artefactos y version visible coinciden.

## CSV de importacion

Formato esperado:

```csv
Fecha,Descripcion,Monto,Tipo,Categoria,Cuenta
2026-05-01,Supermercado,1500.50,Egreso,Comida,Efectivo
```

## Gaps conocidos antes de produccion

- Confirmar que el seed admin configurable por `ADMIN_SEED_*` este definido solo cuando se necesite bootstrap inicial.
- Reemplazar cualquier credencial de fallback por configuracion externa obligatoria.
- Emitir y renovar certificados Let's Encrypt para `finanzas.vircomdelan.com.ar`.
- Guardar `.env`, PFX y backups fuera de Git y con permisos restringidos.
- Validar restore de backup SQLite antes de depender de el operacionalmente.
- Revisar limites de rate limiting, tamano de request y politicas de cookies para el dominio final.
- Ejecutar `dotnet build`, `dotnet test` y una prueba manual de login, SMTP, reportes, CSV y admin antes de publicar.

## Estructura principal

```text
VCDA.FinancialManager.Domain/          Entidades y enums
VCDA.FinancialManager.Web/             Blazor, Identity, EF, servicios
VCDA.FinancialManager.Domain.Tests/    Tests de dominio
docker-compose.yml                     Orquestacion local
docker-compose.prod.yml                Orquestacion productiva 443 + Let's Encrypt DNS-01
Dockerfile                             Imagen de la app
nginx/                                 Reverse proxy HTTPS
certbot/cloudflare.ini.example         Plantilla para token DNS Cloudflare
.env.example                           Plantilla publica sin secretos reales
```
