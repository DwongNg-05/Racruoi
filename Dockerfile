FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NuGet.Config ./
COPY HeThongQLTV/HeThongQLTV.csproj HeThongQLTV/
RUN dotnet restore HeThongQLTV/HeThongQLTV.csproj
COPY HeThongQLTV/ HeThongQLTV/
RUN dotnet publish HeThongQLTV/HeThongQLTV.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/database && chown -R app:app /app/database
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV SeedDemo=true
USER app
EXPOSE 10000
ENTRYPOINT ["dotnet", "HeThongQLTV.dll"]
