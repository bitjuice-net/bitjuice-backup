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

# Strip CR/LF to prevent cron entry injection
SCHEDULE=$(printf '%s' "$SCHEDULE" | tr -d '\r\n')

# Validate cron expression format (5 fields: min hour dom mon dow)
if ! echo "$SCHEDULE" | grep -Eq '^(\S+\s+){4}\S+$'; then
    echo "ERROR: Invalid cron schedule: $SCHEDULE" >&2
    exit 1
fi

ENV_VARS=""
if [ -n "$BACKUP_WORKFLOW_FILE" ]; then
    # Strip CR/LF and validate path contains no shell metacharacters
    WORKFLOW_FILE=$(printf '%s' "$BACKUP_WORKFLOW_FILE" | tr -d '\r\n')
    case "$WORKFLOW_FILE" in
        *[\'\"\\\ \;\&\|\$\`\(\)\{\}\<\>\!]*)
            echo "ERROR: BACKUP_WORKFLOW_FILE contains unsafe characters: $WORKFLOW_FILE" >&2
            exit 1
            ;;
    esac
    ENV_VARS="workflow__file='${WORKFLOW_FILE}' "
fi

echo "Configuring cron schedule: $SCHEDULE"
echo "$SCHEDULE cd /app && ${ENV_VARS}dotnet BitJuice.Backup.dll execute >> /proc/1/fd/1 2>&1" | crontab -
