FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY ServerConsoleApp/ServerConsoleApp.Core/ServerConsoleApp.Core.csproj ServerConsoleApp/ServerConsoleApp.Core/
COPY ShareClassLibrary/ShareClassLibrary.Standard/ShareClassLibrary.Standard.csproj ShareClassLibrary/ShareClassLibrary.Standard/
RUN dotnet restore
COPY . .
WORKDIR /src/ServerConsoleApp/ServerConsoleApp.Core
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ServerConsoleApp.Core.dll"]
