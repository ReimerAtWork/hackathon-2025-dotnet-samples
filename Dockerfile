# Use ASP.NET Core runtime (includes Microsoft.AspNetCore.App)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
# Kestrel in containers defaults to 8080/8443; be explicit:
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/hackathon-dotnet.csproj", "src/"]
RUN dotnet restore "src/hackathon-dotnet.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "hackathon-dotnet.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "hackathon-dotnet.csproj" -c Release -o /app/publish

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hackathon-dotnet.dll"]
