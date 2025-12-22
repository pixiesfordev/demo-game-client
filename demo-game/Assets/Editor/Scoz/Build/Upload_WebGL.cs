using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using Scoz.Func;
using UnityEngine;

namespace Scoz.Editor {
    public class Upload_WebGL {
        const string DIALOG_MESSAGE = "上傳WebGL主程式到Google Storage，請確認以下：\n\n1. 已安裝GoogleCloud工具，並完成初始化\n2. 已加入環境變數\n3. 已登入有權限的帳號\n\n環境: {0}\n版本: {1}\n";
        [MenuItem("Scoz/UploadWebGL/0. DevTest")]
        public static void UploadWebGLForTest() {
            bool isYes = EditorUtility.DisplayDialog("上傳WebGL專案", string.Format(DIALOG_MESSAGE, "DevTest", Application.version), "好!", "住手!😔");
            if (isYes)
                UploadGoogleCloud_DevTest(EnvVersion.Dev);
        }
        [MenuItem("Scoz/UploadWebGL/1. Dev")]
        public static void UploadWebGLToDev() {
            bool isYes = EditorUtility.DisplayDialog("上傳WebGL專案", string.Format(DIALOG_MESSAGE, "Dev", Application.version), "好!", "住手!😔");
            if (isYes)
                UploadGoogleCloud(EnvVersion.Dev);
        }
        [MenuItem("Scoz/UploadWebGL/2. Test")]
        public static void UploadWebGLToTest() {
            bool isYes = EditorUtility.DisplayDialog("上傳WebGL專案", string.Format(DIALOG_MESSAGE, "Test", Application.version), "好!", "住手!😔");
            if (isYes)
                UploadGoogleCloud(EnvVersion.Test);
        }
        [MenuItem("Scoz/UploadWebGL/3. Release")]
        public static void UploadWebGLToRelease() {
            bool isYes = EditorUtility.DisplayDialog("上傳WebGL專案", string.Format(DIALOG_MESSAGE, "Release", Application.version), "好!", "住手!😔");
            if (isYes) {
                isYes = EditorUtility.DisplayDialog("這是Release版本, 我勸你多想想!", string.Format(DIALOG_MESSAGE, "Release", Application.version), "怕三小!", "住手!😔");
                if (isYes) UploadGoogleCloud(EnvVersion.Release);
            }
        }


        public static void UploadGoogleCloud(EnvVersion _envVersion) {
            string googleProjectID = "";
            if (!Setting_Config.GOOGLE_PROJECTS.TryGetValue(_envVersion, out googleProjectID)) {
                WriteLog.LogError("找不到GPC專案ID：" + _envVersion + " version.");
                return;
            }

            string storagePath = "";
            if (!Setting_Config.GCS_WEBGL_PATHS.TryGetValue(_envVersion, out storagePath)) {
                WriteLog.LogError("找不到GPC專案ID：" + _envVersion + " version.");
                return;
            }

            WriteLog.LogFormat("專案ID: {0}  StoragePath: {1}  BundleVersion: {2}",
                googleProjectID, storagePath, Application.version);

            Process process = new Process();
            int exitCode = -1;
            string fileName = "";

            string args = string.Format("{0} {1} {2} {3} {4}", googleProjectID, storagePath, Application.version, Setting_Config.GetEnvVersion, "webgl");

#if UNITY_EDITOR_WIN
            fileName = "UploadWebGL.bat";

            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";  // 系統管理員權限
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = args;
            process.StartInfo.WorkingDirectory = "./";
#endif

            try {
                // 執行
                process.Start();
                // 等待執行完成
                process.WaitForExit();
                exitCode = process.ExitCode;
            } catch (Exception e) {
                WriteLog.LogError("發生錯誤：" + e.ToString());
            } finally {
                process.Dispose();
                process = null;
            }

            // 根據退出碼判斷執行是否成功
            if (exitCode != 0) {
                WriteLog.LogError("執行失敗 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 " + fileName,
                    "執行中斷，請查看Console Log",
                    "嗚嗚嗚", "");
            } else {
                WriteLog.Log("執行成功 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 " + fileName,
                    "執行成功，請查看Console Log確保無任何錯誤",
                    "確認", "");
            }
        }


        static void UploadGoogleCloud_DevTest(EnvVersion _envVersion) {
            string googleProjectID = "";
            if (!Setting_Config.GOOGLE_PROJECTS.TryGetValue(_envVersion, out googleProjectID)) {
                WriteLog.LogError("找不到GPC專案ID：" + _envVersion + " version.");
                return;
            }

            string storagePath = "";
            if (!Setting_Config.GCS_WEBGL_PATHS_DEVTEST.TryGetValue(_envVersion, out storagePath)) {
                WriteLog.LogError("找不到storagePath：" + _envVersion + " version.");
                return;
            }

            WriteLog.LogFormat("專案ID: {0}  StoragePath: {1}  BundleVersion: {2}",
                googleProjectID, storagePath, Application.version);

            string fileName = "UploadWebGL.bat";

            Process process = new Process();
            int exitCode = -1;

            process.StartInfo.UseShellExecute = true;
            process.StartInfo.Verb = "runas";             // 以系統管理員權限
            process.StartInfo.CreateNoWindow = false;     // 顯示命令提示視窗
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

            process.StartInfo.FileName = fileName;

            process.StartInfo.Arguments = string.Format("{0} {1} {2}",
                googleProjectID, storagePath, Application.version);

            process.StartInfo.WorkingDirectory = "./";

            WriteLog.Log($"googleProjectID: {googleProjectID} storagePath: {storagePath} Application.version: {Application.version}");
            WriteLog.Log("命令檔位置：" + process.StartInfo.WorkingDirectory + process.StartInfo.FileName);

            try {
                // 開始執行
                process.Start();
                // 等待外部命令檔結束
                process.WaitForExit();
                exitCode = process.ExitCode;
            } catch (Exception e) {
                WriteLog.LogError("發生錯誤：" + e.ToString());
            } finally {
                process.Dispose();
                process = null;
            }

            // 根據執行結果顯示 Dialog
            if (exitCode != 0) {
                WriteLog.LogError("執行失敗 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 " + fileName,
                    "執行中斷，請查看Console Log",
                    "嗚嗚嗚", "");
            } else {
                WriteLog.Log("執行成功 ExitCode：" + exitCode);
                EditorUtility.DisplayDialog("執行 " + fileName,
                    "執行成功，請查看Console Log確保無任何錯誤",
                    "確認", "");
            }
        }
    }
}