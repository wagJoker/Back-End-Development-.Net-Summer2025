FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChatApp.Backend/ChatApp.Backend.csproj", "ChatApp.Backend/"]
RUN dotnet restore "ChatApp.Backend/ChatApp.Backend.csproj"
COPY . .
WORKDIR "/src/ChatApp.Backend"
RUN dotnet build "ChatApp.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatApp.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatApp.Backend.dll"] 