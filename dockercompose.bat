@ECHO OFF

SET MapQuest=src\MapQuest
RMDIR /S /Q "%MapQuest%\DeployLinux" >NUL

ECHO.
ECHO ** Publish MapQuest for Linux
ECHO.
pushd %MapQuest%
CALL publishLinux.bat >NUL
popd

ECHO.
ECHO ** Checking MapQuest for Linux
ECHO.
IF NOT EXIST "%MapQuest%\DeployLinux\MapQuest.dll" (
  ECHO Camas Release build not found "%MapQuest%\DeployLinux\MapQuest.dll"
  GOTO ERROR
)

ECHO.
ECHO ** Creating docker image
ECHO.
docker compose down
docker build -f Dockerfile -t robertpeters/mapquest:latest .
docker compose up -d --build

GOTO SUCCESS


:ERROR
ECHO.
ECHO !! ERROR - Error occured
GOTO END

:SUCCESS
GOTO END

:END
ECHO.
PAUSE