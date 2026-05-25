FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ["GymManagementSystem/GymManagementSystem.csproj", "GymManagementSystem/"]
RUN dotnet restore "GymManagementSystem/GymManagementSystem.csproj"

# Copy everything else and build
COPY . .
WORKDIR /app/GymManagementSystem
RUN dotnet publish "GymManagementSystem.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GymManagementSystem.dll"]
