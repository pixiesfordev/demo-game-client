@echo off
chcp 65001>nul

REM ----------------------------------------------------
REM RestartDeployment.bat : rolling restart of a k8s Deployment
REM Usage:
REM   RestartDeployment.bat GameName true
REM     → restart GameName-frontend-deployment
REM   RestartDeployment.bat GameName false
REM     → restart GameName-test-deployment
REM ----------------------------------------------------

if "%~1"=="" (
  echo Usage: %~nx0 GameName true^|false
  exit /b 1
)

set "GAME=%~1"

if /I "%~2"=="true" (
  set "SUFFIX=-frontend-deployment"
) else (
  if /I "%~2"=="false" (
    set "SUFFIX=-test-deployment"
  ) else (
    echo Second parameter must be true or false
    exit /b 1
  )
)

set "DEPLOY=%GAME%%SUFFIX%"

REM show command before running
echo Running: kubectl rollout restart deployment %DEPLOY% -n %GAME%

call kubectl rollout restart deployment %DEPLOY% -n %GAME%
if %ERRORLEVEL% EQU 0 (
  echo Success: deployment %DEPLOY% restarted.
  exit /b 0
) else (
  echo Error: restart failed ^(ExitCode=%ERRORLEVEL%^).
  exit /b %ERRORLEVEL%
)
