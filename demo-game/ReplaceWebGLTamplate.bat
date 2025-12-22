chcp 65001

echo 正在複製 WEBGL_CustomTemplates 下的所有檔案至 Build_WebGL ...
xcopy "WEBGL_CustomTemplates\*" "Build_WebGL\" /e /y

echo 覆蓋完成！
exit /b 0
