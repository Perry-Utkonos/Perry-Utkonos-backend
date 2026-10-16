# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/ChatApp.Domain/ChatApp.Domain.csproj src/ChatApp.Domain/
COPY src/ChatApp.Application/ChatApp.Application.csproj src/ChatApp.Application/
COPY src/ChatApp.Infrastructure/ChatApp.Infrastructure.csproj src/ChatApp.Infrastructure/
COPY src/ChatApp.Api/ChatApp.Api.csproj src/ChatApp.Api/

RUN dotnet restore src/ChatApp.Api/ChatApp.Api.csproj

COPY src/ src/
RUN dotnet publish src/ChatApp.Api/ChatApp.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "ChatApp.Api.dll"]
