using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using Scoz.Func;
using UnityEngine;

namespace Scoz.Editor {
    public class Upload_Bundle {
        const string DIALOG_MESSAGE = "上傳資源包到Google Storage，請確認以下：\n\n1. 已安裝GoogleCloud工具，並完成初始化\n2. 已加入環境變數\n3. 已登入有權限的帳號\n\n環境: {0}\nBundle包版號: {1}\n";
        [MenuItem("Scoz/UploadBundle/1. Dev")]
        public static void UploadBundleToDev() {
            bool isYes = EditorUtility.DisplayDialog("上傳資源包", string.Format(DIALOG_MESSAGE, "Dev", Application.version), "好!", "住手!😔");
            if (isYes)
                UploadGoogleCloud(EnvVersion.Dev);
        }
        [MenuItem("Scoz/UploadBundle/2. Test")]
        public static void UploadBundleToTest() {
            bool isYes = EditorUtility.DisplayDialog("上傳資源包", string.Format(DIALOG_MESSAGE, "Test", Application.version), "好!", "住手!😔");
            if (isYes)
                UploadGoogleCloud(EnvVersion.Test);
        }
        [MenuItem("Scoz/UploadBundle/3. Release")]
        public static void UploadBundleToRelease() {
            bool isYes = EditorUtility.DisplayDialog("上傳資源包", string.Format(DIALOG_MESSAGE, "Release", Application.version), "好!", "住手!😔");
            if (isYes) {
                isYes = EditorUtility.DisplayDialog("這是Release版本, 我勸你多想想!", string.Format(DIALOG_MESSAGE, "Release", Application.version), "怕三小!", "住手!😔");
                if (isYes) UploadGoogleCloud(EnvVersion.Release);
            }

        }

        public static void UploadGoogleCloud(EnvVersion _envVersion) {
            string googleProjectID = "";
            if (Setting_Config.GOOGLE_PROJECTS.TryGetValue(_envVersion, out string id)) {
                googleProjectID = id;
            } else {
                WriteLog.LogError("找不到GPC專案ID：" + _envVersion + " version.");
                return;
            }

            string storagePath = "";
            if (Setting_Config.GCS_BUNDLE_PATHS.TryGetValue(_envVersion, out string path)) {
                storagePath = path;
            } else {
                WriteLog.LogError("找不到GPC專案ID：" + _envVersion + " version.");
                return;
            }

            var logStrs = new List<string>();
            Process process = new Process();

            string args = string.Format("{0} {1} {2} {3} {4}", googleProjectID, storagePath, Application.version, Setting_Config.GetEnvVersion, "webgl");
            WriteLog.Log(args);

#if UNITY_EDITOR_WIN
            string fileName = "UploadBundle.bat";
            // 以系統管理員(runas)執行
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";

            process.StartInfo.CreateNoWindow = true;

            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = "./";
#endif

            int exitCode = -1;
            WriteLog.Log("命令檔位置：" + process.StartInfo.WorkingDirectory + process.StartInfo.FileName);

            try {

                process.Start();
                process.WaitForExit(); // 確保 Unity 等待外部命令檔執行完成
                foreach (var str in logStrs) {
                    WriteLog.Log(str);
                }

            } catch (Exception e) {
                WriteLog.LogError("發生錯誤：" + e.ToString());
            } finally {
                exitCode = process.ExitCode;
                process.Dispose();
                process = null;
            }

            if (exitCode != 0) {
                WriteLog.LogError("執行失敗 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行" + fileName, "執行中斷，請查看Console Log", "嗚嗚嗚", "");
            } else {
                WriteLog.Log("執行成功 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行" + fileName, "執行成功，請查看Console Log確保無任何錯誤", "確認", "");
            }
        }

        public static void UploadJsonToGCS(EnvVersion _envVersion) {
            // 1. 取得對應環境的 GCP 專案 ID
            string googleProjectID = "";
            if (Setting_Config.GOOGLE_PROJECTS.TryGetValue(_envVersion, out string projId)) {
                googleProjectID = projId;
            } else {
                WriteLog.LogError("找不到 GCP 專案 ID：" + _envVersion);
                return;
            }

            // 2. 取得對應環境的 GCS JSON 儲存路徑
            string jsonStoragePath = "";
            // 假設你有一個對照表 Setting_Config.GCS_JSON_PATHS，key 是 EnvVersion，value 是像 "my-bucket-name/jsons" 這種格式
            if (Setting_Config.GCS_JSON_PATHS.TryGetValue(_envVersion, out string path)) {
                jsonStoragePath = path;
            } else {
                WriteLog.LogError("找不到 GCS JSON 路徑：" + _envVersion);
                return;
            }

            var logStrs = new List<string>();
            Process process = new Process();

            WriteLog.LogFormat("準備上傳 JSON -> ProjectID: {0}  StoragePath: {1}", googleProjectID, jsonStoragePath);

#if UNITY_EDITOR_WIN
            // Windows 平台下呼叫 UploadJson.bat
            string fileName = "UploadJson.bat";
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";               // 以系統管理員身份(若需要)
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            // 傳給腳本的參數：<GCP_PROJECT_ID> <GCS_BUCKET_PATH>
            process.StartInfo.Arguments = string.Format("{0} {1}", googleProjectID, jsonStoragePath);
            // 這裡假設 UploadJson.bat 和 Unity 專案根目錄同層
            process.StartInfo.WorkingDirectory = "./";

#elif UNITY_EDITOR_OSX
    // macOS 平台下呼叫 UploadJson.sh
    string fileName = "UploadJson.sh";
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
    process.StartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
    process.StartInfo.FileName = fileName;
    // 傳給腳本的參數：<GCP_PROJECT_ID> <GCS_BUCKET_PATH>
    process.StartInfo.Arguments = string.Format("{0} {1}", googleProjectID, jsonStoragePath);
    process.StartInfo.WorkingDirectory = "./";
#endif

            int exitCode = -1;
            WriteLog.Log("要執行的腳本位置：" + process.StartInfo.WorkingDirectory + process.StartInfo.FileName);

            try {
#if !UNITY_EDITOR_WIN
        // 只有 macOS/sh 才需要讀取輸出
        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                logStrs.Add(args.Data);
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                logStrs.Add(args.Data);
        };
#endif

                process.Start();

#if !UNITY_EDITOR_WIN
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
#endif

                process.WaitForExit(); // 等待外部腳本執行完畢

                // 將外部腳本的輸出印到 Unity Console
                foreach (var str in logStrs) {
                    WriteLog.Log(str);
                }
            } catch (Exception e) {
                WriteLog.LogError("執行 UploadJson 腳本時發生錯誤：" + e.ToString());
            } finally {
                exitCode = process.ExitCode;
                process.Dispose();
                process = null;
            }

            // 根據 exitCode 判斷成敗，並彈出提示訊息
            if (exitCode != 0) {
                WriteLog.LogError("UploadJson 腳本執行失敗，ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 UploadJson 腳本", "上傳中斷，請查看 Console Log 取得詳細資訊。", "好");
            } else {
                WriteLog.Log("UploadJson 腳本執行成功，ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 UploadJson 腳本", "上傳完成，請查看 Console Log 確認是否有任何錯誤。", "好");
            }
        }
    }
}