# 🧩 Bolsa FEUCN - Backend

Backend del proyecto **Bolsa FEUCN**, desarrollado con **.NET 9.0** y **Entity Framework Core**, conectado a **PostgreSQL**.  
Incluye una arquitectura limpia basada en capas (Controllers, Services, Repositories, etc.).

## 🚀 Tecnologías utilizadas

- **.NET 9.0**
- **Entity Framework Core**
- **PostgreSQL 16**
- **Docker & Docker Compose**
- **Dependency Injection (DI)**
- **Data Transfer Objects (DTOs)**
- **Repository Pattern**
- **LINQ / Async/Await**
- **Visual Studio / VS Code**

## 🛠️ Configuración inicial

### Requisitos previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

### 1️⃣ Clonar el repositorio

```bash
git clone https://github.com/ProMDFK123/backend-PIS.git
cd backend-PIS
```

### 2️⃣ Configurar la base de datos con Docker

#### Iniciar PostgreSQL con Docker Compose

```bash
docker-compose up -d
```

#### Verificar que el contenedor esté corriendo

```bash
docker ps
```

#### Comandos Make
De forma alternativa, tambien puedes crear el contenedor con el comando
```bash
Make docker-create
```
Una vez se crea, se debe correr con el comando
```bash
Make docker-start
```


Deberías ver un contenedor llamado `bolsafeucn-container` en estado `Up`.

#### Credenciales de desarrollo

- **Host:** localhost
- **Puerto:** 5432
- **Base de datos:** bolsafeucn-db
- **Usuario:** bolsafeucn-user
- **Contraseña:** bolsafeucn-password

### 3️⃣ Configurar appsettings (si es necesario)

El archivo `appsettings.Development.json` ya está configurado para usar Docker.  
Para otros entornos, copia `appsettings.Example.json` y configura según sea necesario:

```bash
cd bolsafeucn_back
cp appsettings.Example.json appsettings.Production.json
# Edita appsettings.Production.json con tus credenciales
```
Configuracion para la subida de archivos
```bash
"Storage":{
		"Provider": "Local",
		"LocalPath": "uploads",
		"BaseUrl": "/uploads",
		"FileAllowedExtensions": [
			".pdf"
		],
		"MaxFileSizeMB": 10
	},
```

### 4️⃣ Restaurar dependencias

```bash
cd bolsafeucn_back
dotnet restore
```

## 🧩 Migraciones de base de datos

### Aplicar migraciones existentes

```bash
dotnet ef database update
```

### Crear una nueva migración

```bash
dotnet ef migrations add NombreDeLaMigracion
dotnet ef database update
```

### Revertir migraciones

```bash
dotnet ef database update NombreMigracionAnterior
```

## 🧪 Ejecución del proyecto

### Ejecutar en modo desarrollo

```bash
dotnet run
```

El servidor estará disponible en:

- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### Ejecutar con recarga automática (watch)

```bash
dotnet watch run
```

## 🔧 Comandos útiles de Makefile


El proyecto incluye un `Makefile` con atajos para tareas comunes de desarrollo.

Es posible que para los usuarios de Windows no les funcione los comandos Make, para eso deben instalar el gestor de paquetes Chocolatey y ejecutar el siguiente comando:
```bash
choco install make
```

### Comandos de desarrollo

```bash
# Reiniciar la base de datos (drop, update) y ejecutar watch
make db-restart

# Ejecutar el proyecto en modo normal
make run

# Ejecutar el proyecto con recarga automática (sin hot-reload)
make watch

# Mostrar ayuda con todos los comandos disponibles
make help
```

### Comandos de Docker (PostgreSQL)

```bash
# Crear y ejecutar un nuevo contenedor PostgreSQL
make docker-create

# Detener y eliminar el contenedor PostgreSQL
make docker-rm

# Iniciar contenedor PostgreSQL existente
make docker-start

# Detener contenedor PostgreSQL
make docker-stop
```

> **Nota:** Los comandos `make docker-*` usan las credenciales configuradas en `appsettings.json` automáticamente. Para utilizar otras credenciales, puedes forzar sus valores en el archivo Makefile, o bien reemplazando la direccion del appsettings definido en el Makefile por el appsettings utilizado en el ambiente de desarollo.

## 🐳 Comandos útiles de Docker

### Detener la base de datos

```bash
docker-compose down
```

### Ver logs del contenedor

```bash
docker logs bolsafeucn-container
```

### Acceder a PostgreSQL desde la terminal

```bash
docker exec -it bolsafeucn-container psql -U bolsafeucn-user -d bolsafeucn-db
```

### Reiniciar la base de datos (eliminar todos los datos)

```bash
docker-compose down -v
docker-compose up -d
dotnet ef database update
```

### Listar volúmenes de Docker

```bash
docker volume ls
```

## 📂 Estructura del proyecto

```
backend-PIS/
├── bolsafeucn_back/
│   ├── src/
│   │   ├── API/
│   │   │   ├── Controllers/
│   │   │   └── Middlewares/
│   │   ├── Application/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   └── Validators/
│   │   ├── Domain/
│   │   │   └── Models/
│   │   └── Infrastructure/
│   │       ├── Data/
│   │       └── Repositories/
│   ├── Migrations/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
├── docker-compose.yml
└── README.md
```

## 📦 Modelo de base de datos

El modelo de base de datos (en formato Visual Paradigm .vpp) se encuentra en:  
`/database model/PIS.vpp`

## 🔒 Seguridad

- ⚠️ **NO** subas archivos `appsettings.Development.json` o `appsettings.Production.json` al repositorio
- Las credenciales de desarrollo en Docker son solo para entorno local
- Para producción, usa variables de entorno o servicios de secretos

## 📝 Endpoints de la API

Consulta la documentación completa de endpoints en:  
`/bolsafeucn_back/API_ENDPOINTS.md`

## Uso de Resend

Consulta la documentacion completa para el uso correcto de Resend con el plan gratuito en:
`/bolsafeucn_back/RESEND_README.md`

## 🧠 Autores

Estudiantes de Proyecto Integrador Software II-2025  
Proyecto académico - Universidad Católica del Norte  
Facultad de Ingeniería y Ciencias Geológicas

## 📄 Licencia

Este proyecto es de uso académico.
