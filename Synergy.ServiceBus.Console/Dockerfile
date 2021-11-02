FROM microsoft/dotnet:sdk AS build-env
WORKDIR /src

COPY . .
WORKDIR /src/Synergy.ServiceBus.Console
RUN dotnet restore
RUN dotnet publish --no-restore -c Release -o /app

FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app .
ENTRYPOINT ["dotnet", "Synergy.ServiceBus.Console.dll"]