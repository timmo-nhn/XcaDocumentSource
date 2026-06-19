ARG UPSTREAM_REGISTRY=mcr.microsoft.com

FROM ${UPSTREAM_REGISTRY}/dotnet/sdk:10.0 AS build
ARG NHN_HTTP_PROXY
ARG NHN_NO_PROXY
ENV HTTP_PROXY=${NHN_HTTP_PROXY}
ENV HTTPS_PROXY=${NHN_HTTP_PROXY}
ENV NO_PROXY=${NHN_NO_PROXY}
ENV http_proxy=${NHN_HTTP_PROXY}
ENV https_proxy=${NHN_HTTP_PROXY}
ENV no_proxy=${NHN_NO_PROXY}
WORKDIR /src

COPY . .

RUN dotnet restore XcaXds.WebService/XcaXds.WebService.csproj
RUN dotnet publish XcaXds.WebService/XcaXds.WebService.csproj -c Release -o /app

# If you do not want the local directories to be included in the container,
# comment the following lines and ensure the paths are correct.
# Otherwise, these files can be mounted as volumes at runtime.
COPY XcaXds.Source/Registry /app/registry
COPY XcaXds.Source/Repository /app/repository
COPY XcaXds.Source/PolicyRepository /app/policyrepository
COPY XcaXds.Source/OfflineCodeSystems /app/offlinecodesystems

####################################################################################################
#                                                                                                  #
# If you have several build stages with RUN commands, these stages must include the 'ARG' and 'ENV' #
# settings to ensure the correct proxy settings.                                                   #
#                                                                                                  #
####################################################################################################

FROM ${UPSTREAM_REGISTRY}/dotnet/aspnet:10.0
ARG NHN_HTTP_PROXY
ARG NHN_NO_PROXY
ENV HTTP_PROXY=${NHN_HTTP_PROXY}
ENV HTTPS_PROXY=${NHN_HTTP_PROXY}
ENV NO_PROXY=${NHN_NO_PROXY}
ENV http_proxy=${NHN_HTTP_PROXY}
ENV https_proxy=${NHN_HTTP_PROXY}
ENV no_proxy=${NHN_NO_PROXY}
ENV HOME=/mnt/data/tmp
RUN mkdir -p /mnt/data/tmp/.fhir/packages

# Update CA certificates to include latest root certificates
RUN apt-get update && apt-get install -y sqlite3 ca-certificates curl && update-ca-certificates && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "XcaXds.WebService.dll"]

# Clear proxy settings for the runtime stage; runtime should not use proxy
ENV HTTP_PROXY= HTTPS_PROXY= NO_PROXY= http_proxy= https_proxy= no_proxy=
