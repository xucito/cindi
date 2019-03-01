@echo off

set /p v="What version is this? "

docker build -f ./Dockerfile -t cindi-ui:%v% .
docker tag cindi:%v% xucito/cindi-ui:%v%
docker push xucito/cindi-ui:%v%
Pause