#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PizzaButt.Notifications/PizzaButt.Notifications.csproj", "PizzaButt.Notifications/"]
RUN dotnet restore "PizzaButt.Notifications/PizzaButt.Notifications.csproj"
COPY . .
WORKDIR "/src/PizzaButt.Notifications"
RUN dotnet build "PizzaButt.Notifications.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PizzaButt.Notifications.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PizzaButt.Notifications.dll"]