FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

USER root

# Configure mongodb
#RUN echo nameserver 8.8.8.8 > /etc/resolv.conf

RUN apt update && apt install gnupg -y

RUN apt-get update

RUN apt-get install wget && \
wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg && \
 mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ &&\
wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list &&\
 mv prod.list /etc/apt/sources.list.d/microsoft-prod.list 

RUN dotnet dev-certs https -ep /app/cindi_cert.pfx -p "P@ssw0rd123!"

RUN apt-get update -yq && apt-get upgrade -yq && apt-get install -yq curl git nano
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash - && apt-get install -yq nodejs build-essential

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./Cindi.Presentation/Cindi.Presentation.csproj ./
COPY ./nuget.config ./

# Copy everything else and build
COPY ./Cindi.Presentation/. ./
# BEWARE paths to other projects!
COPY ./Cindi.Application/. /Cindi.Application/
COPY ./Cindi.Domain/. /Cindi.Domain/
COPY ./Cindi.Infrastructure/. /Cindi.Infrastructure/
COPY ./Cindi.Persistence/. /Cindi.Persistence/

WORKDIR /app

RUN dotnet build Cindi.Presentation.csproj

RUN dotnet publish -c Release -o out Cindi.Presentation.csproj


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app

COPY ./docker/startup.sh /etc/init.d/startup.sh

#RUN awk '{ sub("\r$", ""); print }' /etc/init.d/startup.sh > /etc/init.d/startup.sh

RUN chmod +x /etc/init.d/startup.sh 

COPY --from=build /app/out .

COPY --from=build /app/cindi_cert.pfx .

RUN mkdir /data

#CMD /etc/init.d/startup.sh
#ENTRYPOINT ["dotnet", "Cindi.Presentation.dll"]
ENTRYPOINT ["/etc/init.d/startup.sh"]

#CMD ["sleep", "1d"]