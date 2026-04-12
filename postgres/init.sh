#!/bin/bash
set -e

psql -v ON_ERROR_STOP=0 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE crm_identity;
    CREATE DATABASE crm_clients;
    CREATE DATABASE crm_deals;
    CREATE DATABASE crm_tasks;
    CREATE DATABASE crm_notifications;
    CREATE DATABASE crm_documents;
    CREATE DATABASE crm_analytics;
EOSQL
