# Команды запускать из корня проекта: make dev, make test и т.д.
# Перед запуском должен работать сервер LM Studio (вкладка Developer -> Running).

.PHONY: help install dev backend frontend test reset

help:
	@echo make install  - поставить зависимости бэкенда и фронтенда
	@echo make dev      - запустить бэкенд и фронтенд вместе (остановить: Ctrl+C)
	@echo make backend  - только бэкенд, http://localhost:5080
	@echo make frontend - только фронтенд, http://localhost:5173
	@echo make test     - прогнать тестовые тексты (бэкенд должен быть запущен)
	@echo make reset    - удалить базу (бэкенд должен быть остановлен)

install:
	dotnet restore backend/GlossaryAgent.csproj
	cd frontend && npm install

# -j2 запускает две цели параллельно, иначе make ждал бы, пока завершится первая
dev:
	$(MAKE) -j2 backend frontend

backend:
	cd backend && dotnet run

frontend:
	cd frontend && npm run dev

test:
	powershell -NoProfile -ExecutionPolicy Bypass -File backend/tests/run-samples.ps1

reset:
	powershell -NoProfile -Command "Remove-Item backend/glossary.db* -ErrorAction SilentlyContinue"
