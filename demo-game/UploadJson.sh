#!/usr/bin/env bash
set -euo pipefail
IFS=$'\n\t'

# ----------------------------------------------------
# UploadJson.sh : 上傳 JSON 檔案到 GCS - 全部 或 指定
# Usage:
#   ./UploadJson.sh PROJECT_ID BUCKET_PATH
#     → 上傳所有 *.json
#   ./UploadJson.sh PROJECT_ID BUCKET_PATH file1 file2 ...
#     → 上傳指定檔案 file1.json file2.json ... 以空白分隔
# ----------------------------------------------------

# 1. 切到腳本所在目錄
cd "$(dirname "$0")" || { echo "ERROR: 無法切換到腳本目錄"; exit 1; }

# 2. 參數檢查
if [ -z "${1-}" ]; then
    echo "用法：$(basename "$0") PROJECT_ID BUCKET_PATH [Filename...]"
    exit 1
fi
if [ -z "${2-}" ]; then
    echo "ERROR: 缺少 BUCKET_PATH"
    exit 1
fi

PROJECT_ID="$1"
BUCKET_PATH="$2"
shift 2

# 3. 設定 GCP 專案
echo
echo "設定 gcloud 專案為 ${PROJECT_ID} ..."
gcloud config set project "${PROJECT_ID}" --quiet

# 4. 根據是否指定檔名，決定全量或部分上傳
if [ $# -eq 0 ]; then
  # 全量
  echo
  echo "[ALL] 上傳所有 JSON 檔案..."
  gsutil -m cp -r "Assets/AddressableAssets/Jsons/"*.json "gs://${BUCKET_PATH}/"
  echo "[ALL] 設定 metadata Cache-Control:no-store..."
  gsutil -m setmeta -h "Cache-Control:no-store" -r "gs://${BUCKET_PATH}/"
else
  # 部分
  FILES=("$@")
  echo
  echo "[SPECIFIED] 要上傳的檔案： ${FILES[*]}"
  # 構建來源與目的地 list
  SRC=()
  DST=()
  for f in "${FILES[@]}"; do
    SRC+=("Assets/AddressableAssets/Jsons/${f}.json")
    DST+=("gs://${BUCKET_PATH}/${f}.json")
  done

  echo
  echo "開始並行上傳指定檔案..."
  gsutil -m cp "${SRC[@]}" "gs://${BUCKET_PATH}/"

  echo
  echo "開始並行設定 metadata..."
  gsutil -m setmeta -h "Cache-Control:no-store" "${DST[@]}"
fi

echo
echo "DONE"
exit 0
