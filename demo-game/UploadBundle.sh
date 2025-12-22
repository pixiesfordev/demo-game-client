#!/bin/bash

# 參數：googleProjectID storagePath Application.version
# 例如：csc5023-games-dev minigames-public-dev/card_swap_bundle 1.1

echo "$1 $2 $3"
if [ "$1" != "" ] && [ "$2" != "" ] && [ "$3" != "" ]
then
    # 設定 gcloud 專案
    gcloud config set project "$1"

    # 上傳 Unity Bundle
    gsutil cp -r "ServerData/$3/" "gs://$2/$3/"

    # 設定不快取
    gsutil setmeta -h "Cache-Control:no-store" -r "gs://$2/$3/"

    exit 0
fi

echo "Error: parameter contain empty."
exit -1
