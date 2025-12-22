using System.Collections.Generic;
using UnityEngine;

public static class BundleVersion {

    /// <summary>
    /// 紀錄各版本的BundleVersion
    /// Key 值格式是 [環境版本]_[平台]_[主程式版本] ，主程式版本的部分用"_"隔開，例如 DEV_WEBGL_1_2 就是 Dev的WebGL的1.2主程式版本的Bundle包更新次數
    /// </summary>
    static readonly Dictionary<string, int> Versions = new Dictionary<string, int> {
        ["DEV_WEBGL_1_34"] = 20,
        ["TEST_WEBGL_1_34"] = 14,
        ["DEV_STANDALONEWINDOWS64_1_25_1"] = 1,
        ["DEV_WEBGL_2_1"] = 4,
        ["TEST_WEBGL_2_1"] = 3,
        ["RELEASE_WEBGL_2_1"] = 2,
        ["TEST_WEBGL_2_2"] = 5,
        ["RELEASE_WEBGL_2_2"] = 2,
        ["DEV_WEBGL_2_2"] = 5,
        ["TEST_WEBGL_2_3"] = 2,
        ["DEV_WEBGL_2_3"] = 3,
        ["RELEASE_WEBGL_2_3"] = 2,
        ["TEST_WEBGL_2_4"] = 2,
        ["RELEASE_WEBGL_2_4"] = 1,
        ["DEV_WEBGL_2_4"] = 13,
        ["TEST_WEBGL_2_5"] = 3,
        ["RELEASE_WEBGL_2_5"] = 3,
        ["DEV_WEBGL_2_5"] = 3,
        ["DEV_WEBGL_2_6"] = 1,
        ["TEST_WEBGL_2_6"] = 3,
        ["RELEASE_WEBGL_2_6"] = 3,
        ["TEST_WEBGL_2_7"] = 1,
        ["RELEASE_WEBGL_2_7"] = 1,
        ["DEV_WEBGL_2_7"] = 1,
        ["DEV_WEBGL_2_8"] = 1,
        ["TEST_WEBGL_2_8"] = 9,
        ["RELEASE_WEBGL_2_8"] = 1,
        ["TEST_WEBGL_2_9"] = 2,
        ["RELEASE_WEBGL_2_9"] = 2,
        ["DEV_WEBGL_2_9"] = 7,
        ["DEV_WEBGL_3_0"] = 1,
        ["TEST_WEBGL_3_0"] = 1,
        ["RELEASE_WEBGL_3_0"] = 1,
        ["TEST_WEBGL_3_1"] = 2,
        ["RELEASE_WEBGL_3_1"] = 2,
        ["DEV_WEBGL_1_0"] = 6,
        ["DEV_WEBGL_1_1"] = 1,
        ["TEST_WEBGL_1_1"] = 3,
        ["TEST_WEBGL_1_2"] = 6,
        ["DEV_WEBGL_1_2"] = 4,
        ["DEV_WEBGL_1_3"] = 3,
        ["TEST_WEBGL_1_3"] = 4,
        ["RELEASE_WEBGL_1_3"] = 4,
        ["TEST_WEBGL_1_4"] = 12,
        ["RELEASE_WEBGL_1_4"] = 12,
        ["DEV_WEBGL_1_5"] = 4,
        ["TEST_WEBGL_1_5"] = 2,
        ["RELEASE_WEBGL_1_5"] = 2,
    };

    public static int GetBundleVer(string _evn, string _platform, string _ver) {
        var key = $"{_evn}_{_platform}_{_ver.Replace('.', '_')}";
        if (Versions.TryGetValue(key, out var v)) return v;
        Debug.LogError($"GetBundleVer 不存在的Key: {key}");
        return -1;
    }
}
