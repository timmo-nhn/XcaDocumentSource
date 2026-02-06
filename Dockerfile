FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore XcaXds.WebService/XcaXds.WebService.csproj
RUN dotnet publish XcaXds.WebService/XcaXds.WebService.csproj -c Release -o /app

COPY XcaXds.Source/Registry /app/registry
COPY XcaXds.Source/Repository /app/repository
COPY XcaXds.Source/PolicyRepository /app/policyrepository
COPY XcaXds.Source/AuditEvents /app/auditevents

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Update CA certificates to include latest root certificates
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "XcaXds.WebService.dll"]
