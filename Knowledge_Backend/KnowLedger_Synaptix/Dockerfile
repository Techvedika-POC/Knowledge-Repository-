# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Knowledge_Backend/KnowLedger_Synaptix/KnowLedger Synaptix.csproj", "Knowledge_Backend/KnowLedger_Synaptix/"]

# Restore dependencies
RUN dotnet restore "Knowledge_Backend/KnowLedger_Synaptix/KnowLedger Synaptix.csproj"

# Copy everything
COPY . .

# Build and publish release version
RUN dotnet publish "Knowledge_Backend/KnowLedger_Synaptix/KnowLedger Synaptix.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port your API runs on (default 8080)
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "KnowLedger Synaptix.dll"]
