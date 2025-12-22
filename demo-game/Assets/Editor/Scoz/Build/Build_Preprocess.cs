using Scoz.Func;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class PeBuildProcess : IPreprocessBuildWithReport {
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report) {
        string outputPath = report.summary.outputPath;
        cleanBuildFolder(outputPath);

    }
    void cleanBuildFolder(string _buildPath) {
        // 如果是檔案，就取父資料夾
        string buildFolder = Directory.Exists(_buildPath)
            ? _buildPath
            : Path.GetDirectoryName(_buildPath);

        if (string.IsNullOrEmpty(buildFolder)) {
            Debug.LogError("[PeBuildProcess] 解析輸出路徑失敗：" + _buildPath);
            return;
        }
        // 刪除舊的輸出資料夾
        if (Directory.Exists(buildFolder)) {
            try {
                Directory.Delete(buildFolder, recursive: true);
                Debug.Log($"[PeBuildProcess] 已清空：{buildFolder}");
            } catch (System.Exception ex) {
                Debug.LogError($"[PeBuildProcess] 刪除失敗 ({buildFolder})：{ex.Message}");
            }
        }

        // 重建目標資料夾(這是必要，不然開始Build版會找不到資料夾)
        try {
            Directory.CreateDirectory(buildFolder);
            Debug.Log($"[PeBuildProcess] 已重建：{buildFolder}");
        } catch (System.Exception ex) {
            Debug.LogError($"[PeBuildProcess] 建立資料夾失敗 ({buildFolder})：{ex.Message}");
        }
    }
}
