# Usa la imagen del SDK de .NET 9 para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copia los archivos de proyecto y restaura dependencias (esto cachea las capas de nuget)
COPY ["src/ERP_Discoteca.Web/ERP_Discoteca.Web.csproj", "src/ERP_Discoteca.Web/"]
COPY ["src/ERP_Discoteca.Core/ERP_Discoteca.Core.csproj", "src/ERP_Discoteca.Core/"]
RUN dotnet restore "src/ERP_Discoteca.Web/ERP_Discoteca.Web.csproj"

# Copia el resto del código fuente para compilar y publicar
COPY . .
WORKDIR "/src/src/ERP_Discoteca.Web"
RUN dotnet build "ERP_Discoteca.Web.csproj" -c Release -o /app/build

# Etiqueta publish para optimización separada
FROM build AS publish
RUN dotnet publish "ERP_Discoteca.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# IMAGEN FINAL PARA EL RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Exponer el puerto 8080 explícitamente y configurar el entorno
EXPOSE 8080

# Configurar kestrel para escuchar en 0.0.0.0 puerto 8080 (esencial para Docker web apps)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Por seguridad adicional en la imagen Final, corremos como usuario regular "app" en alpine
USER $APP_UID

# Carga la compilación optimizada
COPY --from=publish /app/publish .

# Define el punto de entrada de la aplicación
ENTRYPOINT ["dotnet", "ERP_Discoteca.Web.dll"]
