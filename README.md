# VCDA-Financial-Manager

Aplicación web de finanzas personales construida con **.NET 10** y **Blazor Server**. Permite gestionar cuentas, transacciones, categorías, presupuestos mensuales, reportes exportables e importación masiva desde CSV, con aislamiento de datos por usuario y panel de administración.

Repositorio objetivo de release `1.0`:

- [HebrineX/VCDA-Financial-Manager](https://github.com/HebrineX/VCDA-Financial-Manager.git)

## Stack

| Capa | Tecnología |
|------|------------|
| UI | Blazor Web App (Interactive Server) |
| Backend | ASP.NET Core .NET 10 |
| Identidad | ASP.NET Core Identity (roles Admin / User) |
| Base de datos | SQLite persistido en contenedor/local |
| Email | SMTP configurable |
| Logging | Serilog |
| Contenedores | Docker + docker-compose |
| Tests | xUnit |

## Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Opcional) [Docker Desktop](https://www.docker.com/products/docker-desktop/) para ejecutar en contenedor

## Setup local

```bash
git clone https://github.com/HebrineX/VCDA-Financial-Manager.git
cd VCDA-Financial-Manager
dotnet restore
dotnet ef database update --project VCDA.FinancialManager.Web
dotnet run --project VCDA.FinancialManager.Web
```

La aplicación queda disponible en `https://localhost:7xxx` (ver consola).

### Usuario administrador por defecto

Tras el primer arranque se crea automáticamente:

| Campo | Valor |
|-------|-------|
| Usuario | `HebrineX` |
| Email | `Biancolucasgerman@gmail.com` |
| Contraseña | `Admin123!` |
| Rol | Admin |

Los usuarios que se registren reciben el rol **User** y solo ven sus propios datos.

## Setup con Docker

```bash
copy .env.example .env
docker compose up --build
```

Abrir `https://localhost:8443` con Nginx local al frente del contenedor web. El puerto HTTP `8080` queda disponible solo para redirigir a HTTPS.

Los volúmenes locales persisten claves de DataProtection y el certificado X.509 en `docker-data/`.
La base SQLite queda persistida en `docker-data/app/`.

Para limpiar datos de prueba en el próximo arranque Docker, configura en `.env`:

```env
APP_RESET_DATABASE_ON_START=true
```

Luego ejecuta `docker compose up --build`. El contenedor borra y recrea la base SQLite, conservando certificados y secretos. Después del reset, vuelve a dejar esa variable en `false` para no borrar datos en cada arranque.

### SMTP, confirmación de email y recuperación de contraseña

Para que funcionen verificación de email y reset de contraseña, completa en `.env`:

```env
APP_PUBLIC_BASE_URL=https://localhost:8443
Smtp__Host=smtp.tu-proveedor.com
Smtp__Port=587
Smtp__EnableSsl=true
Smtp__FromEmail=tu-cuenta@dominio.com
Smtp__FromName=VCDA-Financial-Manager
Smtp__Username=tu-cuenta@dominio.com
Smtp__Password=tu-password-smtp
```

Si usas Gmail, normalmente necesitas contraseña de aplicación en lugar de tu password normal.

`APP_PUBLIC_BASE_URL` define la URL que aparece dentro de los emails de confirmación y recuperación. En local con Nginx debe ser `https://localhost:8443`; en un despliegue real debe apuntar al dominio público, por ejemplo `https://finanzas.tudominio.com`.

### Plan de imagen base 1.0 sin ACR

Nombre objetivo:

- Imagen versionada `vcda-financial-manager:1.0.0`.

Opciones de distribución sin ACR:

- Docker Hub público. Una cuenta gratuita alcanza para publicar esta imagen si no necesitás repositorio privado.
- GHCR si el repositorio queda alojado en GitHub y preferís publicar desde `HebrineX/VCDA-Financial-Manager`.
- Export manual con `docker save` para entrega cerrada.

Publicación sugerida en Docker Hub:

```bash
docker build -t hebrinex/vcda-financial-manager:1.0.0 .
docker login
docker push hebrinex/vcda-financial-manager:1.0.0
```

Si usás cuenta gratuita:

- Docker Hub gratis alcanza para publicar imágenes públicas.
- Si querés evitar límites o mantener todo junto al repo, `GHCR` es una alternativa muy cómoda para `HebrineX`.

Publicación sugerida en GHCR:

```bash
docker build -t ghcr.io/hebrinex/vcda-financial-manager:1.0.0 .
docker login ghcr.io
docker push ghcr.io/hebrinex/vcda-financial-manager:1.0.0
```

Checklist mínimo para cerrar la 1.0:

- Unificar el naming de imagen entre `Dockerfile`, `docker-compose.yml` y documentación.
- Sacar secretos hardcodeados del `docker-compose.yml` y pasarlos por variables o archivo externo no versionado.
- Documentar cómo generar o proveer `docker-data/certs/dataprotection.pfx` y su password.
- Definir persistencia explícita para SQLite dentro del contenedor.
- Validar login, dashboard, reportes, importación CSV y panel admin con la imagen versionada.
- Correr `dotnet build` y `dotnet test` antes de publicar.

### Nginx local con HTTPS

El proyecto ya incluye un servicio `nginx` como reverse proxy local para Blazor Server y WebSockets.

- `web` queda expuesto solo dentro de la red de Docker.
- `nginx` publica `https://localhost:8443`.
- `nginx` conserva `http://localhost:8080` solo para redirigir a HTTPS.
- TLS termina en Nginx y la app recibe `X-Forwarded-Proto=https`.

Para generar un certificado local autofirmado para Nginx:

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
$req.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new($false, $false, 0, $true))
$req.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
  [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor
  [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment,
  $true))

