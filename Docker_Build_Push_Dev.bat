@echo off
FOR /F "tokens=* USEBACKQ" %%F IN (`git rev-parse HEAD`) DO (
SET var=%%F
)
SET git=%var:~0,7%
ECHO Detected Commit %git%

set /p v="What version is this? "

set containername=cindi:%v%-%git%

docker build --no-cache -f ./Dockerfile -t %containername% .
docker tag %containername% xucito/%containername%
docker push xucito/%containername%
Pause