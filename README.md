# 🧩 Bolsa FEUCN - Backend
Backend del proyecto **Bolsa FEUCN**, desarrollado con **.NET 9.0** y **Entity Framework Core**, conectado a **PostgreSQL**.  
Incluye una arquitectura limpia basada en capas (Controllers, Services, Repositories, etc.).

## 🚀 Tecnologías utilizadas
- **.NET 9.0**
- **Entity Framework Core**
- **PostgreSQL**
- **Dependency Injection (DI)**
- **Data Transfer Objects (DTOs)**
- **Repository Pattern**
- **LINQ / Async/Await**
- **Visual Studio / VS Code**

## 🛠️ Configuración inicial
### 1️⃣ Clonar el repositorio
```bash
git clone https://github.com/ProMDFK123/backend-PIS.git
cd backend-PIS/bolsafeucn_back
```
### 2️⃣ Restaurar dependencias
```bash
dotnet restore
```
### 3️⃣ Configurar la conexión a PostgreSQL
Edita el archivo appsettings.json con el formato de appsettings.Example.json

## 🧩 Migraciones de base de datos
Para crear la base de datos y aplicar migraciones:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🧪 Ejecución del proyecto
Ejecuta el backend en modo desarrollo:
```bash
dotnet run
```

## 📦 Repositorio del modelo de base de datos
El modelo de base de datos (en formato Visual Paradigm .vpp) se encuentra en: /database model/PIS.vpp

## 🧠 Autor
Estudiantes de Proyecto Integrador Software II-2025  
Proyecto académico - Universidad Católica del Norte  
Facultad de Ingeniería y Ciencias Geológicas
