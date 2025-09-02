# Use the official .NET 9.0 runtime image as a base
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/hackathon-dotnet.csproj", "src/"]
RUN dotnet restore "src/hackathon-dotnet.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "hackathon-dotnet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "hackathon-dotnet.csproj" -c Release -o /app/publish

# Build the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hackathon-dotnet.dll"]
