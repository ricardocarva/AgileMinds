# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src

COPY ["AgileMindsUI/AgileMindsUI/AgileMindsUI.csproj", "AgileMindsUI/AgileMindsUI/"]
COPY ["AgileMindsUI/AgileMindsUI.Client/AgileMindsUI.Client.csproj", "AgileMindsUI/AgileMindsUI.Client/"]
COPY ["AgileMinds.Shared/AgileMinds.Shared.csproj", "AgileMinds.Shared/"]

# Restore dependencies for all the projects
RUN dotnet restore "./AgileMindsUI/AgileMindsUI/AgileMindsUI.csproj"

COPY . .
RUN mv /src/AgileMindsWebAPI/appsettings.json /src/AgileMindsWebAPI/appsettings.WebAPI.json

WORKDIR "/src/AgileMindsUI/AgileMindsUI"
RUN dotnet build "./AgileMindsUI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "./AgileMindsUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AgileMindsUI.dll"]