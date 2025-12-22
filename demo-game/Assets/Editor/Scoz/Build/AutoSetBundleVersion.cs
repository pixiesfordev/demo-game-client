using Scoz.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class AutoSetBundleVersion {
    const string FILE_PATH = "Assets/Scripts/Assembly_Game/Manager/BundleVersion.cs";
    const int DEFAULT_VERSION = 1;

    /// <summary>
    /// NewBuild 時呼叫：將當前 (環境／平台／主程式版本) 的 bundleVer 設為 DEFAULT_VERSION
    /// </summary>
    public static bool SetBundleVersionToFirstVersion() {
        var (env, plat, ver) = GetContext();
        var dict = LoadEntries();
        dict[$"{env}_{plat}_{ver}"] = DEFAULT_VERSION;
        return WriteAllFile(dict);
    }

    /// <summary>
    /// UpdatePreviousBuild 時呼叫：若該 key 不存在回傳 false；否則版本 +1 並回傳 true
    /// </summary>
    public static bool IncrementBundleVersion() {
        var (env, plat, ver) = GetContext();
        string key = $"{env}_{plat}_{ver}";
        var dict = LoadEntries();

        if (!dict.ContainsKey(key)) {
            Debug.LogError($"無現存版本，無法執行 UpdatePreviousBuild：{key}");
            return false;
        }

        dict[key] = dict[key] + 1;
        return WriteAllFile(dict);
    }

    public static bool SetBundleVersion(int targetVer) {
        var (env, plat, ver) = GetContext();
        string key = $"{env}_{plat}_{ver}";
        var dict = LoadEntries();
        Debug.Log(key + " " + targetVer);

        if (dict.TryGetValue(key, out int curVer)) {
            if (targetVer == curVer) {
                Debug.Log($"SetBundleVersion 版本相同，若需更新請先更新Test：{key}_{curVer}");
                return false;
            }
        }
        dict[key] = targetVer;
        return WriteAllFile(dict);
    }

    // 取得當前環境、平台、主程式版本
    static (string env, string plat, string ver) GetContext() {
        string env = Setting_Config.GetEnvVersion.ToString().ToUpper();
        string plat = EditorUserBuildSettings.activeBuildTarget.ToString().ToUpper();
        string ver = Application.version.Replace('.', '_');
        return (env, plat, ver);
    }

    // 讀出 BundleVersion.cs 裡的所有項目到 Dictionary
    static Dictionary<string, int> LoadEntries() {
        var dict = new Dictionary<string, int>();
        if (!File.Exists(FILE_PATH))
            return dict;

        string txt = File.ReadAllText(FILE_PATH, Encoding.UTF8);
        // 抓 Versions 初始化大括號裡的 body
        var m = Regex.Match(txt,
            @"static readonly Dictionary<string, int> Versions = new Dictionary<string, int> \{\s*(.*?)\s*\};",
            RegexOptions.Singleline);
        if (!m.Success)
            return dict;

        string body = m.Groups[1].Value;
        var itemPattern = new Regex(@"\[\s*""(?<k>[^""]+)""\s*\]\s*=\s*(?<v>\d+)\s*,");
        foreach (Match im in itemPattern.Matches(body)) {
            string k = im.Groups["k"].Value;
            int v = int.Parse(im.Groups["v"].Value);
            dict[k] = v;
        }
        return dict;
    }

    /// <summary>
    /// 根據 dict 重寫整個 BundleVersion.cs，並回傳是否成功
    /// (沒用正則表示法改 因為改起來換行跟空格間距處理起來花時間 所以乾脆直接改整份文件更快)
    /// </summary>
    static bool WriteAllFile(Dictionary<string, int> dict) {
        try {
            var sb = new StringBuilder();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("public static class BundleVersion {");
            sb.AppendLine();
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 紀錄各版本的BundleVersion");
            sb.AppendLine("    /// Key 值格式是 [環境版本]_[平台]_[主程式版本] ，主程式版本的部分用\"_\"隔開，例如 DEV_WEBGL_1_2 就是 Dev的WebGL的1.2主程式版本的Bundle包更新次數");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    static readonly Dictionary<string, int> Versions = new Dictionary<string, int> {");
            foreach (var kv in dict) {
                sb.AppendLine($"        [\"{kv.Key}\"] = {kv.Value},");
            }
            sb.AppendLine("    };");
            sb.AppendLine();
            sb.AppendLine("    public static int GetBundleVer(string _evn, string _platform, string _ver) {");
            sb.AppendLine("        var key = $\"{_evn}_{_platform}_{_ver.Replace('.', '_')}\";");
            sb.AppendLine("        if (Versions.TryGetValue(key, out var v)) return v;");
            sb.AppendLine("        Debug.LogError($\"GetBundleVer 不存在的Key: {key}\");");
            sb.AppendLine("        return -1;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            File.WriteAllText(FILE_PATH, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            return true;
        } catch (Exception e) {
            Debug.LogError($"寫入 BundleVersion.cs 發生錯誤：{e.Message}");
            return false;
        }
    }
}
