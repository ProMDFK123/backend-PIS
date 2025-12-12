# Guía de Deploy a Render - BolsaFE UCN Backend

## Preparación Previa

### 1. Generar Claves Seguras

#### JWT Key (Mínimo 32 caracteres)
En tu terminal local:
```bash
# macOS/Linux
openssl rand -base64 32

# Windows (PowerShell)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

Copiar la salida y usarla para `Jwt__Key`.

### 2. Obtener Credenciales Necesarias

#### a) Resend (Email Service)
1. Ir a: https://resend.com/api-keys
2. Crear una API Key
3. Guardar el valor (comienza con `re_`)

#### b) Cloudinary (Image Storage)
1. Ir a: https://cloudinary.com/console
2. Copiar:
   - **Cloud Name** (en el dashboard)
   - **API Key** (en Settings > API Keys)
   - **API Secret** (en Settings > API Keys)

#### c) PostgreSQL Connection String
Si Render proporciona: `postgresql://user:password@host:port/database`

Convertir al formato ASP.NET:
```
Server=host;Port=5432;Database=database;Username=user;Password=password;SSL Mode=Require;Trust Server Certificate=true
```

### 3. Preparar Archivos Locales

```bash
cd bolsafeucn_back

# 1. Copiar appsettings.Example.json a appsettings.json
cp appsettings.Example.json appsettings.json

# 2. Copiar .env.example a .env (NUNCA subir a Git)
cp .env.example .env

# 3. Actualizar .env con tus credenciales reales
nano .env  # o abre en tu editor
```

## Configuración en Render

### Paso 1: Crear servicio Web en Render

1. Ir a: https://dashboard.render.com
2. Click en "New +" → "Web Service"
3. Conectar repositorio GitHub
4. Configurar:
   - **Name**: `bolsafeucn-api`
   - **Root Directory**: `bolsafeucn_back`
   - **Runtime**: `Docker`
   - **Plan**: `Free` (o `Starter` si necesitas más)
   - **Region**: `Oregon` o la más cercana
   - **Auto-deploy**: ✅ Activado

### Paso 2: Crear Base de Datos PostgreSQL

1. Click en "New +" → "PostgreSQL"
2. Configurar:
   - **Name**: `bolsafeucn-db`
   - **Database**: `bolsafeucn`
   - **User**: `bolsafeucn_user`
   - **Region**: Misma que el servicio web
   - **Plan**: `Free`
3. Una vez creada, copiar la **Internal Database URL**

### Paso 3: Agregar Variables de Entorno

En el dashboard del servicio web, ir a **Environment**:

#### Base de Datos (CONFIDENCIAL)
- **Key**: `ConnectionStrings__DefaultConnection`
- **Value**: (La URL que copiaste de PostgreSQL, convertida al formato ASP.NET)

Ejemplo convertido:
```
Server=dpg-xxxxx.oregon-postgres.render.com;Port=5432;Database=bolsafeucn;Username=bolsafeucn_user;Password=XXXXXXXX;SSL Mode=Require;Trust Server Certificate=true
```

#### JWT (CONFIDENCIAL)
- **Key**: `Jwt__Key`
- **Value**: (La que generaste con `openssl rand`)

#### Resend (CONFIDENCIAL)
- **Key**: `ResendApiKey`
- **Value**: `re_XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX`

#### Cloudinary (CONFIDENCIAL)
- **Key**: `Cloudinary__CloudName`
- **Value**: `tu-cloud-name`

- **Key**: `Cloudinary__ApiKey`
- **Value**: `123456789012345`

- **Key**: `Cloudinary__ApiSecret`
- **Value**: `XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX`

#### Entorno (Público)
- **Key**: `ASPNETCORE_ENVIRONMENT`
- **Value**: `Production`

#### Admin Email (Público)
- **Key**: `AdminNotifications__Email`
- **Value**: `tu-email@example.com`

#### URLs Permitidas (Público)
- **Key**: `AllowedOrigins__0`
- **Value**: `https://tu-frontend.onrender.com`

