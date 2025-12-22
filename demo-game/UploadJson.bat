@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

REM ----------------------------------------------------
REM UploadJson.bat : 上傳 JSON 檔案到 GCS - 全部 或 指定
REM 範例
REM   UploadJson.bat ProjectID BucketPath
REM     => 上傳所有 *.json
REM   UploadJson.bat ProjectID BucketPath file1 file2 file3
REM     => 上傳指定檔案 file1.json file2.json file3.json 以空白分隔
REM ----------------------------------------------------

REM 1. 切到批次檔所在目錄
cd /d "%~dp0" || (
    echo ERROR 無法切換到 %~dp0
    exit /b 1
)

REM 2. 檢查 ProjectID / BucketPath
if "%~1"=="" (
    echo 用法：%~nx0 ProjectID BucketPath [Filename...]
    exit /b 1
)
if "%~2"=="" (
    echo ERROR 缺少 BucketPath
    exit /b 1
)

set "PROJECT_ID=%~1"
set "BUCKET_PATH=%~2"

REM 3. 設定 GCP 專案
echo.
echo 設定 gcloud 專案為 %PROJECT_ID% ...
call gcloud config set project %PROJECT_ID% --quiet
if %ERRORLEVEL% NEQ 0 (
    echo ERROR gcloud 設定失敗
    exit /b 1
)

REM 4. 如果沒給第三參數 → 上傳全部
if "%~3"=="" goto :upload_all

REM 5. 有指定檔名 → 解析出所有檔名列表（去掉前兩個參數）
set "ALLARGS=%*"
for /f "tokens=1,2* delims= " %%A in ("!ALLARGS!") do set "FILES=%%C"
goto :upload_specified


:upload_all
echo.
echo [ALL] 上傳所有 JSON 檔案...
call gsutil -m cp -r "Assets\AddressableAssets\Jsons\*.json" "gs://%BUCKET_PATH%/"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR 上傳全部 JSON 失敗
    exit /b 1
)
echo [ALL] 設定 metadata Cache-Control:no-store...
call gsutil -m setmeta -h "Cache-Control:no-store" -r "gs://%BUCKET_PATH%/"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR metadata 設定失敗
    exit /b 1
)
goto :done


:upload_specified
echo.
echo [SPECIFIED] 要上傳的檔案：!FILES!

REM 6. 組合一次性上傳的來源與目的地列表
set "SRC="
set "DST="
for %%F in (!FILES!) do (
    set "SRC=!SRC! Assets\AddressableAssets\Jsons\%%F.json"
    set "DST=!DST! gs://%BUCKET_PATH%/%%F.json"
)

echo.
echo 開始並行上傳指定檔案...
call gsutil -m cp !SRC! "gs://%BUCKET_PATH%/"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR 指定檔案上傳失敗
    exit /b 1
)

echo.
echo 開始並行設定 metadata...
call gsutil -m setmeta -h "Cache-Control:no-store" !DST!
if %ERRORLEVEL% NEQ 0 (
    echo ERROR metadata 設定失敗
    exit /b 1
)

goto :done


:done
echo.
echo DONE
endlocal
exit /b 0
