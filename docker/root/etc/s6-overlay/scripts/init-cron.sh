#!/bin/sh
set -e

SCHEDULE_FILE="/app/config/cron-schedule"
DEFAULT_SCHEDULE="0 2 * * *"

# Env variable takes priority, then file, then default
if [ -n "$BACKUP_CRON_SCHEDULE" ]; then
    SCHEDULE="$BACKUP_CRON_SCHEDULE"
elif [ -f "$SCHEDULE_FILE" ]; then
    SCHEDULE=$(head -1 "$SCHEDULE_FILE" | tr -d '\r\n')
fi

if [ -z "$SCHEDULE" ]; then
    SCHEDULE="$DEFAULT_SCHEDULE"
fi

ENV_VARS=""
if [ -n "$BACKUP_WORKFLOW_FILE" ]; then
    ENV_VARS="workflow__file=$BACKUP_WORKFLOW_FILE "
fi

echo "Configuring cron schedule: $SCHEDULE"
echo "$SCHEDULE cd /app && ${ENV_VARS}dotnet BitJuice.Backup.dll execute >> /proc/1/fd/1 2>&1" | crontab -
