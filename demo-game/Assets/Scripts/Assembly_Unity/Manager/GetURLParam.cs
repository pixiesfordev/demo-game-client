using Scoz.Func;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class GetURLParam {
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern IntPtr getURLParam(IntPtr keyPtr);
#endif

    /// <summary>
    /// 取 URL query string 裡的某個 key
    /// </summary>
    public static string GetParam(string key) {
#if UNITY_WEBGL && !UNITY_EDITOR
        IntPtr keyPtr = Marshal.StringToHGlobalAnsi(key);
        IntPtr strPtr = getURLParam(keyPtr);
        string result = Marshal.PtrToStringAnsi(strPtr);
        // 釋放記憶體
        Marshal.FreeHGlobal(keyPtr);
        return result;
#else
        WriteLog_UnityAssembly.LogWarning("GetURLParam是給Webgl版呼叫的");
        return "";
#endif
    }
}
