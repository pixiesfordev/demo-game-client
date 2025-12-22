using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Scoz.Func;
using System.IO;
using UnityEditor.AddressableAssets;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using Cysharp.Threading.Tasks;


namespace Scoz.Editor {

    public class Build_Addressable : BuildScriptPackedMode {

        [MenuItem("Scoz/Build Bundle/NewBuild", priority = 12)]
        public static void NewBuild() {
            var setAddressablePath = Setting_SwitchVersion.SetRemoteLoadPathBeforeBuildBundle();
            if (!setAddressablePath) {
                WriteLog.LogError("設置Addressable Path失敗");
                return;
            }
            if (GetEnvVersion() == EnvVersion.Release) {
                SetRelaseBundleVersion();
            } else {
                var setVer = AutoSetBundleVersion.SetBundleVersionToFirstVersion(); // Bundle版號重置
                if (!setVer) {
                    WriteLog.LogError("AutoSetBundleVersion 失敗");
                    return;
                }
            }
            BuildDll();

            // 取得Addressable Asset設置
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) {
                WriteLog.LogError("找不到Addressable Assets設置。");
                return;
            }
            // 進行new Build
            AddressableAssetSettings.BuildPlayerContent();
            WriteLog.LogColor("New Build Bundle完成", WriteLog.LogType.Addressable);
            // 重新命名並儲存帶版本號的 .bin 檔案
            //renameContentState();

        }
        /// <summary>
        /// 重新命名帶版本號的 .bin 檔案
        /// </summary>
        static void renameContentState() {
            string env = GetEnvVersion().ToString();
            string version = Application.version;
            BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            string sourcePath = string.Format(Setting_Config.ADDRESABLE_BIN_PATH, currentBuildTarget, env, version);
            string sourceFile = $"{sourcePath}addressables_content_state.bin";
            string targetPath = $"{sourcePath}addressables_content_state_{version}.bin";

            try {
                // 確保目標目錄存在
                string targetDir = Path.GetDirectoryName(sourcePath);
                if (!Directory.Exists(targetDir)) {
                    Directory.CreateDirectory(targetDir);
                    WriteLog.Log($"建立目錄：{targetDir}");
                }

                // 檢查來源檔案是否存在
                if (File.Exists(sourceFile)) {
                    File.Copy(sourceFile, targetPath, true); // 複製並覆蓋目標檔案
                    File.Delete(sourceFile); // 刪除原始檔案
                    WriteLog.LogColor($"重新命名.bin 檔案：{targetPath}", WriteLog.LogType.Addressable);
                    AssetDatabase.Refresh(); // 刷新 Unity
                } else {
                    WriteLog.LogError($"找不到 addressables_content_state.bin：{sourceFile}");
                }
            }
            catch (Exception e) {
                WriteLog.LogError($"處理 .bin 檔案時發生錯誤：{e.Message}");
            }
        }

        [MenuItem("Scoz/Build Bundle/Update a previous build", priority = 12)]
        public static void UpdateAPreviousBuild() {
            var setAddressablePath = Setting_SwitchVersion.SetRemoteLoadPathBeforeBuildBundle();
            if (!setAddressablePath) {
                WriteLog.LogError("設置Addressable Path失敗");
                return;
            }

            if (GetEnvVersion() == EnvVersion.Release) {
                SetRelaseBundleVersion();
            } else {
                var setVer = AutoSetBundleVersion.IncrementBundleVersion(); // Bundle版號+1
                if (!setVer) {
                    WriteLog.LogError("IncrementBundleVersion 失敗");
                    return;
                }
            }
            BuildDll();
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = GetDefaultGroup();
            var path = ContentUpdateScript.GetContentStateDataPath(false);

            // 取Addressable設定
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            var rawBundleBuildPath = settings.profileSettings.GetValueById(settings.activeProfileId, schema.BuildPath.Id);
            // 替换 [Application.version] 和 [BuildTarget]
            string bundleBuildPath = rawBundleBuildPath;
            bundleBuildPath = bundleBuildPath.Replace("[Application.version]", Application.version.ToString());
            bundleBuildPath = bundleBuildPath.Replace("[BuildTarget]", EditorUserBuildSettings.activeBuildTarget.ToString());
            // 移除舊版Bundle包(不移除每次更新資料夾會變大 導致上傳GCP變慢)
            if (Directory.Exists(bundleBuildPath)) {
                Directory.Delete(bundleBuildPath, true);
                Debug.LogError("移除舊版bundle包: " + bundleBuildPath);
            } else {
                Debug.LogError("無舊版bundle包需要移除: " + bundleBuildPath);
            }

            var result = ContentUpdateScript.BuildContentUpdate(settings, path);

            if (!string.IsNullOrEmpty(result.Error)) {
                WriteLog.LogError(result.Error);
                Debug.LogError(result.Error);
            } else {
                WriteLog.LogColor("更新Bundle完成", WriteLog.LogType.Addressable);
            }
        }
        public static AddressableAssetGroup GetDefaultGroup() {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings.DefaultGroup;
        }

