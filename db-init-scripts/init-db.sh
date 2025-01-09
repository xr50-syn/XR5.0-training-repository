#!/bin/bash

# Path to the SQL file
SQL_FILE="/docker-entrypoint-initdb.d/create-database.sql"

# Check if the SQL file exists
if [ ! -f "$SQL_FILE" ]; then
    echo "SQL file not found: $SQL_FILE"
    exit 1
fi

# Debug: Print environment variables to ensure they are set
echo "XR50_REPO_DB_USER: $XR50_REPO_DB_USER"
echo "XR50_REPO_DB_PASSWORD: $XR50_REPO_DB_PASSWORD"
echo "XR50_REPO_DB_NAME: $XR50_REPO_DB_NAME"


# Process the file in memory and pipe the result directly to the MySQL client
cat "$SQL_FILE" | sed "s|{{XR50_REPO_DB_USER}}|$XR50_REPO_DB_USER|g" \
                | sed "s|{{XR50_REPO_DB_PASSWORD}}|$XR50_REPO_DB_PASSWORD|g" \
                | sed "s|{{XR50_REPO_DB_NAME}}|$XR50_REPO_DB_NAME|g" \
                | mysql -u root -p"$MYSQL_ROOT_PASSWORD"
