using UnityEngine;
using System;
using System.Collections.Generic;
using Scoz.Func;

public static class URLParamReader {
    public static string GetStr(string _key) {
        string url = Application.absoluteURL;
        Dictionary<string, string> args = parseQueryString(url);
        if (args.TryGetValue(_key, out string value))
            WriteLog.Log($"取得{_key} = " + value);
        else {
            if (!Application.isEditor) {
                WriteLog.LogError($"URLParamReader 取不到值 Key: {_key}");
            }
        }
        return value;
    }

    // 簡單解析 query string 成 Dictionary
    static Dictionary<string, string> parseQueryString(string url) {
        var result = new Dictionary<string, string>();
        int idx = url.IndexOf('?');
        if (idx < 0) return result;
        string qs = url.Substring(idx + 1);
        foreach (var part in qs.Split('&')) {
            var kv = part.Split(new[] { '=' }, 2);
            if (kv.Length == 2) {
                string key = Uri.UnescapeDataString(kv[0]);
                string val = Uri.UnescapeDataString(kv[1]);
                result[key] = val;
            }
        }
        return result;
    }
}
