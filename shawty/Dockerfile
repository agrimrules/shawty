FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["shawty/shawty.csproj", "shawty/"]
RUN dotnet restore "shawty/shawty.csproj"
COPY . .
WORKDIR "/src/shawty"
RUN dotnet build "shawty.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "shawty.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENTRYPOINT ["dotnet", "shawty.dll"]
