using Scoz.Func;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Scoz.Editor {
    public class Build_WebGL {

        // 相對於專案根目錄的輸出路徑
        public const string PATH_MAIN_BUILD_ORIGIN = "Builds/Build/";

        [MenuItem("Scoz/MainBuild/WebGL")]
        public static void BuildWebGL() {
            // 1. 取得專案根目錄
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            // 2. 組出最終輸出資料夾路徑
            string outputDir = Path.Combine(projectRoot, PATH_MAIN_BUILD_ORIGIN);
            // 確保資料夾存在
            if (!Directory.Exists(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }
            WriteLog.Log($"Build 檔路徑為 {outputDir}");
            // 3. 讀取所有在 Build Settings 裡勾選的場景
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            // 4. 設定 BuildPlayerOptions
            var buildOptions = new BuildPlayerOptions {
                scenes = scenes,
                locationPathName = Path.Combine(outputDir, "webgl"),
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            // 5. 執行 Build
            try {
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded) {
                    float sizeMB = summary.totalSize / (1024f * 1024f);
                    WriteLog.Log($"WebGL Build 完成：{summary.outputPath} （{sizeMB:F2} MB）");
                } else {
                    WriteLog.LogError($"WebGL Build 失敗：{summary.result}");
                }
            } catch (Exception e) {
                WriteLog.LogError($"WebGL Build 錯誤：{e.Message}");
            }
        }
    }
}
