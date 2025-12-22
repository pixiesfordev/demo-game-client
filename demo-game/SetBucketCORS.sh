#!/bin/bash

# 1. 先確表有GCS權限
# 2. BUCKET_NAME有調整要記得改
# 3. 在gitbash跑./SetBucketCORS.sh


BUCKET_NAME="minigames-public-test"
CORS_FILE="cors.json"

echo "為bucket \"gs://$BUCKET_NAME\" 設定CORS規則..."

# 產生 cors.json
cat > "$CORS_FILE" <<EOF
[
  {
    "origin": ["*"],
    "method": ["GET","HEAD","OPTIONS"],
    "responseHeader": ["Content-Type","Access-Control-Allow-Origin"],
    "maxAgeSeconds": 3600
  }
]
EOF

# 套用 CORS
gsutil cors set "$CORS_FILE" "gs://$BUCKET_NAME"

if [ $? -ne 0 ]; then
  echo "錯誤：設定 CORS 失敗，請檢查錯誤訊息並重試"
  # 刪除暫存的 cors.json
  rm -f "$CORS_FILE"
  exit 2
fi

# 刪除暫存的 cors.json
rm -f "$CORS_FILE"

echo "完成：已為 gs://$BUCKET_NAME 套用 CORS 規則"
echo "設定完成 但GCS節點更新可能會需要等一下子"
exit 0
