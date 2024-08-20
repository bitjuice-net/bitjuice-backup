dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish/linux-x64-sc src\BitJuice.Backup\BitJuice.Backup.csproj
dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true -o publish/linux-x64 src\BitJuice.Backup\BitJuice.Backup.csproj

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64-sc src\BitJuice.Backup\BitJuice.Backup.csproj
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish/win-x64 src\BitJuice.Backup\BitJuice.Backup.csproj