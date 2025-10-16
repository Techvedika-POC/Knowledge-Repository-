# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the .csproj and restore
COPY ["KnowLedger Synaptix.csproj", "./"]
RUN dotnet restore "KnowLedger Synaptix.csproj"

# Copy all source files
COPY . .

# Build and publish
RUN dotnet publish "KnowLedger Synaptix.csproj" -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "KnowLedger Synaptix.dll"]
