# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /app

# Copy solution and project files first (for better caching)
COPY BeeGameBackend.sln ./
COPY BeeGameApi/BeeGameApi.csproj BeeGameApi/

# Restore dependencies
RUN dotnet restore

# Copy everything and publish
COPY . ./
RUN dotnet publish -c Release -o out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/out .

# Expose the port Render uses
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

ENTRYPOINT ["dotnet", "BeeGameApi.dll"]
