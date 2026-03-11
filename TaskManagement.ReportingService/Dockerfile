FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagement.ReportingService/TaskManagement.ReportingService.csproj", "TaskManagement.ReportingService/"]
RUN dotnet restore "TaskManagement.ReportingService/TaskManagement.ReportingService.csproj"
COPY . .
WORKDIR "/src/TaskManagement.ReportingService"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=80
ENTRYPOINT ["dotnet", "TaskManagement.ReportingService.dll"]
