# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["EmergencyComm.Api/EmergencyComm.Api.csproj", "EmergencyComm.Api/"]
RUN dotnet restore "EmergencyComm.Api/EmergencyComm.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/EmergencyComm.Api"
RUN dotnet build "EmergencyComm.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "EmergencyComm.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/emergency_comm.db"

ENTRYPOINT ["dotnet", "EmergencyComm.Api.dll"]