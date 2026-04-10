FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY src/ .
RUN dotnet publish "./BitJuice.Backup/BitJuice.Backup.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

RUN apk add --no-cache curl icu-libs xz

ARG S6_OVERLAY_VERSION=3.2.0.2
RUN curl -fsSL https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-noarch.tar.xz | tar -C / -Jxpf - && \
    curl -fsSL https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-x86_64.tar.xz | tar -C / -Jxpf -

COPY docker/root/ /
RUN chmod +x /etc/s6-overlay/scripts/init-cron.sh \
    && find /etc/s6-overlay/s6-rc.d -type f -exec sed -i 's/\r$//' {} \; \
    && sed -i 's/\r$//' /etc/s6-overlay/scripts/init-cron.sh

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["/init"]