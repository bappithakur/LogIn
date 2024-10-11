# Base image for building the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the project files
COPY ["Login.Microservice/Login.Microservice.csproj", "Login.Microservice/"]
RUN dotnet restore "Login.Microservice/Login.Microservice.csproj"

# Copy all the necessary directories
COPY . .

# Build the project
WORKDIR "/src/Login.Microservice"
RUN dotnet build "Login.Microservice.csproj" -c Release -o /app/build

# Publish the project
FROM build AS publish
RUN dotnet publish "Login.Microservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Login.Microservice.dll"]
