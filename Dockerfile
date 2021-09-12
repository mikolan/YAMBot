FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
COPY * build/
WORKDIR /build
RUN dotnet publish --configuration Release

FROM mcr.microsoft.com/dotnet/runtime:5.0
COPY --from=build /build/bin/Release/net5.0/publish app/
WORKDIR /app
ENTRYPOINT dotnet YAMBot.dll