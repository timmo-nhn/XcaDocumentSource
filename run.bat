@echo off
:: Build both projects
dotnet build .\XcaXds.WebService\XcaXds.WebService.csproj
dotnet build .\XcaXds.Frontend\XcaXds.Frontend.csproj

:: Run both projects
start dotnet run --project .\XcaXds.WebService\XcaXds.WebService.csproj
start dotnet run --project .\XcaXds.Frontend\XcaXds.Frontend.csproj
