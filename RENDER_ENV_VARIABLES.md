# Variables de Entorno para Render

A continuación se muestran todas las variables de entorno que debes configurar en el **Dashboard de Render** en la sección **Environment Variables**.

## Formato en Render

Render usa el formato de variables de entorno estándar. Para estructuras anidadas (como las que en `appsettings.json` usan `:` ), Render usa `__` (doble guión bajo).

---

## Variables a Configurar en Render Dashboard

### 1. Database (PostgreSQL)
Render proporciona `DATABASE_URL` automáticamente, pero aquí está el formato esperado:

```
DATABASE_URL=postgresql://bolsafeucnuser:H0xxxxxxxxxxxxxx@dpg-d4tpql7pm1nc73cb52b0-a:5432/bolsafeucndb_pmrn
```

### 2. JWT (Autenticación)
**Generar nueva clave segura** (mínimo 32 caracteres):
```bash
openssl rand -base64 32
```

Variable a configurar:
```
Jwt__Key=EsteEsUnSecretoJWTMuyLargoYAleatorioParaDesarrolloQueDebeTenerAlMenos32Caracteres
```

### 3. Resend (Servicio de Emails)
Obtener API Key en: https://resend.com/api-keys

```
ResendApiKey=re_bpcLA8Vo_MfTC8uRjTeYCekwusvDFoFMg
```

### 4. Cloudinary (Almacenamiento de Imágenes)
Obtener credenciales en: https://cloudinary.com/console

```
Cloudinary__CloudName=Proyecto
Cloudinary__ApiKey=353517814928147
Cloudinary__ApiSecret=JP_bPwZ-TNg42CDM0PD87mEe8_o
```

### 5. Storage Provider
```
Storage__Provider=cloudinary
```
(Opciones: `cloudinary` o `local`)

### 6. Environment
```
ASPNETCORE_ENVIRONMENT=Production
```

### 7. Configuración Adicional (Opcional)
```
AdminNotifications__Email=admin@bolsafe.com
AllowedOrigins__0=https://frontend-bolsafe.onrender.com
AllowedOrigins__1=https://www.bolsafe.com
```

---

## Pasos en Render Dashboard

1. Ve a tu servicio en **Render.com**
2. Click en **Environment** (en el menu izquierdo)
3. Click en **Add Environment Variable**
4. Para cada variable de arriba:
   - **Key**: Nombre exacto de la variable (ej: `Jwt__Key`)
   - **Value**: El valor correspondiente
5. Click en **Save Changes**
6. El servicio se redesplegará automáticamente

---

## Formato de Conversión

| Formato appsettings.json | Formato Render (Environment) |
|--------------------------|------------------------------|
| `"Jwt": { "Key": "..." }` | `Jwt__Key=...` |
| `"Cloudinary": { "CloudName": "..." }` | `Cloudinary__CloudName=...` |
| `"Storage": { "Provider": "..." }` | `Storage__Provider=...` |

**Regla**: Reemplaza todos los `:` por `__` (doble guión bajo)

---

## Verificación

Para verificar que las variables se cargaron correctamente, revisa los logs de tu aplicación en Render. Deberías ver en los logs:

```
[STARTUP] ✓ ResendApiKey encontrada
[STARTUP] ✓ Clave JWT encontrada
[SEED-DB] ✓ Base de datos creada desde el modelo
```

Si alguna variable falta, verás un mensaje de advertencia como:
```
[STARTUP] ⚠️ ResendApiKey no configurada - emails no funcionarán
```

---

## Notas de Seguridad

- ✅ **NUNCA** compartas credenciales públicamente
- ✅ Genera una nueva `Jwt__Key` para producción (usa `openssl rand -base64 32`)
- ✅ Las variables sensibles deben estar SOLO en Render Dashboard, no en Git
- ✅ El archivo `.env` local es solo para desarrollo (nunca para producción)

