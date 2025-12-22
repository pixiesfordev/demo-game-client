@echo off
chcp 65001
cd /d "%~dp0"

REM 參數：1=GoogleProjectID  2=StorageBucketPath  3=AppVersion  4=EnvVersion  5=Platform
if "%~1"=="" goto errParam
if "%~2"=="" goto errParam
if "%~3"=="" goto errParam
if "%~4"=="" goto errParam
if "%~5"=="" goto errParam

set PROJECT=%~1
set BUCKET=%~2
set APPVER=%~3
set ENVVER=%~4
set PLATFORM=%~5

echo 開始執行 UploadBundle.bat：
echo   PROJECT   = %PROJECT%
echo   BUCKET    = %BUCKET%
echo   APPVER    = %APPVER%
echo   ENVVER    = %ENVVER%
echo   PLATFORM  = %PLATFORM%
echo.

REM 用 call 來啟動外部 .cmd
call gcloud config set project %PROJECT% || goto errGcloud

call gsutil -m cp -r ServerData\%ENVVER%\%PLATFORM%\%APPVER%\* gs://%BUCKET%/%APPVER%/ || goto errUpload

call gsutil -m setmeta -h "Cache-Control:no-store" -r gs://%BUCKET%/%APPVER%/ || goto errMeta

echo Success: 完成上傳並設定 metadata
exit /b 0

:errParam
echo Error: 參數不足或為空
exit /b 1

:errGcloud
echo Error: 無法切換到 GCP 專案 %PROJECT%
exit /b 2

:errUpload
echo Error: 上傳 ServerData\%PLATFORM%\%APPVER%\ 失敗
exit /b 3

:errMeta
echo Error: 設定 Cache-Control metadata 失敗
exit /b 4
