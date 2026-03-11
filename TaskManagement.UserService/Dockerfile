FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagement.UserService/TaskManagement.UserService.csproj", "TaskManagement.UserService/"]
RUN dotnet restore "TaskManagement.UserService/TaskManagement.UserService.csproj"
COPY . .
WORKDIR "/src/TaskManagement.UserService"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=80
ENTRYPOINT ["dotnet", "TaskManagement.UserService.dll"]
