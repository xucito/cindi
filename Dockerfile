

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

# Configure mongodb
RUN echo nameserver 8.8.8.8 > /etc/resolv.conf

RUN apt update

RUN apt install gnupg -y

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 9DA31620334BD75D9DCB49F368818C72E52529D4

RUN echo "deb http://repo.mongodb.org/apt/debian stretch/mongodb-org/4.0 main" | tee /etc/apt/sources.list.d/mongodb-org-4.0.list

RUN apt-get update

RUN apt-get install -y mongodb-org

COPY ./docker/startup.sh /etc/init.d/startup.sh

#RUN awk '{ sub("\r$", ""); print }' /etc/init.d/startup.sh > /etc/init.d/startup.sh

RUN chmod +x /etc/init.d/startup.sh && mkdir /data && mkdir /data/db

WORKDIR /app
COPY --from=build /app/out .

#CMD /etc/init.d/startup.sh
#ENTRYPOINT ["dotnet", "Cindi.Presentation.dll"]
ENTRYPOINT ["/etc/init.d/startup.sh"]

#CMD ["sleep", "1d"]