        public static AddressableAssetGroup GetGroupByName(string groupName) {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings.FindGroup(groupName);
        }
        public static EnvVersion GetEnvVersion() {
#if Dev
            return EnvVersion.Dev;

#elif Test
            return EnvVersion.Test;
#elif Release
            return EnvVersion.Release;
#else
            WriteLog.LogError("GetEnvVersion 目前尚未選擇版本，請先執行SwitchVersion");
            return EnvVersion.Dev;
#endif
        }
        static void BuildDll() {
            BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
            string logPath = "ScozBuildLog";
            LogFile.AppendWrite(logPath, "\n");
            LogFile.AppendWrite(logPath, $"開始更新Dll  平台: {activeTarget}  版本: {Application.version}");
            HybridCLR.Editor.Commands.AOTReferenceGeneratorCommand.GenerateAOTGenericReference(activeTarget);
            var metaDatas = UpdateHybridCLManagerMetaData();//更新GameAssembly的元數據資料(UnityAssembly載好GameAssembly資源後會透過反射去取需求的元數據dll清單)
            //HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll(); //之前有GenerateAll過, 如何沒動到AOT程式 後續更新只需要使用CompileDll就可以
            HybridCLR.Editor.Commands.CompileDllCommand.CompileDllActiveBuildTarget();
            LogFile.AppendWrite(logPath, $"將需要的Dlls複製到AddressableAssets/Dlls/Dlls/");
            // 刪除舊資料並重新建立目標資料夾
            string directoryPath = Path.Combine(Application.dataPath, "AddressableAssets/Dlls/Dlls/");
            // 檢查目標資料夾是否存在
            if (Directory.Exists(directoryPath)) {
                try {
                    // 嘗試刪除目標資料夾
                    Directory.Delete(directoryPath, true);
                    LogFile.AppendWrite(logPath, $"已刪除目標資料夾{directoryPath}");
                }
                catch (Exception e) {
                    LogFile.AppendWrite(logPath, $"無法刪除目標資料夾({directoryPath})：{e.Message}");
                }
            }
            string targetDirectory = Path.GetDirectoryName(directoryPath);
            Directory.CreateDirectory(targetDirectory);
            LogFile.AppendWrite(logPath, $"已重新建立目標資料夾{directoryPath}");

            // 複製所有需要的Dlls並追加.bytes結尾
            LogFile.AppendWrite(logPath, $"開始複製DLLs");
            // Game
            string sourcePath = Path.Combine(Application.dataPath, $"../HybridCLRData/HotUpdateDlls/{activeTarget}/Game.dll");
            string targetPath = Path.Combine(Application.dataPath, "AddressableAssets/Dlls/Dlls/Game.dll.bytes");
            try {
                File.Copy(sourcePath, targetPath, true);
                LogFile.AppendWrite(logPath, $"成功! 從 {sourcePath} 到 {targetPath}");
            }
            catch (Exception _e) {
                LogFile.AppendWrite(logPath, $"失敗! 從 {sourcePath} 到 {targetPath}  錯誤: {_e}");
            }
            // 補充元數據(使用 metaDatas來複製dll, 不可以直接使用AOTMetadata.AotDllList 因為自動化腳本修改.cs檔案後 在此自動化腳本中只能訪問未修改前的版本)
            LogFile.AppendWrite(logPath, "MetaData Count: " + metaDatas.Length.ToString());
            foreach (var item in metaDatas) {
                LogFile.AppendWrite(logPath, item);
                string dllName = item;
                //// 檢查結尾是否為 ".dll" 如果不是，則追加 ".dll" (不知道為什麼Realm結尾不是.dll)
                //if (!dllName.EndsWith(".dll")) {
                //    dllName += ".dll";
                //    LogFile.AppendWrite(logPath, $"PatchedAOTAssemblyList有資料不為.dll結尾 自動更名為.dll結尾 更名後{dllName}");
                //}
                sourcePath = Path.Combine(Application.dataPath, $"../HybridCLRData/AssembliesPostIl2CppStrip/{activeTarget}/{dllName}");
                targetPath = Path.Combine(Application.dataPath, $"AddressableAssets/Dlls/Dlls/{dllName}.bytes");
                try {
                    File.Copy(sourcePath, targetPath);
                    LogFile.AppendWrite(logPath, $"成功! 從 {sourcePath} 到 {targetPath}");
                }
                catch (Exception _e) {
                    LogFile.AppendWrite(logPath, $"失敗! 從 {sourcePath} 到 {targetPath}  錯誤: {_e}");
                }
            }

            // Unity重新import資料(必須要重新import 否則透過Unity取到的資料都會是舊的)
            string importPath = "Assets/AddressableAssets/Dlls/Dlls";
            AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ImportRecursive);
            LogFile.AppendWrite(logPath, $"重新import 路徑{importPath}");

