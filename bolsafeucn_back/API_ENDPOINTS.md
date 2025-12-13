# 📚 Documentación Completa de API - BolsaFE UCN

> **Versión:** 1.0  
> **Framework:** ASP.NET Core 9.0  
> **Fecha:** Octubre 17, 2025  
> **Base URL:** `http://localhost:5185/api` o `https://your-domain.com/api`

---

## � Credenciales de Prueba Pre-configuradas

El sistema incluye **4 usuarios de prueba** con credenciales fáciles de recordar. Todos tienen el email confirmado y están listos para usar:

| Rol | Email | Contraseña | Descripción |
|-----|-------|------------|-------------|
| 👨‍🎓 **Estudiante** | `estudiante@alumnos.ucn.cl` | `Test123!` | Puede postular, ver detalles completos de ofertas |
| 🏢 **Empresa** | `empresa@techcorp.cl` | `Test123!` | Puede crear ofertas, ver postulaciones |
| 👤 **Particular** | `particular@ucn.cl` | `Test123!` | Puede crear ofertas, ver postulaciones |
| 👑 **Admin** | `admin@ucn.cl` | `Test123!` | Administrador del sistema |

### 🚀 Inicio Rápido

**1. Inicia la aplicación:**
```bash
dotnet run
```

**2. Verifica en los logs que aparezcan las credenciales:**
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 CREDENCIALES DE PRUEBA:
👨‍🎓 ESTUDIANTE: estudiante@alumnos.ucn.cl / Test123!
🏢 EMPRESA: empresa@techcorp.cl / Test123!
👤 PARTICULAR: particular@ucn.cl / Test123!
👑 ADMIN: admin@ucn.cl / Test123!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

**3. Usa estas credenciales en Postman:**
```http
POST http://localhost:5185/api/auth/login
Content-Type: application/json

{
  "email": "estudiante@alumnos.ucn.cl",
  "password": "Test123!",
  "rememberMe": true
}

→ Copiar el token JWT de la respuesta
```

**4. Configura el token en Postman:**
- Tab **Authorization** → Type: **Bearer Token**
- Pegar el token copiado
- ✅ Listo para probar endpoints protegidos

### ⚡ Prueba Rápida de Seguridad

```http
# 1. Login como Estudiante
POST /api/auth/login
Body: {"email": "estudiante@alumnos.ucn.cl", "password": "Test123!"}
→ Guardar token como {{student_token}}

# 2. Ver detalles COMPLETOS de oferta (como estudiante)
GET /api/publications/offers/1
Authorization: Bearer {{student_token}}
→ ✅ Ves: description, remuneration, requirements, contactInfo

# 3. Login como Empresa
POST /api/auth/login
Body: {"email": "empresa@techcorp.cl", "password": "Test123!"}
→ Guardar token como {{company_token}}

# 4. Ver detalles BÁSICOS de oferta (como empresa)
GET /api/publications/offers/1
Authorization: Bearer {{company_token}}
→ ⚠️ Solo ves: title, company, location, dates (SIN datos sensibles)

# 5. Intentar postular como empresa (DEBE FALLAR)
POST /api/publications/offers/1/apply
Authorization: Bearer {{company_token}}
Body: {"motivationLetter": "..."}
→ ❌ 403 Forbidden: "Solo los estudiantes pueden postular"

# 6. Postular como estudiante (EXITOSO)
POST /api/publications/offers/1/apply
Authorization: Bearer {{student_token}}
Body: {"motivationLetter": "Me interesa porque..."}
→ ✅ 200 OK: Postulación creada
```

### 📝 Notas Importantes

- ✅ **Todos los usuarios tienen email confirmado** - No necesitas verificar email
- ✅ **Password universal** - Todos usan `Test123!` para facilitar testing
- ✅ **Se crean automáticamente** - Al iniciar la app por primera vez
- ⚠️ **Base de datos limpia** - Si eliminas la BD, se recrean automáticamente

Para información completa sobre permisos y testing, ver [`TEST_CREDENTIALS.md`](./TEST_CREDENTIALS.md).

---

## �📑 Tabla de Contenidos

