# ============================ #
#        CONTRUCCION           #
# ============================ #
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY ["SGCM/SGCM.Web.csproj", "SGCM/"]
COPY ["SGCM.Domain/SGCM.Domain.csproj", "SGCM.Domain/"]
COPY ["SGCM.Application/SGCM.Application.csproj", "SGCM.Application/"]
COPY ["SGCM.Data/SGCM.Data.csproj", "SGCM.Data/"]
COPY ["SGCM.Test/SGCM.Test.csproj", "SGCM.Test/"]
RUN dotnet restore "SGCM/SGCM.Web.csproj"

# Copia el resto del codigo fuente 
COPY . .
WORKDIR "/app/SGCM/"
RUN dotnet publish "SGCM.Web.csproj" -c Release -o /app/publish

# ============================= #
#          Produccion           #
# ============================= #
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS Produccion
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "SGCM.Web.dll"]
