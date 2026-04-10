# BitJuice Backup

A modular, extensible backup utility built with .NET. Define flexible backup workflows using YAML configuration with support for multiple data sources, transformations, and storage destinations. Run on a schedule via Docker with cron, or execute manually from the command line.

## Architecture

The project follows a plugin-based architecture with dynamic module discovery:

```
Workflow
  ├─ Pre-actions     (IAction)       → shell commands, Docker operations
  ├─ Providers        (IProvider)     → read data from sources
  ├─ Aggregator       (IAggregator)   → transform/archive data
  ├─ Storage          (IStorage)      → write data to destinations
  └─ Post-actions     (IAction)       → shell commands, Docker operations
```

### Projects

| Project | Description |
|---|---|
| `BitJuice.Backup` | CLI entry point |
| `BitJuice.Backup.Core` | Workflow orchestration, module discovery and factory |
| `BitJuice.Backup.Model` | Interfaces (`IProvider`, `IStorage`, `IAction`, `IAggregator`, `IWorkflow`) |
| `BitJuice.Backup.Infrastructure` | Base classes and attributes |
| `BitJuice.Backup.Modules` | All built-in module implementations |

## Available Modules

### Providers (Data Sources)

| Module | Description |
|---|---|
| `directory-provider` | Read files from filesystem directories with path rewriting and exclusions |
| `github-provider` | Export GitHub repositories (anonymous, token, or OAuth authentication) |

### Storages (Destinations)

| Module | Description |
|---|---|
| `file-storage` | Write to local filesystem |
| `google-drive-storage` | Upload to Google Drive (user OAuth or service account) |

### Aggregators (Transformations)

| Module | Description |
|---|---|
| `tgz-aggregator` | Create `.tar.gz` archives |
| `tar-aggregator` | Create `.tar` archives |
| `zip-aggregator` | Create `.zip` archives |

### Actions (Pre/Post)

| Module | Description |
|---|---|
| `shell-action` | Execute shell commands |
| `docker-action` | Start/stop Docker containers by name pattern |

### Workflows

| Module | Description |
|---|---|
| `streaming-workflow` | Single-pass workflow: providers → aggregator → storage |
| `multi-workflow` | Run multiple streaming workflows sequentially |

## Configuration

Workflows are defined in YAML. Each module is specified with a `module` key.

### Single Workflow

```yaml
module: streaming-workflow
description: Backup home directories
providers:
  - module: directory-provider
    paths:
      - /home/user
    rewrites:
      - from: /home
        to: /users
aggregator:
  module: tgz-aggregator
  filename: backup.tar.gz
storage:
  module: file-storage
  path: /backup
```

### Multiple Workflows

```yaml
module: multi-workflow
workflows:
  - module: streaming-workflow
    description: Backup to local storage
    providers:
      - module: directory-provider
        paths:
          - /home/user
    aggregator:
      module: tgz-aggregator
      filename: backup.tar.gz
    storage:
      module: file-storage
      path: /backup
  - module: streaming-workflow
    description: Backup to Google Drive
    providers:
      - module: directory-provider
        paths:
          - /home/user
    aggregator:
      module: tgz-aggregator
      filename: backup.tar.gz
    storage:
      module: google-drive-storage
      folderId: <your-folder-id>
      credentialsFile: config/credentials.json
```

### Pre/Post Actions

```yaml
module: streaming-workflow
pre-actions:
  - module: docker-action
    command: stop
    containerName: "my-app-*"
post-actions:
  - module: docker-action
    command: start
    containerName: "my-app-*"
  - module: shell-action
    command: /usr/bin/notify-send
    arguments: "Backup completed"
providers:
  # ...
```

## Usage

### CLI Commands

```bash
# Execute a workflow from a config file
bitjuice-backup execute -c config/workflow.yml

# Install/uninstall cron job (Linux)
bitjuice-backup cron install
bitjuice-backup cron uninstall

# Self-update from GitHub releases
bitjuice-backup update
```

### Docker

The recommended way to run BitJuice Backup is via Docker with cron scheduling.

```yaml
# docker-compose.yml
services:
  backup:
    image: jaroslawdutka/bitjuice-backup:latest
    container_name: bitjuice-backup
    restart: unless-stopped
    environment:
      - BACKUP_CRON_SCHEDULE=0 2 * * *
      - BACKUP_WORKFLOW_FILE=config/workflow.yml
    volumes:
      - ./config:/app/config
      - ./logs:/app/logs
```

**Environment variables:**

| Variable | Default | Description |
|---|---|---|
| `BACKUP_CRON_SCHEDULE` | `0 2 * * *` | Cron expression for scheduling backups |
| `BACKUP_WORKFLOW_FILE` | `config/workflow.yml` | Path to the workflow configuration file |

The cron schedule can also be set via a `config/cron-schedule` file mounted into the container.

```bash
docker compose up -d
```

## Building

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build

```bash
cd src
dotnet build
```

### Publish

Framework-dependent (requires .NET runtime on target):

```bash
# Linux / macOS
./publish.sh

# Windows
publish.cmd
```

Self-contained (no runtime needed):

```bash
publish-selfcontained.cmd
```

Outputs are placed in `publish/` with platform-specific subdirectories (`win-x64`, `linux-x64`, `osx-x64`, `osx-arm64`).

### Docker Build

```bash
docker build -t bitjuice-backup .
```

## License

See [LICENSE](LICENSE) for details.
