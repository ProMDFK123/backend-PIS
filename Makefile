# Makefile para tareas comunes de desarrollo
# - aliases para dotnet ef / watch dentro de `./bolsafeucn_back`
# hice esto con claude pq me da flojera apreneder makefile
DOTNET_DIR := ./bolsafeucn_back
CONTAINER_NAME := bolsafeucn-docker
POSTGRES_USER := bolsafeucn-user
POSTGRES_PASSWORD := bolsafeucn-password
POSTGRES_DB := bolsafeucn-db

.PHONY: help db-restart watch docker-create docker-rm docker-start docker-stop

help:
	@echo "Makefile - targets disponibles:"
	@echo "  make db-restart      -> drop, update DB y ejecutar dotnet watch (no-hot-reload)"
	@echo "  make watch           -> ejecutar dotnet watch (no-hot-reload)"
	@echo "  make docker-create   -> crear y ejecutar contenedor PostgreSQL"
	@echo "  make docker-rm       -> detener y eliminar contenedor PostgreSQL"
	@echo "  make docker-start    -> iniciar contenedor PostgreSQL existente"
	@echo "  make docker-stop     -> detener contenedor PostgreSQL"

# 1) Drop DB, update y lanzar dotnet watch (sin hot reload)
db-restart:
	@echo "Entrando en $(DOTNET_DIR) y reiniciando la BD..."
	cd $(DOTNET_DIR) && \
		dotnet ef database drop --force && \
		dotnet ef database update && \
		dotnet watch --no-hot-reload

# 2) dotnet watch simple
watch:
	@echo "Ejecutando: cd $(DOTNET_DIR) && dotnet watch --no-hot-reload"
	cd $(DOTNET_DIR) && dotnet watch --no-hot-reload

# 3) Crear y ejecutar contenedor PostgreSQL
docker-create:
	@echo "Creando contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker run --name $(CONTAINER_NAME) \
		-e POSTGRES_USER=$(POSTGRES_USER) \
		-e POSTGRES_PASSWORD=$(POSTGRES_PASSWORD) \
		-e POSTGRES_DB=$(POSTGRES_DB) \
		-p 5432:5432 \
		-d postgres

# 4) Detener y eliminar contenedor PostgreSQL (si existe)
docker-rm:
	@echo "Deteniendo y eliminando contenedor PostgreSQL: $(CONTAINER_NAME)"
	-@docker stop $(CONTAINER_NAME) >/dev/null 2>&1 || true
	-@docker rm $(CONTAINER_NAME) >/dev/null 2>&1 || true
	@echo "Contenedor $(CONTAINER_NAME) eliminado (si existía)."

# 5) Iniciar contenedor PostgreSQL existente
docker-start:
	@echo "Iniciando contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker start $(CONTAINER_NAME)

# 6) Detener contenedor PostgreSQL
docker-stop:
	@echo "Deteniendo contenedor PostgreSQL: $(CONTAINER_NAME)"
	docker stop $(CONTAINER_NAME)

