@echo off

set /p v="What version is this? "

docker build -f ./Dockerfile -t cindi:%v% .
docker tag cindi:%v% xucito/cindi:%v%
docker push xucito/cindi:%v%
Pause