1. [🔐 Autenticación (AuthController)](#-autenticación-authcontroller)
2. [📰 Publicaciones (PublicationController)](#-publicaciones-publicationcontroller)
3. [📝 Postulaciones (JobApplicationController)](#-postulaciones-jobapplicationcontroller)
4. [🔒 Seguridad y JWT](#-seguridad-y-jwt)
5. [📊 Códigos de Estado HTTP](#-códigos-de-estado-http)
6. [🧪 Colección de Postman](#-colección-de-postman)

---

## 🔐 Autenticación (AuthController)

**Base URL:** `/api/auth`

Todos los endpoints de autenticación y registro de usuarios.

### 1. Registrar Estudiante

```http
POST /api/auth/register/student
Content-Type: application/json

Body:
{
  "email": "juan.perez@alumnos.ucn.cl",
  "userName": "juanperez",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "rut": "12.345.678-9",
  "name": "Juan",
  "lastName": "Pérez",
  "phone": "+56912345678",
  "career": "Ingeniería Civil en Computación"
}

Response 200 OK:
{
  "message": "Estudiante registrado exitosamente. Por favor verifica tu email."
}

Response 400 Bad Request:
{
  "message": "El email ya está registrado"
}
```

**Validaciones:**
- ✅ Email debe ser `@alumnos.ucn.cl`
- ✅ Password mínimo 8 caracteres, 1 mayúscula, 1 minúscula, 1 número
- ✅ Password y ConfirmPassword deben coincidir
- ✅ RUT válido con formato chileno

---

### 2. Registrar Particular/Individual

```http
POST /api/auth/register/individual
Content-Type: application/json

Body:
{
  "email": "maria.gonzalez@gmail.com",
  "userName": "mariaglez",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "rut": "15.678.234-5",
  "name": "María",
  "lastName": "González",
  "phone": "+56987654321"
}

Response 200 OK:
{
  "message": "Particular registrado exitosamente. Por favor verifica tu email."
}
```

**Validaciones:**
- ✅ Email válido (cualquier dominio)
- ✅ Password mínimo 8 caracteres
- ✅ RUT válido

---

### 3. Registrar Empresa

```http
POST /api/auth/register/company
Content-Type: application/json

Body:
{
  "email": "rrhh@techcorp.cl",
  "userName": "techcorp",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "rut": "76.123.456-7",
  "companyName": "TechCorp SpA",
  "businessSector": "Tecnología",
  "phone": "+56222334455",
  "address": "Av. Angamos 0610, Antofagasta"
}

Response 200 OK:
{
  "message": "Empresa registrada exitosamente. Por favor verifica tu email."
}
```

**Validaciones:**
- ✅ Email válido
- ✅ RUT de empresa (formato chileno)
- ✅ Nombre de empresa requerido

---

### 4. Registrar Administrador

```http
POST /api/auth/register/admin
Content-Type: application/json

Body:
{
  "email": "admin@bolsafe.ucn.cl",
  "userName": "adminucn",
  "password": "SuperSecure123!",
  "confirmPassword": "SuperSecure123!",
  "rut": "18.234.567-8"
}

Response 200 OK:
{
  "message": "Administrador registrado exitosamente. Por favor verifica tu email."
}
```

**⚠️ Nota:** Este endpoint debería estar protegido en producción.

---

### 5. Verificar Email

```http
POST /api/auth/verify-email
Content-Type: application/json

Body:
{
  "email": "juan.perez@alumnos.ucn.cl",
  "verificationCode": "123456"
}

Response 200 OK:
{
  "message": "Email verificado exitosamente. Ya puedes iniciar sesión."
}

Response 400 Bad Request:
{
  "message": "Código de verificación inválido o expirado"
}
```

**Validaciones:**
- ✅ Código de 6 dígitos
- ✅ Código válido y no expirado (15 minutos)
- ✅ Email debe existir y no estar verificado

---

### 6. Iniciar Sesión (Login)

```http
POST /api/auth/login
Content-Type: application/json

Body:
{
  "email": "juan.perez@alumnos.ucn.cl",
  "password": "Password123!"
}

Response 200 OK:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6Ikp1YW4gUGVyZXoiLCJpYXQiOjE1MTYyMzkwMjJ9.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
}

Response 401 Unauthorized:
{
  "message": "Email o contraseña incorrectos"
}

Response 403 Forbidden:
{
  "message": "Debes verificar tu email antes de iniciar sesión"
}
```

**Validaciones:**
- ✅ Email verificado
- ✅ Usuario no baneado
- ✅ Credenciales correctas

**JWT Token incluye:**
```json
{
  "sub": "123",                          // User ID
  "email": "juan.perez@alumnos.ucn.cl",
  "nameid": "123",                       // ClaimTypes.NameIdentifier
  "role": "Student",                      // User role
  "iat": 1697558400,                     // Issued at
  "exp": 1697644800                      // Expiration (24 hours)
}
```

---

## 📰 Publicaciones (PublicationController)

**Base URL:** `/api/publications`

Todos los endpoints relacionados con publicaciones (ofertas laborales, voluntariados y compra/venta).

---

## � POLÍTICAS DE SEGURIDAD IMPORTANTES

### ⚠️ Protección de Información Sensible

El sistema implementa **protección multinivel** para garantizar que los estudiantes sean los beneficiarios:

#### 🎓 SOLO ESTUDIANTES pueden:
- ✅ **Postular a ofertas laborales** - Rol requerido: `Applicant`
- ✅ **Ver DETALLES COMPLETOS** de ofertas (descripción, requisitos, contacto, remuneración)
- ✅ **Ver sus propias postulaciones**

#### 🏢 Empresas y Particulares (NO-Estudiantes):
- ⚠️ **NO pueden postular** a ofertas (403 Forbidden)
- ⚠️ **Solo ven información básica** de ofertas:
  - ✅ Título, ubicación, tipo, fechas
  - ❌ NO ven: Descripción completa, requisitos, contacto, remuneración
- ℹ️ **Razón**: Evitar robo de información, contactos externos y proteger a estudiantes

#### ✅ TODOS los usuarios autenticados pueden:
- ✅ Crear ofertas laborales
- ✅ Crear publicaciones de compra/venta

#### 🏢 Solo Empresas/Particulares (Offerents) pueden:
- ✅ Ver postulaciones a **sus** ofertas
- ✅ Actualizar estado de postulaciones

---

## �📋 Endpoints Disponibles


### 🔐 Crear Publicaciones (Requiere autenticación JWT)

#### 1. Crear Oferta Laboral/Voluntariado

**IMPORTANTE:** Cualquier usuario autenticado puede crear ofertas (Estudiantes, Empresas, Particulares, Admins).

```http
POST /api/publications/offers
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

Body:
{
  "title": "Desarrollador Backend",
  "description": "Buscamos desarrollador con experiencia en .NET",
  "endDate": "2025-12-31T23:59:59Z",
  "deadlineDate": "2025-11-30T23:59:59Z",
  "remuneration": 1500000,
  "offerType": 0,
  "location": "Antofagasta, Chile",
  "requirements": "2 años de experiencia en C#",
  "contactInfo": "rrhh@empresa.cl",
  "imagesURL": ["https://example.com/image1.jpg"],
  "isCvRequired": true
}

Response 200 OK:
{
  "message": "Oferta creada exitosamente",
  "data": "Oferta ID: 15"
}

Response 401 Unauthorized:
{
  "message": "Usuario no autenticado",
  "data": null
}
```

**Quién puede crear ofertas:**
- ✅ Estudiantes (ejemplo: "Busco tutor de matemáticas")
- ✅ Empresas (ofertas laborales formales)
- ✅ Particulares (trabajos freelance)
- ✅ Admins (ofertas institucionales)

**OfferType:**
- `0` = Trabajo (puede tener remuneración)
- `1` = Voluntariado (remuneración DEBE ser 0)

**Validaciones:**
- ✅ `title`: 5-200 caracteres
- ✅ `description`: 10-2000 caracteres
- ✅ `deadlineDate` debe ser futura
- ✅ `endDate` debe ser posterior a `deadlineDate`
- ✅ Voluntariados (`offerType = 1`) deben tener `remuneration = 0`
- ✅ `remuneration`: 0 - $100.000.000
- ✅ `imagesURL`: máximo 10 imágenes
- ✅ `isCvRequired`: booleano (por defecto `true`)
  - `true` = CV obligatorio para postular
  - `false` = CV opcional, puede postular sin CV

---

#### 2. Crear Publicación de Compra/Venta

```http
POST /api/publications/buysells
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

Body:
{
  "title": "Notebook HP Pavilion",
  "description": "Laptop en excelente estado, poco uso",
  "category": "Electrónica",
  "price": 450000,
  "imagesURL": ["https://example.com/laptop1.jpg"],
  "location": "Antofagasta",
  "contactInfo": "+56912345678"
}

Response 200 OK:
{
  "message": "Publicación de compra/venta creada exitosamente",
  "data": "Publicación ID: 8"
}
```

**Validaciones:**
- ✅ `title`: 5-200 caracteres
- ✅ `description`: 10-2000 caracteres
- ✅ `category`: requerido, máximo 100 caracteres
- ✅ `price`: 0 - $100.000.000
- ✅ `imagesURL`: máximo 10 imágenes
- ✅ `location`: opcional, máximo 200 caracteres
- ✅ `contactInfo`: opcional, máximo 200 caracteres

---

### 🌐 Obtener Ofertas Laborales (Público - sin autenticación)

#### 3. Listar Ofertas Activas

```http
GET /api/publications/offers

Response 200 OK:
{
  "message": "Ofertas recuperadas exitosamente",
  "data": [
    {
      "id": 1,
      "title": "Desarrollador Frontend",
      "description": "Buscamos...",
      "remuneration": 1200000,
      "location": "Antofagasta",
      "offerType": 0,
      "deadlineDate": "2025-11-30T23:59:59Z",
      "endDate": "2025-12-31T23:59:59Z",
      "companyName": "Tech Corp",
      "isActive": true
    }
  ]
}
```

---

#### 4. Ver Detalles de Oferta

**🔐 PROTECCIÓN DE INFORMACIÓN SENSIBLE:**

Este endpoint devuelve **información diferente** según el tipo de usuario:

##### Para Estudiantes Autenticados (INFORMACIÓN COMPLETA):

```http
GET /api/publications/offers/{id}
Authorization: Bearer {STUDENT_JWT_TOKEN}

Ejemplo: GET /api/publications/offers/1

Response 200 OK:
{
  "message": "Detalles de oferta recuperados exitosamente",
  "data": {
    "id": 1,
    "title": "Desarrollador Frontend",
    "description": "Descripción completa con todos los detalles...",  ✅
    "companyName": "Tech Corp",
    "location": "Antofagasta",
    "postDate": "2025-10-17T10:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "remuneration": 1200000,  ✅
    "offerType": "Trabajo"
  }
}
```

##### Para NO-Estudiantes o Usuarios Anónimos (INFORMACIÓN BÁSICA):

```http
GET /api/publications/offers/1
# Sin Authorization o con token de Empresa/Particular/Admin

Response 200 OK:
{
  "message": "Información básica de oferta (inicia sesión como estudiante para ver detalles completos)",
  "data": {
    "id": 1,
    "title": "Desarrollador Frontend",
    "companyName": "Tech Corp",
    "location": "Antofagasta",
    "postDate": "2025-10-17T10:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "offerType": "Trabajo",
    "message": "⚠️ Debes ser estudiante y estar autenticado para ver descripción completa, requisitos y remuneración"
  }
}
```

**Información OCULTA para NO-Estudiantes:**
- ❌ `description` (puede contener información de contacto)
- ❌ `remuneration` (datos de negocio sensibles)
- ❌ Requisitos detallados
- ❌ Información de contacto

**⚠️ Razón de Seguridad:**
Evitar que empresas competidoras roben contactos, que headhunters externos aprovechen la plataforma, y proteger la privacidad de las empresas ofertantes. **Los estudiantes son los beneficiarios del sistema**.

**Response 404 Not Found:**
```json
{
  "message": "Oferta no encontrada",
  "data": null
}
```

---

### 🛒 Obtener Publicaciones de Compra/Venta (Público)

#### 5. Listar Publicaciones de Compra/Venta Activas

```http
GET /api/publications/buysells

Response 200 OK:
{
  "message": "Publicaciones de compra/venta recuperadas exitosamente",
  "data": [
    {
      "id": 1,
      "title": "Notebook HP Pavilion",
      "category": "Electrónica",
      "price": 450000,
      "location": "Antofagasta",
      "publicationDate": "2025-10-15T14:30:00Z",
      "firstImageUrl": "https://example.com/laptop.jpg",
      "userId": 5,
      "userName": "JuanPerez123"
    }
  ]
}
```

---

#### 6. Ver Detalles de Publicación de Compra/Venta

```http
GET /api/publications/buysells/{id}

Ejemplo: GET /api/publications/buysells/1

Response 200 OK:
{
  "message": "Detalles de publicación recuperados exitosamente",
  "data": {
    "id": 1,
    "title": "Notebook HP Pavilion",
    "description": "Laptop en excelente estado, 8GB RAM, 256GB SSD",
    "category": "Electrónica",
    "price": 450000,
    "location": "Antofagasta",
    "contactInfo": "+56912345678",
    "publicationDate": "2025-10-15T14:30:00Z",
    "isActive": true,
    "imageUrls": [
      "https://example.com/laptop1.jpg",
      "https://example.com/laptop2.jpg"
    ],
    "userId": 5,
    "userName": "JuanPerez123",
    "userEmail": "juan.perez@alumnos.ucn.cl"
  }
}

Response 404 Not Found:
{
  "message": "Publicación no encontrada",
  "data": null
}
```

---

### 📝 Postulaciones a Ofertas (JWT Required - SOLO ESTUDIANTES)

**⚠️ RESTRICCIÓN IMPORTANTE:** Solo usuarios con rol `Applicant` (Estudiantes) pueden postular a ofertas.

#### 7. Postular a una Oferta (Postulación Directa)

**Roles permitidos:** `Applicant` (Estudiantes únicamente)

**⚠️ IMPORTANTE - CV Obligatorio u Opcional:**
- Cada oferta define si requiere CV obligatorio con el campo `isCvRequired`
- **CV Obligatorio** (`isCvRequired = true`): El estudiante DEBE tener CV en su perfil para postular
- **CV Opcional** (`isCvRequired = false`): El estudiante puede postular sin CV, solo con sus datos básicos y carta de motivación opcional

```http
POST /api/publications/offers/{id}/apply
Authorization: Bearer {STUDENT_JWT_TOKEN}

Ejemplo: POST /api/publications/offers/15/apply

# SIN BODY - Postulación directa
# El CV y carta de motivación se toman del perfil del estudiante
# CV obligatorio SOLO si la oferta tiene isCvRequired = true

Response 200 OK:
{
  "message": "Postulación creada exitosamente",
  "data": {
    "id": 42,
    "studentName": "Juan Pérez González",
    "studentEmail": "juan.perez@alumnos.ucn.cl",
    "offerTitle": "Desarrollador Frontend",
    "status": "Pendiente",
    "applicationDate": "2025-10-17T16:30:00Z",
    "curriculumVitae": "https://storage.com/cv/juan_perez.pdf",
    "motivationLetter": "Me interesa esta posición porque..."
  }
}

Response 400 Bad Request:
{
  "message": "Ya has postulado a esta oferta",
  "data": null
}

Response 401 Unauthorized (sin CV cuando es obligatorio):
{
  "message": "Esta oferta requiere CV. Por favor, sube tu CV en tu perfil antes de postular",
  "data": null
}

Response 401 Unauthorized (otro motivo):
{
  "message": "El estudiante no es elegible para postular",
  "data": null
}

Response 403 Forbidden:
{
  "message": "Solo los estudiantes pueden postular a ofertas",
  "data": null
}

Response 404 Not Found:
{
  "message": "La oferta no existe o no está activa",
  "data": null
}

Response 409 Conflict:
{
  "message": "La fecha límite para postular a esta oferta ha expirado",
  "data": null
}
```

**Validaciones automáticas:**
- ✅ Usuario DEBE ser estudiante (rol `Applicant`)
- ✅ Usuario DEBE tener `UserType = Estudiante`
- ✅ Email `@alumnos.ucn.cl`
- ✅ No puede estar baneado
- ⚠️ **CV obligatorio SOLO si la oferta lo requiere** (`isCvRequired = true`)
  - Si `isCvRequired = true` → Debe tener CV cargado (error: "Esta oferta requiere CV. Por favor, sube tu CV en tu perfil antes de postular")
  - Si `isCvRequired = false` → Puede postular sin CV
- ✅ La oferta debe estar activa
- ✅ No debe haber vencido la `deadlineDate`
- ✅ La oferta no debe haber finalizado (`endDate`)
- ✅ No puede haber postulado anteriormente

**⚠️ Si intentas postular como Empresa/Particular/Admin:**
```json
{
  "status": 403,
  "message": "Forbidden - Solo estudiantes pueden postular a ofertas"
}
```

---

#### 8. Ver Mis Postulaciones

**Roles permitidos:** `Applicant` (Estudiantes únicamente)

```http
GET /api/publications/offers/my-applications
Authorization: Bearer {STUDENT_JWT_TOKEN}

Response 200 OK:
{
  "message": "Postulaciones recuperadas exitosamente",
  "data": [
    {
      "id": 42,
      "studentName": "Juan Pérez González",
      "studentEmail": "juan.perez@alumnos.ucn.cl",
      "offerTitle": "Desarrollador Frontend",
      "status": "Pendiente",
      "applicationDate": "2025-10-17T16:30:00Z",
      "curriculumVitae": "https://storage.com/cv/juan_perez.pdf",
      "motivationLetter": "Me interesa..."
    },
    {
      "id": 38,
      "studentName": "Juan Pérez González",
      "studentEmail": "juan.perez@alumnos.ucn.cl",
      "offerTitle": "Asistente de Investigación",
      "status": "Aceptado",
      "applicationDate": "2025-10-10T10:00:00Z",
      "curriculumVitae": "https://storage.com/cv/juan_perez.pdf",
      "motivationLetter": null
    }
  ]
}

Response 401 Unauthorized:
{
  "message": "No autenticado o token inválido",
  "data": null
}

Response 403 Forbidden:
{
  "message": "Solo los estudiantes pueden ver sus postulaciones",
  "data": null
}
```

**⚠️ Si intentas acceder como Empresa/Particular/Admin:**
```json
{
  "status": 403,
  "message": "Forbidden - Solo estudiantes pueden ver sus postulaciones"
}
```

---

## � Postulaciones (JobApplicationController)

**Base URL:** `/api/job-applications`

Gestión avanzada de postulaciones para estudiantes y empresas.

---

### Para Estudiantes

#### 1. Postular a Oferta (Alternativa)

```http
POST /api/job-applications/apply/{offerId}
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

Body:
{
  "motivationLetter": "Carta de motivación opcional..."
}

Response 200 OK:
{
  "message": "Postulación creada exitosamente",
  "data": {
    "id": 42,
    "studentName": "Juan Pérez",
    "studentEmail": "juan.perez@alumnos.ucn.cl",
    "offerTitle": "Desarrollador Backend",
    "status": "Pendiente",
    "applicationDate": "2025-10-17T16:30:00Z",
    "curriculumVitae": "https://...",
    "motivationLetter": "..."
  }
}
```

---

#### 2. Ver Mis Postulaciones (Alternativa)

```http
GET /api/job-applications/my-applications
Authorization: Bearer {JWT_TOKEN}

Response 200 OK:
{
  "message": "Postulaciones recuperadas exitosamente",
  "data": [...]
}
```

---

### Para Empresas

#### 3. Ver Postulaciones de una Oferta

```http
GET /api/job-applications/offer/{offerId}
Authorization: Bearer {JWT_TOKEN}

Ejemplo: GET /api/job-applications/offer/15

Response 200 OK:
{
  "message": "Postulaciones de la oferta 15 recuperadas exitosamente",
  "data": [
    {
      "id": 42,
      "studentName": "Juan Pérez González",
      "studentEmail": "juan.perez@alumnos.ucn.cl",
      "offerTitle": "Desarrollador Backend",
      "status": "Pendiente",
      "applicationDate": "2025-10-17T16:30:00Z",
      "curriculumVitae": "https://storage.com/cv/juan_perez.pdf",
      "motivationLetter": "Me interesa..."
    },
    {
      "id": 43,
      "studentName": "María González",
      "studentEmail": "maria.gonzalez@alumnos.ucn.cl",
      "offerTitle": "Desarrollador Backend",
      "status": "Pendiente",
      "applicationDate": "2025-10-17T17:00:00Z",
      "curriculumVitae": "https://storage.com/cv/maria_gonzalez.pdf",
      "motivationLetter": null
    }
  ]
}
```

**Seguridad:** Solo el creador de la oferta puede ver las postulaciones.

---

#### 4. Ver Todas las Postulaciones de Mis Ofertas

```http
GET /api/job-applications/my-offers-applications
Authorization: Bearer {JWT_TOKEN}

Response 200 OK:
{
  "message": "Postulaciones recibidas recuperadas exitosamente",
  "data": [
    {
      "id": 42,
      "studentName": "Juan Pérez González",
      "studentEmail": "juan.perez@alumnos.ucn.cl",
      "offerTitle": "Desarrollador Backend",
      "status": "Pendiente",
      "applicationDate": "2025-10-17T16:30:00Z",
      "curriculumVitae": "https://...",
      "motivationLetter": "..."
    },
    {
      "id": 43,
      "studentName": "María González",
      "studentEmail": "maria.gonzalez@alumnos.ucn.cl",
      "offerTitle": "Diseñador UX/UI",
      "status": "Aceptado",
      "applicationDate": "2025-10-15T10:00:00Z",
      "curriculumVitae": "https://...",
      "motivationLetter": null
    }
  ]
}
```

**Nota:** Retorna todas las postulaciones de TODAS las ofertas creadas por la empresa autenticada.

---

#### 5. Actualizar Estado de Postulación

```http
PATCH /api/job-applications/{applicationId}/status
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json

Ejemplo: PATCH /api/job-applications/42/status

Body:
{
  "newStatus": "Aceptado"
}

Response 200 OK:
{
  "message": "Estado de postulación actualizado a 'Aceptado' exitosamente",
  "data": null
}

Response 401 Unauthorized:
{
  "message": "No tienes permiso para modificar esta postulación",
  "data": null
}

Response 400 Bad Request:
{
  "message": "Estado inválido. Debe ser uno de: Pendiente, Aceptado, Rechazado",
  "data": null
}
```

**Estados válidos:**
- `Pendiente`
- `Aceptado`
- `Rechazado`

**Seguridad:** Solo el creador de la oferta puede actualizar el estado.

---

## 🔒 Seguridad y JWT

---

## 🔒 Seguridad y JWT

### Obtención del Token

Utiliza el endpoint `/api/auth/login` con credenciales válidas:

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@alumnos.ucn.cl",
  "password": "TuPassword123!"
}

Response:
{
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": 5,
      "email": "usuario@alumnos.ucn.cl",
      "role": "Student",
      "name": "Juan Pérez"
    }
  }
}
```

### Usar el Token en Postman

1. **Copiar el token JWT** de la respuesta del login
2. **Ir al tab "Authorization"** en Postman
3. **Seleccionar Type:** `Bearer Token`
4. **Pegar el token** en el campo Token
5. **Hacer la petición** normalmente

**Formato del Header (generado automáticamente):**
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Información contenida en el JWT

El token contiene claims con información del usuario:
- `NameIdentifier`: ID del usuario en la base de datos
- `Email`: Email del usuario autenticado
- `Role`: Rol del usuario (`Student`, `Individual`, `Company`, `Admin`)

**Expiración:** El token es válido por **7 días** desde la emisión.

### Endpoints Protegidos (Requieren JWT)

| Endpoint | Método | Requiere Auth | Rol Permitido | Descripción |
|----------|--------|---------------|---------------|-------------|
| `/api/publications/offers` | POST | ✅ Sí | **Cualquiera** | Crear oferta laboral |
| `/api/publications/buysells` | POST | ✅ Sí | **Cualquiera** | Crear buy/sell |
| `/api/publications/offers/{id}/apply` | POST | ✅ Sí | **Applicant SOLO** | Postular a oferta |
| `/api/publications/offers/my-applications` | GET | ✅ Sí | **Applicant SOLO** | Ver mis postulaciones |
| `/api/job-applications/my-applications` | GET | ✅ Sí | **Applicant SOLO** | Ver mis postulaciones (alt) |
| `/api/job-applications/offer/{id}` | GET | ✅ Sí | **Offerent** | Ver postulaciones de mi oferta |
| `/api/job-applications/my-offers-applications` | GET | ✅ Sí | **Offerent** | Ver todas mis postulaciones |
| `/api/job-applications/{id}/status` | PATCH | ✅ Sí | **Offerent** | Actualizar estado postulación |
| `/api/publications/offers` | GET | ❌ No | **Público** | Ver ofertas (limitado para no-estudiantes) |
| `/api/publications/buysells` | GET | ❌ No | **Público** | Ver buy/sells |
| `/api/publications/offers/{id}` | GET | ❌ No | **Público** | Ver oferta (info completa solo para estudiantes) |
| `/api/publications/buysells/{id}` | GET | ❌ No | **Público** | Ver buy/sell |
| `/api/auth/*` | POST | ❌ No | **Público** | Auth endpoints |

**Leyenda de Roles:**
- `Applicant` = Estudiantes (`@alumnos.ucn.cl`)
- `Offerent` = Empresas + Particulares
- `Admin` = Administradores
- `Cualquiera` = Cualquier usuario autenticado
- `Público` = Sin autenticación requerida

---

## 🧪 Cómo Probar en Postman

### Credenciales de Prueba

El sistema incluye usuarios pre-configurados con credenciales fáciles:

| Tipo | Email | Password | Rol |
|------|-------|----------|-----|
| 👨‍🎓 Estudiante | `estudiante@alumnos.ucn.cl` | `Test123!` | Applicant |
| 🏢 Empresa | `empresa@techcorp.cl` | `Test123!` | Offerent |
| 👤 Particular | `particular@ucn.cl` | `Test123!` | Offerent |
| 👑 Admin | `admin@ucn.cl` | `Test123!` | Admin |

Ver [`TEST_CREDENTIALS.md`](./TEST_CREDENTIALS.md) para detalles completos.

---

### Flujo de Testing Completo

#### Escenario 1: Estudiante Postula a Oferta (EXITOSO)

**Paso 1: Login como Estudiante**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "estudiante@alumnos.ucn.cl",
  "password": "Test123!",
  "rememberMe": true
}

→ Copiar el token de la respuesta
```

**Paso 2: Ver Ofertas Disponibles**
```http
GET /api/publications/offers
# Sin Authorization (público)

→ Copiar el ID de una oferta (ej: 1)
```

**Paso 3: Ver Detalles COMPLETOS (como estudiante)**
```http
GET /api/publications/offers/1
Authorization: Bearer {token_estudiante}

→ Deberías ver: description, remuneration, requirements, contactInfo
```

**Paso 4: Postular a la Oferta**
```http
POST /api/publications/offers/1/apply
Authorization: Bearer {token_estudiante}
Content-Type: application/json

{
  "motivationLetter": "Me interesa porque tengo experiencia relevante..."
}

→ Response 200 OK: "Postulación creada exitosamente"
```

**Paso 5: Ver Mis Postulaciones**
```http
GET /api/publications/offers/my-applications
Authorization: Bearer {token_estudiante}

→ Deberías ver tu postulación con status "Pendiente"
```

---

#### Escenario 2: Empresa Intenta Postular (DEBE FALLAR - 403)

**Paso 1: Login como Empresa**
```http
POST /api/auth/login
{
  "email": "empresa@techcorp.cl",
  "password": "Test123!",
  "rememberMe": true
}

→ Copiar token de empresa
```

**Paso 2: Intentar Postular (DEBE FALLAR)**
```http
POST /api/publications/offers/1/apply
Authorization: Bearer {token_empresa}
{
  "motivationLetter": "Queremos postular..."
}

→ Response 403 Forbidden
→ "Solo los estudiantes pueden postular a ofertas"
```

**Paso 3: Ver Detalles de Oferta (INFORMACIÓN LIMITADA)**
```http
GET /api/publications/offers/1
Authorization: Bearer {token_empresa}

→ Response 200 OK pero SIN: description, remuneration
→ Mensaje: "⚠️ Debes ser estudiante para ver información completa"
```

**Paso 4: Crear Oferta (EXITOSO - Empresas SÍ pueden crear)**
```http
POST /api/publications/offers
Authorization: Bearer {token_empresa}
{
  "title": "Desarrollador Full Stack",
  "description": "Buscamos desarrollador...",
  "endDate": "2025-12-31T23:59:59Z",
  "deadlineDate": "2025-11-30T23:59:59Z",
  "remuneration": 2000000,
  "offerType": 0,
  "location": "Antofagasta",
  "requirements": "3 años de experiencia",
  "contactInfo": "rrhh@techcorp.cl",
  "imagesURL": []
}

→ Response 200 OK: "Oferta creada exitosamente"
→ Copiar el ID de la oferta creada
```

**Paso 5: Ver Postulaciones a Mi Oferta**
```http
GET /api/job-applications/offer/{id_oferta_creada}
Authorization: Bearer {token_empresa}

→ Deberías ver las postulaciones de estudiantes
```

---

#### Escenario 3: Usuario Anónimo (Sin Login)

**Paso 1: Ver Listado de Ofertas (PÚBLICO)**
```http
GET /api/publications/offers
# Sin Authorization

→ Response 200 OK: Lista de ofertas
```

**Paso 2: Ver Detalles de Oferta (INFORMACIÓN LIMITADA)**
```http
GET /api/publications/offers/1
# Sin Authorization

→ Response 200 OK pero SIN datos sensibles
→ Solo: title, companyName, location, dates, offerType
→ NO incluye: description, remuneration, requirements
```

**Paso 3: Intentar Postular (DEBE FALLAR - 401)**
```http
POST /api/publications/offers/1/apply
# Sin Authorization
{
  "motivationLetter": "..."
}

→ Response 401 Unauthorized
→ "Usuario no autenticado"
```

---

### Paso 1: Registrar Usuario

```http
POST /api/auth/register/student
Content-Type: application/json

{
  "email": "test.student@alumnos.ucn.cl",
  "password": "Test123!",
  "confirmPassword": "Test123!",
  "userName": "TestStudent",
  "firstName": "Test",
  "lastName": "Student",
  "phoneNumber": "+56912345678",
  "career": "Ingeniería Civil en Computación",
  "curriculumVitae": "https://example.com/cv.pdf"
}
```

### Paso 2: Verificar Email

```http
POST /api/auth/verify-email
Content-Type: application/json

{
  "email": "test.student@alumnos.ucn.cl",
  "token": "{token_recibido_por_email}"
}
```

### Paso 3: Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test.student@alumnos.ucn.cl",
  "password": "Test123!"
}
```

**Copiar el campo `token` de la respuesta.**

### Paso 4: Configurar Authorization en Postman

1. Click en el tab **"Authorization"**
2. Seleccionar **Type: Bearer Token**
3. Pegar el token copiado
4. ✅ Listo para usar endpoints protegidos

### Paso 5: Probar Endpoint Protegido

```http
GET /api/publications/offers/my-applications
Authorization: Bearer {tu_token_aquí}
```

---

## 🆘 Errores Comunes y Soluciones

### Error 415: Unsupported Media Type

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.16",
  "title": "Unsupported Media Type",
  "status": 415,
  "traceId": "00-494a5ac1be8f7db5bac7288796df3810-992b8e7cabe1b869-00"
}
```

**Causas:**
- ❌ Falta el header `Content-Type: application/json`
- ❌ El body no está en formato JSON
- ❌ Estás usando **form-data** o **x-www-form-urlencoded** en lugar de **raw JSON**
- ❌ El dropdown del body está en "Text" en lugar de "JSON"

**Soluciones en Postman:**

1. **Tab Headers:** Agregar manualmente si no existe
   ```
   Content-Type: application/json
   ```

2. **Tab Body:**
   - Seleccionar **raw** (no form-data)
   - Cambiar dropdown de "Text" a **JSON**

3. **Verificar JSON válido:**
   ```json
   {
     "title": "Desarrollador Backend",
     "description": "Buscamos desarrollador con experiencia",
     "endDate": "2025-12-31T23:59:59Z",
     "deadlineDate": "2025-11-30T23:59:59Z",
     "remuneration": 1500000,
     "offerType": 0,
     "location": "Antofagasta",
     "requirements": "2 años de experiencia",
     "contactInfo": "rrhh@empresa.cl",
     "imagesURL": []
   }
   ```

4. **Propiedades requeridas (no pueden faltar):**
   - ✅ `title` (string, 5-200 caracteres)
   - ✅ `description` (string, 10-2000 caracteres)
   - ✅ `endDate` (DateTime en formato ISO 8601)
   - ✅ `deadlineDate` (DateTime en formato ISO 8601)
   - ✅ `remuneration` (number, 0-100000000)
   - ✅ `offerType` (number: 0=Trabajo, 1=Voluntariado)

5. **Propiedades opcionales:**
   - `location` (string, max 200)
   - `requirements` (string, max 1000)
   - `contactInfo` (string, max 200)
   - `imagesURL` (array de strings, max 10)

**Configuración Visual Correcta:**
```
Body: ● raw  [JSON ▼]  ← Importante: JSON, no Text

{
  "title": "...",
  "description": "...",
  ...
}
```

**Tip:** Si usas Postman, al seleccionar "JSON" en el dropdown, automáticamente agrega el header `Content-Type: application/json`.

---

### Error 401: Unauthorized

```json
{
  "message": "Usuario no autenticado",
  "data": null
}
```

**Causas:**
- ❌ No incluiste el header `Authorization`
- ❌ Token JWT inválido
- ❌ Token expirado (más de 7 días)
- ❌ Formato incorrecto (debe ser `Bearer {token}`)

**Soluciones:**
1. Verifica que el token esté en el header
2. Verifica el formato: `Authorization: Bearer eyJhbG...`
3. Genera un nuevo token con `/api/auth/login`

---

### Error 403: Forbidden (Nuevo - Restricción por Rol)

```json
{
  "message": "Solo los estudiantes pueden postular a ofertas",
  "data": null
}
```

**Causas:**
- ❌ Intentaste postular sin ser estudiante (rol Applicant)
- ❌ Intentaste ver tus postulaciones sin ser estudiante
- ❌ Intentaste modificar una postulación que no es tuya

**Soluciones:**
1. Verifica que tu usuario tenga el rol `Applicant` (estudiante)
2. Solo usuarios con email `@alumnos.ucn.cl` pueden postular
3. Usa las credenciales de prueba: `estudiante@alumnos.ucn.cl` / `Test123!`

**Ejemplo de Testing:**
```http
# ❌ INCORRECTO (Empresa intenta postular)
POST /api/publications/offers/1/apply
Authorization: Bearer {token_empresa}
→ 403 Forbidden

# ✅ CORRECTO (Estudiante postula)
POST /api/publications/offers/1/apply
Authorization: Bearer {token_estudiante}
→ 200 OK
```

---

### Información Limitada al Ver Ofertas (Nuevo - Protección de Datos)

**Situación:** Al hacer `GET /api/publications/offers/1` como empresa o sin login, no ves la información completa.

```json
{
  "message": "Información básica de oferta (inicia sesión como estudiante para ver detalles completos)",
  "data": {
    "id": 1,
    "title": "Desarrollador Backend",
    "companyName": "Tech Corp",
    "location": "Antofagasta",
    "postDate": "2025-10-17T10:00:00Z",
    "endDate": "2025-12-31T23:59:59Z",
    "offerType": "Trabajo",
    "message": "⚠️ Debes ser estudiante y estar autenticado para ver descripción completa, requisitos y remuneración"
  }
}
```

**¿Por qué?**
- 🔒 Protección contra robo de información
- 🔒 Evitar contactos externos a la plataforma
- 🔒 Proteger datos sensibles de empresas
- 🔒 Los estudiantes son los beneficiarios del sistema

**Solución:**
1. Inicia sesión como estudiante: `estudiante@alumnos.ucn.cl` / `Test123!`
2. Usa el token de estudiante en la petición
3. Ahora verás: `description`, `remuneration`, `requirements`, `contactInfo`

---

### Error 400: Bad Request (Validación)

```json
{
  "message": "Errores de validación",
  "data": [
    {
      "field": "title",
      "error": "El título debe tener entre 5 y 200 caracteres"
    },
    {
      "field": "remuneration",
      "error": "Los voluntariados deben tener remuneración 0"
    }
  ]
}
```

**Solución:** Revisa los datos enviados y corrige según las validaciones documentadas.

---

### Error 404: Not Found

```json
{
  "message": "Oferta no encontrada",
  "data": null
}
```

**Solución:** Verifica que el ID del recurso sea correcto y que el recurso exista.

---

### Error 409: Conflict

```json
{
  "message": "Ya has postulado a esta oferta",
  "data": null
}
```

**Solución:** Este estudiante ya tiene una postulación activa para esta oferta.

---

### Error 403: Forbidden

```json
{
  "message": "No tienes permiso para realizar esta acción",
  "data": null
}
```

**Causas:**
- Intentaste postular sin ser estudiante (rol Applicant)
- Intentaste ver postulaciones sin ser estudiante
- Intentaste modificar una postulación que no es tuya
- Intentaste ver postulaciones de una oferta que no creaste

**Solución:** Verifica que tu usuario tenga el rol correcto para esta acción.

**Tabla de Permisos:**

| Acción | Estudiante | Empresa | Particular | Admin |
|--------|------------|---------|------------|-------|
| Crear oferta | ✅ | ✅ | ✅ | ✅ |
| Ver lista ofertas | ✅ | ✅ | ✅ | ✅ |
| Ver detalles COMPLETOS oferta | ✅ | ❌ | ❌ | ❌ |
| Postular a oferta | ✅ | ❌ | ❌ | ❌ |
| Ver mis postulaciones | ✅ | ❌ | ❌ | ❌ |
| Ver postulaciones de mi oferta | ❌ | ✅ | ✅ | ❌ |
| Actualizar estado postulación | ❌ | ✅ | ✅ | ❌ |

---

## 🧩 Colección de Postman

### Importar Colección (JSON)

Copia y pega este JSON en Postman (Import → Raw text):

```json
{
  "info": {
    "name": "BolsaFE UCN API",
    "description": "Colección completa de endpoints para BolsaFE UCN Backend",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://localhost:7169",
      "type": "string"
    },
    {
      "key": "jwt_token",
      "value": "",
      "type": "string"
    }
  ],
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Register Student",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "url": "{{baseUrl}}/api/auth/register/student",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"email\": \"test@alumnos.ucn.cl\",\n  \"password\": \"Test123!\",\n  \"confirmPassword\": \"Test123!\",\n  \"userName\": \"TestUser\",\n  \"firstName\": \"Test\",\n  \"lastName\": \"User\",\n  \"phoneNumber\": \"+56912345678\",\n  \"career\": \"Ing. Civil\",\n  \"curriculumVitae\": \"https://cv.com/test.pdf\"\n}"
            }
          }
        },
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "header": [{"key": "Content-Type", "value": "application/json"}],
            "url": "{{baseUrl}}/api/auth/login",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"email\": \"test@alumnos.ucn.cl\",\n  \"password\": \"Test123!\"\n}"
            }
          },
          "event": [
            {
              "listen": "test",
              "script": {
                "exec": [
                  "var jsonData = pm.response.json();",
                  "pm.collectionVariables.set('jwt_token', jsonData.data.token);"
                ]
              }
            }
          ]
        }
      ]
    },
    {
      "name": "Publications - Offers",
      "item": [
        {
          "name": "Create Offer",
          "request": {
            "method": "POST",
            "header": [
              {"key": "Content-Type", "value": "application/json"},
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/publications/offers",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"title\": \"Desarrollador Backend\",\n  \"description\": \"Buscamos desarrollador .NET\",\n  \"endDate\": \"2025-12-31T23:59:59Z\",\n  \"deadlineDate\": \"2025-11-30T23:59:59Z\",\n  \"remuneration\": 1500000,\n  \"offerType\": 0,\n  \"location\": \"Antofagasta\",\n  \"requirements\": \"2 años C#\",\n  \"contactInfo\": \"hr@company.cl\",\n  \"imagesURL\": []\n}"
            }
          }
        },
        {
          "name": "Get All Offers",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/publications/offers"
          }
        },
        {
          "name": "Get Offer By ID",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/publications/offers/1"
          }
        },
        {
          "name": "Apply to Offer",
          "request": {
            "method": "POST",
            "header": [
              {"key": "Content-Type", "value": "application/json"},
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/publications/offers/1/apply",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"motivationLetter\": \"Me interesa porque...\"\n}"
            }
          }
        },
        {
          "name": "My Applications",
          "request": {
            "method": "GET",
            "header": [
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/publications/offers/my-applications"
          }
        }
      ]
    },
    {
      "name": "Publications - Buy/Sell",
      "item": [
        {
          "name": "Create Buy/Sell",
          "request": {
            "method": "POST",
            "header": [
              {"key": "Content-Type", "value": "application/json"},
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/publications/buysells",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"title\": \"Notebook HP\",\n  \"description\": \"Laptop excelente estado\",\n  \"category\": \"Electrónica\",\n  \"price\": 450000,\n  \"imagesURL\": [],\n  \"location\": \"Antofagasta\",\n  \"contactInfo\": \"+56912345678\"\n}"
            }
          }
        },
        {
          "name": "Get All Buy/Sells",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/publications/buysells"
          }
        },
        {
          "name": "Get Buy/Sell By ID",
          "request": {
            "method": "GET",
            "url": "{{baseUrl}}/api/publications/buysells/1"
          }
        }
      ]
    },
    {
      "name": "Job Applications",
      "item": [
        {
          "name": "Get My Offers Applications (Company)",
          "request": {
            "method": "GET",
            "header": [
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/job-applications/my-offers-applications"
          }
        },
        {
          "name": "Get Applications By Offer (Company)",
          "request": {
            "method": "GET",
            "header": [
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/job-applications/offer/1"
          }
        },
        {
          "name": "Update Application Status",
          "request": {
            "method": "PATCH",
            "header": [
              {"key": "Content-Type", "value": "application/json"},
              {"key": "Authorization", "value": "Bearer {{jwt_token}}"}
            ],
            "url": "{{baseUrl}}/api/job-applications/1/status",
            "body": {
              "mode": "raw",
              "raw": "{\n  \"newStatus\": \"Aceptado\"\n}"
            }
          }
        }
      ]
    }
  ]
}
```

### Configurar Variables de Postman

1. Importa la colección
2. Variables pre-configuradas:
   - `baseUrl`: `https://localhost:7169` (cambiar según tu entorno)
   - `jwt_token`: Se auto-completa al hacer login

3. **Automático:** El token se guarda automáticamente al hacer login exitoso

---

## 🧪 Testing y Validación

### Flujo de Testing Recomendado

1. **Registrar usuarios de diferentes tipos**
   - Student: `student@alumnos.ucn.cl`
   - Company: `company@empresa.cl`
   - Individual: `individual@ucn.cl`

2. **Verificar emails** (si SMTP configurado)

3. **Login con cada usuario** y guardar tokens

4. **Como Company:**
   - Crear ofertas laborales
   - Crear ofertas de voluntariado
   - Ver postulaciones recibidas
   - Actualizar estados de postulaciones

5. **Como Student:**
   - Ver ofertas disponibles
   - Postular a ofertas
   - Ver mis postulaciones

6. **Como cualquier usuario:**
   - Crear publicaciones de compra/venta
   - Ver publicaciones activas

7. **Probar casos de error:**
   - Postular dos veces a la misma oferta
   - Crear oferta sin autenticación
   - Postular con usuario Company
   - Actualizar postulación de oferta ajena

---

### JWT Token
Los endpoints que requieren autenticación necesitan un token JWT válido en el header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

El token debe contener el claim `ClaimTypes.NameIdentifier` con el ID del usuario.

---

## 📋 Estructura de Respuesta Genérica

Todos los endpoints retornan el mismo formato:

```json
{
  "message": "Descripción del resultado (en español)",
  "data": {
    // Datos específicos del endpoint
    // Puede ser: objeto, array, string, number o null
  }
}
```

**Ejemplos:**

```json
// Éxito con datos
{
  "message": "Ofertas recuperadas exitosamente",
  "data": [...]
}

// Éxito sin datos adicionales
{
  "message": "Estado actualizado exitosamente",
  "data": null
}

// Error
{
  "message": "Email ya registrado",
  "data": null
}
```

---

## 📊 Códigos de Estado HTTP

| Código | Icono | Descripción | Cuándo ocurre |
|--------|-------|-------------|---------------|
| 200 | ✅ | OK - Petición exitosa | Request procesado correctamente |
| 201 | ✅ | Created - Recurso creado | POST exitoso (creación) |
| 400 | ⚠️ | Bad Request - Solicitud inválida | Errores de validación, datos incorrectos |
| 401 | 🔒 | Unauthorized - No autenticado | Token JWT inválido, expirado o ausente |
| 403 | 🚫 | Forbidden - Sin permisos | Usuario autenticado pero sin permisos |
| 404 | ❌ | Not Found - No encontrado | Recurso no existe |
| 409 | ⚡ | Conflict - Conflicto | Ya existe (ej: ya postulado) |
| 500 | 💥 | Internal Server Error | Error inesperado del servidor |

---

## 🎨 Estructura de Respuestas

Todas las respuestas siguen el formato `GenericResponse<T>`:

```typescript
{
  "message": string,    // Mensaje descriptivo
  "data": T | null      // Datos de respuesta (tipo genérico)
}
```

---

## 📝 Notas Importantes

### Ofertas Laborales vs Voluntariados
- **Trabajo** (`offerType = 0`): Puede tener cualquier remuneración ≥ $0
- **Voluntariado** (`offerType = 1`): Remuneración DEBE ser $0

### Diferencias entre Publicaciones
- **Ofertas (`/offers`)**: Para trabajos y voluntariados. Incluyen fechas límite, remuneración, y permiten postulaciones de estudiantes.
- **Compra/Venta (`/buysells`)**: Para vender productos o servicios. Incluyen precio, categoría, y son publicaciones simples sin sistema de postulación.

### Seguridad
- ✅ El `studentId` se obtiene automáticamente del token JWT
- ✅ No es posible postular como otro usuario
- ✅ Los endpoints de creación requieren autenticación
- ✅ Los endpoints de listado son públicos

---

## 🚀 Ejemplo de Flujo Completo

### 1. Usuario crea una oferta laboral
```http
POST /api/publications/offers
Authorization: Bearer {token}
→ Response: Oferta ID: 15
```

### 2. Estudiantes consultan ofertas disponibles
```http
GET /api/publications/offers
→ Response: Lista de 10 ofertas activas
```

### 3. Estudiante ve detalles de oferta
```http
GET /api/publications/offers/15
→ Response: Detalles completos de la oferta
```

### 4. Estudiante postula a la oferta
```http
POST /api/publications/offers/15/apply
Authorization: Bearer {student_token}
→ Response: Postulación creada exitosamente
```

### 5. Estudiante verifica sus postulaciones
```http
GET /api/publications/offers/my-applications
Authorization: Bearer {student_token}
→ Response: Lista con todas sus postulaciones
```

---

## 🔗 Recursos Adicionales

- **Swagger UI:** `https://localhost:7169/swagger` (solo en Development)
- **Logs:** Archivos JSON estructurados en carpeta `logs/`
- **Base de Datos:** PostgreSQL con Entity Framework Core
- **Migraciones:** Ver carpeta `Migrations/`
- **Autenticación:** JWT Bearer tokens (HS256)
- **Tiempo de expiración token:** 7 días

### 📚 Documentación de Testing

Para realizar pruebas completas de la API en Postman, consulta los siguientes recursos:

- **📖 Guía Completa de Testing:** [`POSTMAN_TESTING_GUIDE.md`](./POSTMAN_TESTING_GUIDE.md)
  - 19 endpoints documentados con ejemplos completos
  - Instrucciones paso a paso para cada endpoint
  - Scripts de auto-guardado de variables
  - Flujos de testing completos por rol de usuario
  - Solución de errores comunes

- **⚡ Referencia Rápida:** [`QUICK_REFERENCE.md`](./QUICK_REFERENCE.md)
  - Tabla resumen de todos los endpoints
  - Ejemplos de requests abreviados
  - Variables y scripts útiles
  - Orden recomendado de testing

- **🔧 Configuración de Cloudinary:** [`CLOUDINARY_SETUP.md`](./CLOUDINARY_SETUP.md)
  - Guía de implementación de subida de imágenes
  - Configuración de Cloudinary
  - Alternativas de implementación

---

## 📝 Limitaciones Actuales (V1.0)

| Característica | Estado | Notas |
|----------------|--------|-------|
| Paginación | ❌ No implementado | Endpoints retornan todos los registros activos |
| Filtros | ❌ No implementado | Futuro: filtrar por categoría, precio, ubicación |
| Búsqueda | ❌ No implementado | Futuro: búsqueda por texto/keywords |
| Ordenamiento | ✅ Por fecha | Más reciente primero |
| Soft Delete | ✅ Implementado | `isActive = false` en lugar de eliminación física |
| Rate Limiting | ❌ No implementado | Sin protección contra spam |
| File Upload | ❌ No implementado | URLs de imágenes/CV son strings |
| Email Service | ⚠️ Pendiente | Verificación de email configurada pero requiere SMTP |
| Protección de Datos | ✅ **NUEVO** | Solo estudiantes ven información completa de ofertas |
| Control de Acceso por Rol | ✅ **NUEVO** | Solo estudiantes pueden postular |

### 🔐 Características de Seguridad Implementadas (V1.0)

| Característica | Estado | Descripción |
|----------------|--------|-------------|
| JWT Authentication | ✅ Implementado | Tokens con expiración de 7 días |
| Role-Based Authorization | ✅ Implementado | `Applicant`, `Offerent`, `Admin` |
| Información Sensible Protegida | ✅ **NUEVO** | No-estudiantes no ven contacto ni detalles |
| Validación Doble de Rol | ✅ **NUEVO** | `[Authorize(Roles)]` + verificación `UserType` |
| Ownership Validation | ✅ Implementado | Solo puedes modificar tus recursos |
| Email Confirmation | ✅ Implementado | Verificación obligatoria antes de login |
| Password Hashing | ✅ Implementado | Identity Framework con hash seguro |

### Próximas Características (Roadmap)

- [ ] Paginación y límite de resultados
- [ ] Búsqueda full-text en publicaciones
- [ ] Filtros avanzados (precio, ubicación, categoría, tipo de usuario)
- [ ] Subida de archivos (imágenes, CV)
- [ ] Notificaciones por email (postulación recibida, estado actualizado)
- [ ] Sistema de favoritos/guardados
- [ ] Estadísticas y analytics para empresas
- [ ] Rate limiting por IP/usuario
- [ ] Moderación de contenido por admins
- [ ] Sistema de reportes de contenido inapropiado
- [ ] Badges visuales en frontend (Empresa, Estudiante, Particular)
- [ ] Historial de cambios de estado en postulaciones

---

## �📞 Contacto y Soporte

Para más información sobre la API, consultar la documentación Swagger en:
```
https://localhost:7169/swagger
```

---

## 🔐 Resumen de Políticas de Seguridad y Acceso

### Matriz Completa de Permisos por Endpoint y Rol

| Endpoint | Método | Anónimo | Estudiante | Empresa | Particular | Admin |
|----------|--------|---------|------------|---------|------------|-------|
| **Autenticación** |
| `/api/auth/register/*` | POST | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/auth/login` | POST | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/auth/verify-email` | POST | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Ofertas Laborales - Lectura** |
| `/api/publications/offers` (lista) | GET | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/publications/offers/{id}` (detalles) | GET | ⚠️ Básico | ✅ Completo | ⚠️ Básico | ⚠️ Básico | ⚠️ Básico |
| **Ofertas Laborales - Escritura** |
| `/api/publications/offers` (crear) | POST | ❌ | ✅ | ✅ | ✅ | ✅ |
| `/api/publications/offers/{id}/apply` | POST | ❌ | ✅ | ❌ | ❌ | ❌ |
| `/api/publications/offers/my-applications` | GET | ❌ | ✅ | ❌ | ❌ | ❌ |
| **Compra/Venta** |
| `/api/publications/buysells` (lista) | GET | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/publications/buysells/{id}` | GET | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/publications/buysells` (crear) | POST | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Gestión de Postulaciones** |
| `/api/job-applications/my-applications` | GET | ❌ | ✅ | ❌ | ❌ | ❌ |
| `/api/job-applications/offer/{id}` | GET | ❌ | ❌ | ✅* | ✅* | ❌ |
| `/api/job-applications/my-offers-applications` | GET | ❌ | ❌ | ✅ | ✅ | ❌ |
| `/api/job-applications/{id}/status` | PATCH | ❌ | ❌ | ✅* | ✅* | ❌ |
| **Gestión de Reviews** |
| `/api/reviews/AddStudentReview` | POST | ❌ | ❌ | ✅* | ✅* | ❌ |
| `/api/reviews/AddOfferorReview` | POST | ❌ | ✅* | ❌ | ❌ | ❌ |
| `/api/reviews/GetReviews/{offerorId}` | GET | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/reviews/GetAverage/{offerorId}` | GET | ✅ | ✅ | ✅ | ✅ | ✅ |
| `/api/reviews/DeleteReviewPart` | DELETE | ❌ | ❌ | ❌ | ❌ | ✅ |

**Leyenda:**
- ✅ = Acceso completo
- ⚠️ = Acceso limitado (información básica sin datos sensibles)
- ❌ = Sin acceso (401 Unauthorized o 403 Forbidden)
- ✅* = Solo si es dueño del recurso (el oferente/estudiante específico de esa publicación)

---

## 📋 Reviews - Sistema de Calificaciones

Las reviews permiten que estudiantes y oferentes se califiquen mutuamente después de completar un trabajo. Cada review tiene dos partes independientes:
- **Review del Estudiante → Oferente** (`AddOfferorReview`): El **estudiante** califica al oferente (rating, comentario, puntualidad y presentación)
- **Review del Oferente → Estudiante** (`AddStudentReview`): El **oferente** califica al estudiante (rating y comentario)

### ⚠️ Aclaración de Nomenclatura

**IMPORTANTE:** Los nombres de los endpoints pueden parecer contraintuitivos, pero siguen esta lógica:

| Endpoint | Quién lo usa | A quién califica | Descripción |
|----------|--------------|------------------|-------------|
| `AddStudentReview` | **Oferente** (Empresa/Particular) | **Estudiante** | El oferente agrega su calificación **hacia** el estudiante |
| `AddOfferorReview` | **Estudiante** | **Oferente** | El estudiante agrega su calificación **hacia** el oferente |

**Ejemplo práctico:**
```
📊 Publicación #1: "Desarrollador Backend"
👤 Oferente: Empresa TechCorp (ID: 3)
👨‍🎓 Estudiante: Juan Pérez (ID: 5)

Después del trabajo:
1. Juan (estudiante) llama a POST /AddOfferorReview → Califica a TechCorp
2. TechCorp (oferente) llama a POST /AddStudentReview → Califica a Juan
```

---

### 1️⃣ Crear Review Inicial

**Endpoint:** `POST /api/reviews/AddInitialReview`  
**Autorización:** Requerida  
**Descripción:** Crea una review inicial vacía vinculada a una publicación. Esta review será completada posteriormente por ambas partes.

**Request Body:**
```json
{
  "publicationId": 1,
  "studentId": 5,
  "offerorId": 3,
  "reviewWindowEndDate": "2025-12-31T23:59:59Z"
}
```

**Response:** `200 OK`
```json
"Initial review added successfully"
```

---

### 2️⃣ Agregar Review hacia el Estudiante (por el Oferente)

**Endpoint:** `POST /api/reviews/AddStudentReview`  
**Autorización:** `[Authorize(Roles = "Offerent")]` - Solo Empresas y Particulares  
**Descripción:** El **oferente** califica al **estudiante** con quien trabajó.

**Validaciones de Seguridad:**
- ✅ Solo usuarios con rol "Offerent" (Empresa o Particular)
- ✅ Solo el oferente específico de esa publicación puede dejar la review
- ✅ No se puede duplicar: valida que no haya completado ya su review

**Request Body:**
```json
{
  "publicationId": 1,
  "ratingForStudent": 4,
  "commentForStudent": "Buen trabajo, responsable y cumplió con las expectativas.",
  "sendedAt": "2025-11-03T10:30:00Z"
}
```

**Campos:**
- `publicationId` (int, requerido): ID de la publicación asociada
- `ratingForStudent` (int, 1-6): Calificación del estudiante
- `commentForStudent` (string, requerido, max 320 chars): Comentario sobre el estudiante
- `sendedAt` (DateTime, requerido): Fecha de envío de la review

**Responses:**

`200 OK`
```json
"Student review added successfully"
```

`401 Unauthorized`
```json
"No se pudo identificar al usuario autenticado."
```

`403 Forbidden`
```json
"Solo el oferente de esta publicación puede dejar una review hacia el estudiante."
```

`400 Bad Request`
```json
"Ya has completado tu review para este estudiante."
```

`404 Not Found`
```json
"No se ha encontrado una reseña para el ID de publicación dado."
```

---

### 3️⃣ Agregar Review hacia el Oferente (por el Estudiante)

**Endpoint:** `POST /api/reviews/AddOfferorReview`  
**Autorización:** `[Authorize(Roles = "Applicant")]` - Solo Estudiantes  
**Descripción:** El **estudiante** califica al **oferente** con quien trabajó.

**Validaciones de Seguridad:**
- ✅ Solo usuarios con rol "Applicant" (Estudiante)
- ✅ Solo el estudiante específico de esa publicación puede dejar la review
- ✅ No se puede duplicar: valida que no haya completado ya su review

**Request Body:**
```json
{
  "publicationId": 1,
  "ratingForOfferor": 5,
  "commentForOfferor": "Excelente experiencia, muy profesional y puntual en los pagos.",
  "sendedAt": "2025-11-03T10:30:00Z",
  "atTime": true,
  "goodPresentation": true
}
```

**Campos:**
- `publicationId` (int, requerido): ID de la publicación asociada
- `ratingForOfferor` (int, 1-6): Calificación del oferente
- `commentForOfferor` (string, requerido, max 320 chars): Comentario sobre el oferente
- `sendedAt` (DateTime, requerido): Fecha de envío de la review
- `atTime` (bool): Si el oferente cumplió con los horarios acordados
- `goodPresentation` (bool): Si el oferente fue profesional y organizado

**Responses:**

`200 OK`
```json
"Offeror review added successfully"
```

`401 Unauthorized`
```json
"No se pudo identificar al usuario autenticado."
```

`403 Forbidden`
```json
"Solo el estudiante de esta publicación puede dejar una review hacia el oferente."
```

`400 Bad Request`
```json
"Ya has completado tu review para este oferente."
```

`404 Not Found`
```json
"No se ha encontrado una reseña para el ID de publicación dado."
```

---

### 4️⃣ Obtener Reviews de un Oferente

**Endpoint:** `GET /api/reviews/GetReviews/{offerorId}`  
**Autorización:** Pública  
**Descripción:** Obtiene todas las reviews recibidas por un oferente.

**Response:** `200 OK`
```json
[
  {
    "id": 1,
    "ratingForStudent": 4,
    "commentForStudent": "Buen trabajo...",
    "ratingForOfferor": 5,
    "commentForOfferor": "Excelente experiencia...",
    "atTime": true,
    "goodPresentation": true,
    "studentId": 5,
    "offerorId": 3,
    "publicationId": 1,
    "isCompleted": true
  }
]
```

---

### 5️⃣ Obtener Promedio de Calificación

**Endpoint:** `GET /api/reviews/GetAverage/{offerorId}`  
**Autorización:** Pública  
**Descripción:** Calcula el promedio de todas las calificaciones recibidas por un oferente.

**Response:** `200 OK`
```json
4.7
```

---

### 6️⃣ Eliminar Partes de una Review (Solo Admin)

**Endpoint:** `DELETE /api/reviews/DeleteReviewPart`  
**Autorización:** `[Authorize(Roles = "Admin")]` - Solo Administradores  
**Descripción:** Permite a un administrador eliminar selectivamente partes de una review (estudiante, oferente, o ambas). Útil para moderar reviews falsas o inapropiadas sin eliminar toda la review.

**Headers Requeridos:**
```
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "reviewId": 1,
  "deleteStudentPart": true,
  "deleteOfferorPart": false
}
```

**Campos:**
- `reviewId` (int, requerido): ID de la review a modificar
- `deleteStudentPart` (bool): Si es `true`, elimina la review del estudiante hacia el oferente
- `deleteOfferorPart` (bool): Si es `true`, elimina la review del oferente hacia el estudiante
- Al menos uno de los dos booleanos debe ser `true`

**Casos de Uso:**

**Ejemplo 1: Eliminar solo la review del estudiante (review falsa)**
```json
{
  "reviewId": 1,
  "deleteStudentPart": true,
  "deleteOfferorPart": false
}
```
**Efecto:**
- ❌ Elimina: `RatingForOfferor`, `CommentForOfferor`, `AtTime`, `GoodPresentation`
- ✅ Mantiene: `RatingForStudent`, `CommentForStudent`
- Marca `IsReviewForStudentCompleted = false`
- No afecta las métricas del oferente

**Ejemplo 2: Eliminar solo la review del oferente**
```json
{
  "reviewId": 1,
  "deleteStudentPart": false,
  "deleteOfferorPart": true
}
```
**Efecto:**
- ❌ Elimina: `RatingForStudent`, `CommentForStudent`
- ✅ Mantiene: `RatingForOfferor`, `CommentForOfferor`, `AtTime`, `GoodPresentation`
- Marca `IsReviewForOfferorCompleted = false`

**Ejemplo 3: Eliminar toda la review (ambas partes)**
```json
{
  "reviewId": 1,
  "deleteStudentPart": true,
  "deleteOfferorPart": true
}
```
**Efecto:**
- ❌ Elimina todas las calificaciones y comentarios
- Marca `IsCompleted = false`
- La review queda vacía pero sigue existiendo en la BD

**Responses:**

`200 OK`
```json
"Review part(s) deleted successfully"
```

`400 Bad Request`
```json
"Debe especificar al menos una parte de la review para eliminar."
```

`401 Unauthorized`
```json
{
  "message": "No autorizado. Token inválido o no proporcionado."
}
```

`403 Forbidden`
```json
{
  "message": "Acceso denegado. Solo administradores pueden realizar esta acción."
}
```

`404 Not Found`
```json
"No se encontró una review con ID 1."
```

---

### 🔐 Notas de Seguridad

1. **Solo Administradores:** Este endpoint está protegido con `[Authorize(Roles = "Admin")]`
2. **Token JWT Requerido:** Debe incluir un token válido de un usuario con rol "Admin" en el header Authorization
3. **Validaciones:** El sistema valida que al menos una parte debe ser eliminada
4. **Integridad:** La review permanece en la base de datos pero con campos nulos
5. **Métricas:** Al eliminar la review del estudiante, no afecta las estadísticas del oferente

---

### 🎓 Información Visible Según Tipo de Usuario

#### Endpoint: `GET /api/publications/offers/{id}`

**Como Estudiante Autenticado:**
```json
{
  "id": 1,
  "title": "Desarrollador Backend",
  "description": "Descripción completa con detalles...",  ✅
  "companyName": "Tech Corp SpA",
  "location": "Antofagasta",
  "postDate": "2025-10-17T10:00:00Z",
  "endDate": "2025-12-31T23:59:59Z",
  "remuneration": 1500000,  ✅
  "offerType": "Trabajo"
}
```

**Como Empresa / Particular / Admin / Anónimo:**
```json
{
  "id": 1,
  "title": "Desarrollador Backend",
  "companyName": "Tech Corp SpA",
  "location": "Antofagasta",
  "postDate": "2025-10-17T10:00:00Z",
  "endDate": "2025-12-31T23:59:59Z",
  "offerType": "Trabajo",
  "message": "⚠️ Debes ser estudiante y estar autenticado para ver descripción completa, requisitos y remuneración"
}
```

**Campos Ocultos para No-Estudiantes:**
- ❌ `description`
- ❌ `remuneration`
- ❌ `requirements`
- ❌ `contactInfo`

---

### 🛡️ Validaciones de Seguridad Implementadas

#### Postulaciones (Apply to Offer)

1. **Autorización por Atributo:**
   ```csharp
   [Authorize(Roles = "Applicant")]
   ```

2. **Validación Doble de UserType:**
   ```csharp
   if (currentUser.UserType != UserType.Estudiante)
   {
       return Forbid(); // 403
   }
   ```

3. **Validaciones de Negocio:**
   - ✅ Usuario no baneado
   - ✅ Oferta activa
   - ✅ Dentro de deadline
   - ✅ No postulado previamente
   - ✅ CV subido

#### Ver Detalles de Oferta

1. **Detección de Tipo de Usuario:**
   ```csharp
   var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
   bool isStudent = currentUser?.UserType == UserType.Estudiante;
   ```

2. **Respuesta Condicional:**
   - Si `isStudent == true` → Información completa
   - Si `isStudent == false` → Información básica + mensaje

#### Gestión de Postulaciones (Empresas)

1. **Validación de Propiedad:**
   ```csharp
   if (offer.UserId != currentUserId)
   {
       return Unauthorized(); // 401
   }
   ```

2. **Solo Offerents:**
   - Empresas y Particulares pueden ver sus postulaciones
   - Estudiantes y Admins no tienen acceso

---

### 📚 Documentación Relacionada

Para información completa sobre testing, credenciales y ejemplos:

- **📖 Testing Completo:** [`POSTMAN_TESTING_GUIDE.md`](./POSTMAN_TESTING_GUIDE.md)
- **🔑 Credenciales de Prueba:** [`TEST_CREDENTIALS.md`](./TEST_CREDENTIALS.md)
- **⚡ Referencia Rápida:** [`QUICK_REFERENCE.md`](./QUICK_REFERENCE.md)
- **🔄 Cambios de Permisos:** [`CAMBIOS_PERMISOS.md`](./CAMBIOS_PERMISOS.md)
- **🔧 Configuración Cloudinary:** [`CLOUDINARY_SETUP.md`](./CLOUDINARY_SETUP.md)

---

**📅 Última actualización:** 17 de Octubre 2025  
**🔢 Versión API:** 1.0  
**👨‍💻 Desarrollado con:** ASP.NET Core 9.0 + PostgreSQL + Entity Framework Core
