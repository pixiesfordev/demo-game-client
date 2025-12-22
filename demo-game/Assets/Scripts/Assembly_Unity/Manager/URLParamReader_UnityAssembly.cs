using UnityEngine;
using System;
using System.Collections.Generic;

public static class URLParamReader_UnityAssembly {
    public static string GetStr(string key) {
        string url = Application.absoluteURL;
        Dictionary<string, string> args = parseQueryString(url);

        string value = null;

        if (args.TryGetValue(key, out value)) {
#if !Release
        Debug.Log($"取得 {key} = {value}");
#endif
        } else {
#if !Release
        if (!Application.isEditor) Debug.LogError($"URLParamReader 取不到值 Key: {key}");
#endif
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