- **Key**: `AllowedOrigins__1`
- **Value**: `https://www.tudominio.cl` (si tienes dominio personalizado)

#### Storage (Público)
- **Key**: `Storage__Provider`
- **Value**: `cloudinary`

#### Imágenes por Defecto (Público)
- **Key**: `Images__DefaultUserImageUrl`
- **Value**: `https://res.cloudinary.com/TU_CLOUD_NAME/image/upload/v1/defaults/user-avatar.jpg`

- **Key**: `Images__DefaultUserImagePublicId`
- **Value**: `defaults/user-avatar`

### Paso 4: Build & Deploy

1. Hacer commit y push de cambios a GitHub:
```bash
git add appsettings.json appsettings.Example.json .env.example
git commit -m "chore: update configuration files for production"
git push origin dev
```

2. Render automáticamente iniciará el build cuando detecte el push
3. Monitorear en el tab "Logs" de Render
4. Si hay errores, verificar:
   - Connection string de base de datos
   - Variables de entorno completas
   - Dockerfile está en el directorio correcto

### Paso 5: Verificar Deploy

1. Una vez completado, Render proporciona una URL: `https://bolsafeucn-api.onrender.com`

2. Probar endpoints:
```bash
# Health check
curl https://bolsafeucn-api.onrender.com/api/health

# Swagger (si está activado)
https://bolsafeucn-api.onrender.com/swagger

# Test de login
curl -X POST https://bolsafeucn-api.onrender.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"estudiante@alumnos.ucn.cl","password":"Test123!"}'
```

## Configuración del Frontend

En tu aplicación Next.js, actualizar la variable de entorno:

```bash
# .env.local o .env.production
NEXT_PUBLIC_API_URL=https://bolsafeucn-api.onrender.com/api
```

O si usas Render para el frontend también:
```bash
NEXT_PUBLIC_API_URL=https://tu-frontend.onrender.com/api
```

## CORS Configuración

Si el frontend está en Render, necesitas:

1. Obtener la URL del frontend
2. Agregar en Environment Variables:
   - `AllowedOrigins__0=https://tu-frontend-url.onrender.com`
   - O actualizar en [Program.cs](../Program.cs) y redeploy

## Monitoreo & Logs

### Logs en Render
Dashboard → Servicio → "Logs" tab → Ver logs en tiempo real

### Health Check
Render monitorea automáticamente. Si la app no responde en 30s, la reinicia.

### Reset de Base de Datos
Si necesitas limpiar todo:
1. Dashboard → PostgreSQL → "Danger" tab
2. Click "Delete Database"
3. El servicio web se reiniciará y creará nueva BD con seed

## Troubleshooting

### Error: "Invalid JWT Key"
```
La clave no tiene 32+ caracteres. Regenerar con:
openssl rand -base64 32
```

### Error: "Could not connect to database"
```
1. Verificar Connection String está completo
2. Verificar SSL Mode = Require
3. Verificar Trust Server Certificate = true
```

### Error: "Resend API Key not configured"
```
Verificar que ResendApiKey está en Environment Variables
```

### App se reinicia constantemente
```
1. Revisar Logs en Render dashboard
2. Verificar que todas las variables requeridas existan
3. Verificar que Dockerfile existe en bolsafeucn_back/
```

## Notas Importantes

⚠️ **Plan Gratuito de Render:**
- Se suspende después de 15 minutos de inactividad
- PostgreSQL gratuita expira en 90 días
- Máximo 750 horas/mes

✅ **Pasos antes de Producción Real:**
- Considerar planes pagos
- Configurar backup automático de BD
- Implementar alertas de error
- Usar dominio personalizado (añadir en Render)
- Rotar claves de API regularmente

## Seguridad

- **NUNCA** subir `.env` a Git (debe estar en `.gitignore`)
- **NUNCA** compartir API keys públicamente
- Usar Render's native secret management para credenciales
- Rotar JWT Key cada 6 meses en producción
