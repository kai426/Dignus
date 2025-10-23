# This Dockerfile should be run from the root directory with:
# docker build -f Dignus.Candidate.Back/Dockerfile -t dignus-candidate-back .

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Dignus.Candidate.Back/Dignus.Candidate.Back.csproj", "Dignus.Candidate.Back/"]
COPY ["Dignus.Data/Dignus.Data.csproj", "Dignus.Data/"]
RUN dotnet restore "Dignus.Candidate.Back/Dignus.Candidate.Back.csproj"
COPY . .
WORKDIR "/src/Dignus.Candidate.Back"
RUN dotnet build "Dignus.Candidate.Back.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dignus.Candidate.Back.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "Dignus.Candidate.Back.dll"]