@echo off
setlocal

echo [1/3] Stopping old containers...
docker compose down --remove-orphans

echo [2/3] Starting containers (build if needed)...
docker compose up -d --build

echo [3/3] Status:
docker compose ps

echo.
echo Open: http://localhost:8080
endlocal