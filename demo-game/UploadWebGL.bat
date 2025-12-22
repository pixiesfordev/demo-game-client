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

echo 開始執行 UploadWebGL.bat 傳入參數為：%PROJECT% %BUCKET% %APPVER% %ENVVER% %PLATFORM%
echo.

REM -- 設置 Google Cloud 專案 --
call gcloud config set project %PROJECT% || (
    echo 錯誤：無法切換到 GCP 專案 %PROJECT%
    pause
    exit /b -2
)

REM -- 上傳 WebGL build 檔案 --
echo 正在上傳檔案到 gs://%BUCKET%/%APPVER%/ ...
call gsutil -m cp -r "Builds\%ENVVER%\%PLATFORM%\%APPVER%\*" "gs://%BUCKET%/%APPVER%/" || (
    echo 錯誤：檔案上傳失敗。
    pause
    exit /b -3
)

REM -- 設置 .gz 檔案 Content-Encoding 與 Content-Type --
echo 正在設定 .gz 檔案 metadata...
call gsutil -m setmeta -h "Content-Encoding: gzip" -h "Content-Type: application/octet-stream" "gs://%BUCKET%/%APPVER%/Build/*.data.gz"
call gsutil -m setmeta -h "Content-Encoding: gzip" -h "Content-Type: application/javascript"      "gs://%BUCKET%/%APPVER%/Build/*.framework.js.gz"
call gsutil -m setmeta -h "Content-Encoding: gzip" -h "Content-Type: application/wasm"            "gs://%BUCKET%/%APPVER%/Build/*.wasm.gz" || (
    echo 錯誤：.gz metadata 設置失敗。
    pause
    exit /b -4
)

REM -- 設置 Cache-Control 禁止快取 --
echo 設置 Cache-Control...
call gsutil -m setmeta -h "Cache-Control: no-cache, no-store, max-age=0, must-revalidate" -r "gs://%BUCKET%/%APPVER%/" || (
    echo 錯誤：Cache-Control 設置失敗。
    pause
    exit /b -5
)

echo 上傳並設定 metadata 完成！
pause
exit /b 0

:errParam
echo Error: 參數不足或為空（需要 5 個參數：GoogleProjectID, StoragePath, AppVersion, EnvVersion, Platform）
pause
exit /b -1
