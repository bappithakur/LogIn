#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Login.Microservice", "Login.Microservice/"]
COPY ["Libraries", "Libraries/"]
COPY ["Utilities", "Utilities/"]
COPY ["Database", "Database/"]
RUN dotnet restore "Login.Microservice/Login.Microservice.csproj"
WORKDIR "/src/Login.Microservice"
RUN dotnet build "Login.Microservice.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Login.Microservice.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM publish AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Login.Microservice.dll"]