using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System;
using Scoz.Editor;
class Build_Postprocess : IPostprocessBuildWithReport {
    // callbackOrder 決定執行順序，數字越小優先度越高
    public int callbackOrder => 0;

    // 實作 IPostprocessBuildWithReport 的方法
    public void OnPostprocessBuild(BuildReport report) {
        // 取得輸出路徑
        string buildPath = report.summary.outputPath;

        // ────────────────────────────────────────────
        // 複製整個 build 輸出到 copyDestinyFolder
        // ────────────────────────────────────────────
        // 設定目標路徑
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        Debug.Log("[Build_Postprocess] 專案根目錄=" + projectRoot);
        string relativeDest = string.Format(
            Setting_Config.PATH_MAIN_BUILD_TARGET,
            Setting_Config.GetEnvVersion.ToString(),
            "webgl",
            Application.version
        );
        string copyDestinyFolder = Path.Combine(projectRoot, relativeDest);
        Debug.Log("[Build_Postprocess] 主程式輸出路徑=" + copyDestinyFolder);


        // 檢查來源檔案是不是包含目標路經
        string fullSrc = Path.GetFullPath(buildPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        + Path.DirectorySeparatorChar;
        string fullDest = Path.GetFullPath(copyDestinyFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                         + Path.DirectorySeparatorChar;
        if (fullDest.StartsWith(fullSrc, StringComparison.OrdinalIgnoreCase)) {
            Debug.LogError($"[Build_Postprocess] 目標資料夾 ({fullDest}) 位於來源 ({fullSrc}) 之下，已取消複製。");
            return;
        }


        try {

            // 如果資料夾不存在就建立
            if (!Directory.Exists(copyDestinyFolder))
                Directory.CreateDirectory(copyDestinyFolder);

            // 複製檔案
            copyDirectory(buildPath, copyDestinyFolder);

            Debug.Log($"[Build_Postprocess] 已將 {buildPath} 複製到 {copyDestinyFolder}");
        } catch (Exception ex) {
            Debug.LogError($"[Build_Postprocess] 複製失敗: {ex.Message}");
        }

    }
    /// <summary>
    /// 遞迴複製整個資料夾
    /// </summary>
    private static void copyDirectory(string sourceDir, string destDir) {
        // 複製所有檔案
        foreach (var file in Directory.GetFiles(sourceDir)) {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }
        // 處理子資料夾
        foreach (var directory in Directory.GetDirectories(sourceDir)) {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(directory));
            if (!Directory.Exists(destSubDir)) {
                Directory.CreateDirectory(destSubDir);
            }
            copyDirectory(directory, destSubDir);
        }
    }
}
