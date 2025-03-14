FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CashoutServices/CashoutServices.csproj", "src/CashoutServices/"]
RUN dotnet restore "./src/CashoutServices/CashoutServices.csproj"
COPY . .
WORKDIR "/src/src/CashoutServices"
RUN dotnet build "./CashoutServices.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CashoutServices.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Certificates /app/Certificates
ENTRYPOINT ["dotnet", "CashoutServices.dll", "--urls", "http://*:5000"]
