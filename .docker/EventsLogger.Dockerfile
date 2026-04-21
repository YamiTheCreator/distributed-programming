# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ValuatorLib/ValuatorLib.csproj", "ValuatorLib/"]
COPY ["EventsLogger/EventsLogger.csproj", "EventsLogger/"]
RUN dotnet restore "EventsLogger/EventsLogger.csproj"

COPY ValuatorLib/ ValuatorLib/
COPY EventsLogger/ EventsLogger/
WORKDIR "/src/EventsLogger"
RUN dotnet build "EventsLogger.csproj" -c Release -o /app/build
RUN dotnet publish "EventsLogger.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventsLogger.dll"]
