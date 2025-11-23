# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["IMS.sln", "./"]

# Copy project files
COPY ["IMS/IMS.csproj", "IMS/"]
COPY ["IMS.Application/IMS.APPLICATION.csproj", "IMS.Application/"]
COPY ["IMS.COMMON/IMS.COMMON.csproj", "IMS.COMMON/"]
COPY ["IMS.Models/IMS.Models.csproj", "IMS.Models/"]
COPY ["IMS.Repository/IMS.Repository.csproj", "IMS.Repository/"]

# Restore dependencies
RUN dotnet restore "IMS/IMS.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/IMS"
RUN dotnet build "IMS.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create a non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Copy published files
COPY --from=publish --chown=appuser:appuser /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (optional)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "IMS.dll"]