$oids = [System.Security.Cryptography.OidCollection]::new()
[void]$oids.Add([System.Security.Cryptography.Oid]::new('1.3.6.1.5.5.7.3.1'))
$req.CertificateExtensions.Add([System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new($oids, $true))

$cert = $req.CreateSelfSigned([DateTimeOffset]::Now.AddDays(-1), [DateTimeOffset]::Now.AddYears(2))
$certPem = [System.Security.Cryptography.PemEncoding]::WriteString(
  'CERTIFICATE',
  $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
$keyPem = [System.Security.Cryptography.PemEncoding]::WriteString('PRIVATE KEY', $rsa.ExportPkcs8PrivateKey())

Set-Content -LiteralPath (Join-Path $certDir 'nginx.crt') -Value $certPem -NoNewline -Encoding ascii
Set-Content -LiteralPath (Join-Path $certDir 'nginx.key') -Value $keyPem -NoNewline -Encoding ascii
```

El navegador puede mostrar advertencia por ser autofirmado. Para evitar advertencias en LAN o Internet, usá un certificado confiable de Let's Encrypt, Cloudflare Tunnel, ngrok o un reverse proxy con TLS válido.

### Exponer en red local o Internet

Para exponerlo dentro de tu red local, define en `.env` el puerto y la URL pública que usarán los emails:

```env
APP_HTTP_PORT=8080
APP_HTTPS_PORT=8443
APP_PUBLIC_BASE_URL=https://IP_DE_TU_PC:8443
APP_RESET_DATABASE_ON_START=false
```

Luego levantá:

```bash
docker compose up --build -d
```

En Windows, permití el puerto en el firewall:

```powershell
New-NetFirewallRule -DisplayName "VCDA Financial Manager HTTPS 8443" -Direction Inbound -Protocol TCP -LocalPort 8443 -Action Allow
```

Desde otro equipo de la misma red, abrí `https://IP_DE_TU_PC:8443`.

Para Internet, necesitás redirigir en el router el puerto externo `443` hacia `IP_DE_TU_PC:8443`, o usar un túnel/reverse proxy con TLS. Seteá siempre:

```env
APP_PUBLIC_BASE_URL=https://tu-dominio-o-tunel
```

Controles activos antes de exponer:

- confirmación de email obligatoria
- email único por usuario
- bloqueo temporal tras 5 intentos fallidos
- política de contraseña reforzada
- cookies `HttpOnly` y sesión expirable
- rate limiting global por IP
- headers `X-Frame-Options`, `X-Content-Type-Options`, `Referrer-Policy` y `Permissions-Policy`
- tamaño máximo de request en Nginx limitado a `5m`

## Funcionalidades principales

- **Dashboard**: patrimonio, ingresos/egresos del mes, movimientos recientes, distribución de gastos, barras de presupuesto y tendencia financiera histórica.
- **Gráficos SVG**: visuales de dashboard y reportes sin dependencias externas.
- **Cuentas y transacciones**: saldos actualizados de forma atómica; transacciones inmutables.
- **Presupuestos**: límite mensual por categoría de egreso.
- **Reportes**: filtros, paginación, totales y gráfico histórico de ingresos, egresos y balance de los últimos 6 meses.
- **Exportar CSV**: descarga sin dependencias externas (`/api/reportes/exportar-csv`).
- **Importar CSV**: vista previa con validación por fila e inserción atómica.
- **Admin**: listado de usuarios y activar/desactivar acceso (lockout).
- **Identity por email**: confirmación de cuenta, reenvío de confirmación y recuperación de contraseña vía SMTP.

### Formato CSV de importación

```csv
Fecha,Descripcion,Monto,Tipo,Categoria,Cuenta
2026-05-01,Supermercado,1500.50,Egreso,Comida,Efectivo
```

## Capturas de pantalla

> Añade aquí capturas del dashboard, login y reportes cuando despliegues la app (`docs/screenshots/`).

## Tests

```bash
dotnet test
```

## Estructura del repositorio

```
VCDA.FinancialManager.Domain/     # Entidades y enums
VCDA.FinancialManager.Web/        # Blazor, Identity, EF, servicios
VCDA.FinancialManager.Domain.Tests/
.hebrinex/                        # Specs y progreso del harness
docker-compose.yml
Dockerfile
nginx/
```

## Documentación de arquitectura

Ver [.hebrinex/orquestador/context/architecture.md](.hebrinex/orquestador/context/architecture.md).
