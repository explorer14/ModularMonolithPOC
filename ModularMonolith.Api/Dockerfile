FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 1983
#EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ModularMonolith.Api/ModularMonolith.Api.csproj", "ModularMonolith.Api/"]
COPY ["Greetings/Greetings.Application/Greetings.Application.csproj", "Greetings/Greetings.Application/"]
COPY ["WeatherReporting/WeatherReporting.PublishedInterfaces/WeatherReporting.PublishedInterfaces.csproj", "WeatherReporting/WeatherReporting.PublishedInterfaces/"]
COPY ["Greetings/Greetings.DomainModel/Greetings.DomainModel.csproj", "Greetings/Greetings.DomainModel/"]
COPY ["Greetings/Greetings.PublishedInterfaces/Greetings.PublishedInterfaces.csproj", "Greetings/Greetings.PublishedInterfaces/"]
COPY ["Greetings/Greetings.Storage.Adapter/Greetings.Storage.Adapter.csproj", "Greetings/Greetings.Storage.Adapter/"]
COPY ["Greetings/Greetings.WeatherReportingApi.Adapter/Greetings.WeatherReportingApi.Adapter.csproj", "Greetings/Greetings.WeatherReportingApi.Adapter/"]
COPY ["SharedInfrastructure/SharedInfrastructure.csproj", "SharedInfrastructure/"]
COPY ["WeatherModeling/WeatherModeling.Application/WeatherModeling.Application.csproj", "WeatherModeling/WeatherModeling.Application/"]
COPY ["WeatherModeling/WeatherModeling.Messaging.Adapter/WeatherModeling.Messaging.Adapter.csproj", "WeatherModeling/WeatherModeling.Messaging.Adapter/"]
COPY ["WeatherModeling/WeatherModeling.DomainModel/WeatherModeling.DomainModel.csproj", "WeatherModeling/WeatherModeling.DomainModel/"]
COPY ["WeatherReporting/WeatherReporting.Application/WeatherReporting.Application.csproj", "WeatherReporting/WeatherReporting.Application/"]
COPY ["WeatherReporting/WeatherReporting.DomainModel/WeatherReporting.DomainModel.csproj", "WeatherReporting/WeatherReporting.DomainModel/"]
COPY ["WeatherReporting/WeatherReporting.ExternalSources.Adapter/WeatherReporting.ExternalSources.Adapter.csproj", "WeatherReporting/WeatherReporting.ExternalSources.Adapter/"]
COPY ["WeatherReporting/WeatherReporting.Publishing.Adapter/WeatherReporting.Publishing.Adapter.csproj", "WeatherReporting/WeatherReporting.Publishing.Adapter/"]
RUN dotnet restore "ModularMonolith.Api/ModularMonolith.Api.csproj"
COPY . .
WORKDIR "/src/ModularMonolith.Api"
RUN dotnet build "./ModularMonolith.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ModularMonolith.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ModularMonolith.Api.dll",  "--urls=http://+:1983"]
