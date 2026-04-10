#!/bin/sh
set -e

SCHEDULE_FILE="/app/config/cron-schedule"
DEFAULT_SCHEDULE="0 2 * * *"

if [ -f "$SCHEDULE_FILE" ]; then
    SCHEDULE=$(head -1 "$SCHEDULE_FILE" | tr -d '\r\n')
else
    SCHEDULE="$DEFAULT_SCHEDULE"
fi

if [ -z "$SCHEDULE" ]; then
    SCHEDULE="$DEFAULT_SCHEDULE"
fi

echo "Configuring cron schedule: $SCHEDULE"
echo "$SCHEDULE cd /app && dotnet BitJuice.Backup.dll execute >> /proc/1/fd/1 2>&1" | crontab -
