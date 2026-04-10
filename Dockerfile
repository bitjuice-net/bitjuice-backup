FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY src/ .
RUN dotnet publish "./BitJuice.Backup/BitJuice.Backup.csproj" -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine  AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "BitJuice.Backup.dll"]