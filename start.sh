#!/bin/bash
# Start the full Allocare stack: Docker (PostgreSQL), Backend (.NET), Frontend (Next.js)

set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"

echo "════════════════════════════════════════════"
echo "  Allocare — Starting all services"
echo "════════════════════════════════════════════"
echo ""

# ── 1. Docker (PostgreSQL) ─────────────────────────────────────
echo "[1/3] Starting Docker containers..."
cd "$ROOT"
if ! docker info > /dev/null 2>&1; then
  echo "  ERROR: Docker is not running. Please start Docker Desktop and try again."
  exit 1
fi

docker compose up -d
echo "  Waiting for PostgreSQL to be ready..."
until docker exec allocare-postgres pg_isready -U postgres > /dev/null 2>&1; do
  sleep 1
done
echo "  PostgreSQL ready on localhost:5437"
echo ""

# ── 2. Backend (.NET API) ─────────────────────────────────────
echo "[2/3] Starting Allocore Backend (.NET API)..."
cd "$ROOT/Allocore-backend/src/Allocore.API" && dotnet run --launch-profile http &
BACKEND_PID=$!
echo "  Backend PID: $BACKEND_PID → http://localhost:5103"
echo ""

# ── 3. Frontend (Next.js) ─────────────────────────────────────
echo "[3/3] Starting Allocore Frontend (Next.js)..."
if [ -f "$ROOT/Allocore-frontend/package.json" ]; then
  cd "$ROOT/Allocore-frontend" && npm run dev &
  FRONTEND_PID=$!
  echo "  Frontend PID: $FRONTEND_PID → http://localhost:3000"
else
  FRONTEND_PID=""
  echo "  Frontend not yet set up — skipping."
fi
echo ""

echo "════════════════════════════════════════════"
echo "  All services started!"
echo ""
echo "  PostgreSQL:  localhost:5437"
echo "  Backend:     http://localhost:5103"
echo "  Frontend:    http://localhost:3000"
echo ""
echo "  Press Ctrl+C to stop backend & frontend."
echo "  (Docker containers keep running.)"
echo "════════════════════════════════════════════"

cleanup() {
  echo ""
  echo "Stopping backend & frontend..."
  kill $BACKEND_PID ${FRONTEND_PID:-} 2>/dev/null
  exit
}

trap cleanup INT TERM
wait
