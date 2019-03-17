

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base

FROM microsoft/dotnet:2.2-sdk AS build

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Cindi.Presentation/Cindi.Presentation.csproj ./

# Copy everything else and build
COPY ./Cindi.Presentation/. ./
# BEWARE paths to other projects!
COPY ./Cindi.Application/. /Cindi.Application/
COPY ./Cindi.Domain/. /Cindi.Domain/
COPY ./Cindi.Infrastructure/. /Cindi.Infrastructure/
COPY ./Cindi.Persistence/. /Cindi.Persistence/

WORKDIR ./ClientApp/

RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_8.x | bash - && apt-get install -yq nodejs build-essential
RUN npm install -g npm

RUN npm install
RUN npm rebuild node-sass
WORKDIR /app

RUN dotnet build Cindi.Presentation.csproj

RUN dotnet publish -c Release -o out Cindi.Presentation.csproj

FROM base AS final

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Cindi.Presentation.dll"]
