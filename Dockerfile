# ----------------------------------------------------------------------
# Etapa de Construcción (build)
# Usa la imagen SDK para compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copia los archivos de proyecto y solución, respetando la estructura de carpetas
# Esto optimiza el uso de la caché de Docker
# Copia la SOLUCIÓN (.sln) a la raíz /src y el PROYECTO (.csproj) a /src/bolsafeucn_back/
COPY ./bolsafeucn_back/*.sln ./
COPY ./bolsafeucn_back/bolsafeucn_back.csproj ./bolsafeucn_back/

# 2. Restaura las dependencias (paquetes NuGet)
# Especificamos la ruta al archivo de proyecto para restaurar
RUN dotnet restore ./bolsafeucn_back/bolsafeucn_back.csproj

# 3. Copia el resto del código fuente
COPY . .

# 4. Publica la aplicación en modo Release
# Cambiamos el directorio de trabajo al directorio del proyecto para ejecutar 'publish'
WORKDIR /src/bolsafeucn_back
RUN dotnet publish -c Release -o /app/publish

# ----------------------------------------------------------------------
# Etapa Final (runtime)
# Usa la imagen de runtime de ASP.NET Core, mucho más ligera para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copia los archivos publicados desde la etapa de construcción
COPY --from=build /app/publish .

# Define el puerto de escucha.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Comando de inicio
ENTRYPOINT ["dotnet", "bolsafeucn_back.dll"]