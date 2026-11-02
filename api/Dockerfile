FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /scr

COPY ["api.csproj", "./"]

RUN dotnet restore "api.csproj"

COPY . .

RUN dotnet publish "api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT [ "dotnet", "api.dll"]