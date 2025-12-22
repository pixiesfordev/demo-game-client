#!/bin/bash
cd "$(dirname "$0")"
echo "執行UCB_MoveBuildFile.sh"
# 顯示目前目錄下所有資料夾名稱
echo "目前目錄下的資料夾："
for dir in */; do
    # 刪除尾端的斜線(就好看用)
    echo "${dir%/}"
done
echo ""

echo "檢查環境變數"
echo "GAME_VERSION: $GAME_VERSION"
echo "ENV_VERSION: $ENV_VERSION"

echo "開始複製 Bundle與addressables_content_state 到UCB下載資料夾中"



# 測試時 可以在Git Bash中cd到同目錄然後下./UCB_MoveBuildFile.sh
# UNITY_PLAYER_PATH="./Test"
# GAME_VERSION="1.8.1"
# ENV_VERSION="Dev"

# 將 GameVersion 轉換成只取前兩個數字（例如 "1.5.1" 變成 "1.5"）
gameVersion=${GAME_VERSION%.*}
addressables_content_state_Path="./Assets/AddressableAssetsData/WebGL/$ENV_VERSION/$gameVersion/addressables_content_state_$gameVersion.bin"
# 如果是 Windows 環境，要轉換路徑格式
if [[ "$BUILDER_OS" == "WINDOWS" ]]; then
    PLAYER_PATH=$(cygpath -wa "$UNITY_PLAYER_PATH")
else
    PLAYER_PATH="$UNITY_PLAYER_PATH"
fi

echo "WebGL 輸出資料夾：$PLAYER_PATH"
echo "Bundle 資料夾版本：$gameVersion"

# 先確保 Bundle 目錄存在
mkdir -p "$PLAYER_PATH/Bundle"
mkdir -p "$PLAYER_PATH/AddressablesContentState"

# 複製 Bundle 資料夾到指定資料夾中
cp -r "./ServerData/$gameVersion/" "$PLAYER_PATH/Bundle"
echo "Bundle 已複製到 $PLAYER_PATH/Bundle/"

# 複製 addressables_content_state.bin 檔案到指定資料夾
cp "$addressables_content_state_Path" "$PLAYER_PATH/AddressablesContentState/"
echo "addressables_content_state.bin檔案 已複製到 $PLAYER_PATH/AddressablesContentState/"
