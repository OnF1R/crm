#!/bin/sh
set -eu

PGHOST="${PGHOST:-postgres}"
PGUSER="${PGUSER:-crm_admin}"
PGPASSWORD="${PGPASSWORD:-crm_secret_2026}"
SEED_NAME="${SEED_NAME:-seed-test-data}"
SEED_VERSION="${SEED_VERSION:-v1}"
SEED_FORCE="${SEED_FORCE:-0}"
SEED_SQL_PATH="${SEED_SQL_PATH:-/seed/seed-test-data.sql}"
MAX_RETRIES="${SEED_MAX_RETRIES:-90}"
RETRY_DELAY_SECONDS="${SEED_RETRY_DELAY:-2}"
SEED_MARKER_DB="${SEED_MARKER_DB:-postgres}"

export PGPASSWORD

ensure_seed_marker() {
  psql -h "$PGHOST" -U "$PGUSER" -d "$SEED_MARKER_DB" -v ON_ERROR_STOP=1 -c "
    CREATE TABLE IF NOT EXISTS public.seed_runs (
      seed_name text NOT NULL,
      seed_version text NOT NULL,
      applied_at timestamptz NOT NULL DEFAULT now(),
      CONSTRAINT seed_runs_pkey PRIMARY KEY (seed_name, seed_version)
    );
"
}

is_seed_applied() {
  psql -h "$PGHOST" -U "$PGUSER" -d "$SEED_MARKER_DB" -Atq -c "
    SELECT EXISTS (
      SELECT 1
      FROM public.seed_runs
      WHERE seed_name = '${SEED_NAME}' AND seed_version = '${SEED_VERSION}'
    );
  " | grep -q '^t$'
}

mark_seed_applied() {
  psql -h "$PGHOST" -U "$PGUSER" -d "$SEED_MARKER_DB" -v ON_ERROR_STOP=1 -c "
    INSERT INTO public.seed_runs (seed_name, seed_version, applied_at)
    VALUES ('${SEED_NAME}', '${SEED_VERSION}', now())
    ON CONFLICT (seed_name, seed_version)
    DO UPDATE SET applied_at = now();
"
}

require_table() {
  db="$1"
  table="$2"
  attempt=0
  query="SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$table'"

  while [ "$attempt" -lt "$MAX_RETRIES" ]; do
    if psql -h "$PGHOST" -U "$PGUSER" -d "$db" -Atq -c "$query" | grep -q "^1$"
    then
      echo "[seed] ${db}.${table} is ready"
      return 0
    fi

    attempt=$((attempt + 1))
    echo "[seed] waiting for ${db}.${table} ... (${attempt}/${MAX_RETRIES})"
    sleep "$RETRY_DELAY_SECONDS"
  done

  echo "[seed] timeout waiting for ${db}.${table}" >&2
  return 1
}

ensure_seed_marker

if [ "$SEED_FORCE" != "1" ] && is_seed_applied; then
  echo "[seed] marker found: ${SEED_NAME}@${SEED_VERSION} already applied, skipping"
  exit 0
fi

if [ "$SEED_FORCE" != "1" ]; then
  echo "[seed] no marker for ${SEED_NAME}@${SEED_VERSION}, applying once"
fi

require_table crm_identity users
require_table crm_clients clients
require_table crm_deals deals
require_table crm_tasks tasks
require_table crm_documents documents
require_table crm_notifications notifications
require_table crm_analytics dashboard_widgets
require_table crm_audit audit_logs

echo "[seed] running seed SQL..."
psql -h "$PGHOST" -U "$PGUSER" -d postgres -v ON_ERROR_STOP=1 -f "$SEED_SQL_PATH"
mark_seed_applied
echo "[seed] done"
