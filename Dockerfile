# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VCDA.FinancialManager.Web/VCDA.FinancialManager.Web.csproj", "VCDA.FinancialManager.Web/"]
COPY ["VCDA.FinancialManager.Application/VCDA.FinancialManager.Application.csproj", "VCDA.FinancialManager.Application/"]
COPY ["VCDA.FinancialManager.Domain/VCDA.FinancialManager.Domain.csproj", "VCDA.FinancialManager.Domain/"]
COPY ["VCDA.FinancialManager.Infrastructure/VCDA.FinancialManager.Infrastructure.csproj", "VCDA.FinancialManager.Infrastructure/"]
RUN dotnet restore "./VCDA.FinancialManager.Web/VCDA.FinancialManager.Web.csproj"
COPY . .
WORKDIR "/src/VCDA.FinancialManager.Web"
RUN dotnet build "./VCDA.FinancialManager.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VCDA.FinancialManager.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --chown=app:app --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VCDA.FinancialManager.Web.dll"]