            LogFile.AppendWrite(logPath, "結束更新Dlls : " + Application.version);
        }

        /// <summary>
        /// 更新UpdateHybridCLManager內的補充元數據內容
        /// </summary>
        static string[] UpdateHybridCLManagerMetaData() {
            string logPath = "ScozBuildLog";
            string copyPath = "Assets/HybridCLRGenerate/AOTGenericReferences.cs";
            string pastePath = "Assets/Scripts/Assembly_Game/HybridCLR/AOTMetadata.cs";

            try {
                string[] metaDatas = null;
                // 讀取第一份文字檔的內容
                using (StreamReader firstReader = new StreamReader(copyPath)) {
                    string firstContent = firstReader.ReadToEnd();

                    // 使用正則表達式匹配PatchedAOTAssemblyList的內容
                    Match match = Regex.Match(firstContent, @"public static readonly IReadOnlyList<string> PatchedAOTAssemblyList\s*=\s*new List<string>\s*{(.+?)};", RegexOptions.Singleline);
                    if (match.Success) {
                        // 取PatchedAOTAssemblyList的內容
                        string patchedAOTAssemblyListContent = match.Groups[1].Value.Trim();
                        //將dll名稱去掉"與空白字串並存到陣列中之後使用(不可以直接使用AOTMetadata.AotDllList 因為自動化腳本修改.cs檔案後 在此自動化腳本中只能訪問未修改前的版本)
                        metaDatas = patchedAOTAssemblyListContent.Split(',');
                        metaDatas = metaDatas.Select(s => Regex.Replace(s.Replace("\"", ""), "\\s+", "")).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                        // 取第二份文字檔的內容
                        using (StreamReader secondReader = new StreamReader(pastePath)) {
                            string secondContent = secondReader.ReadToEnd();

                            // 使用正則表達式替換AotDllList的內容
                            string updatedSecondContent = Regex.Replace(secondContent, @"public static List<string>\s+AotDllList\s*=\s*new List<string>\s*{(.*?)};", $"public static List<string> AotDllList = new List<string> {{{patchedAOTAssemblyListContent}}};", RegexOptions.Singleline);
                            // 替換版本字符串
                            string targetVersion = Application.version;
                            updatedSecondContent = Regex.Replace(updatedSecondContent, @"public static string Version { get; private set; } = ""\d+\.\d+\.\d+"";", $"public static string Version {{ get; private set; }} = \"{targetVersion}\";");

                            secondReader.Close();

                            // 寫入修改後的內容
                            using (StreamWriter writer = new StreamWriter(pastePath, false, Encoding.UTF8)) {
                                writer.Write(updatedSecondContent);
                            }
                            LogFile.AppendWrite(logPath, $"UpdateHybridCLManagerMetaData完成");
                        }
                    } else {
                        LogFile.AppendWrite(logPath, $"未找到PatchedAOTAssemblyList的內容");
                    }

                    // 關閉原來的檔案
                    firstReader.Close();
                    return metaDatas;
                }
            }
            catch (Exception _e) {
                LogFile.AppendWrite(logPath, $"UpdateHybridCLManagerMetaData錯誤：{_e.Message}");
                return null;
            }
        }

        static void SetRelaseBundleVersion() {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString().ToUpper();
            string ver = Application.version.Replace('.', '_');
            var bundleVer = BundleVersion.GetBundleVer(EnvVersion.Test.ToString().ToUpper(), platform, ver);
            AutoSetBundleVersion.SetBundleVersion(bundleVer); // Release版號設為Test版號
        }
    }
}