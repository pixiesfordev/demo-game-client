using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

public static class JSFuncWrapper {

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void callVoidFunc(IntPtr fnamePtr);
    [DllImport("__Internal")]
    private static extern void callVoidFuncWithStringArgs(IntPtr fnamePtr, IntPtr argsPtr, int argsCount);
    [DllImport("__Internal")]
    private static extern void sendAction(string action);
    [DllImport("__Internal")]
    private static extern void sendActionWithParam(string action, string param);
    [DllImport("__Internal")]
    private static extern int isMobileBrowser();
    [DllImport("__Internal")]
    private static extern void copyText(string text);
    [DllImport("__Internal")]
    private static extern int getTimeZoneOffsetHours();

#endif

    public static void CallFunc(string functionName) {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Convert function name to pointer
        IntPtr fnamePtr = Marshal.StringToHGlobalAnsi(functionName);

        // Call the JavaScript function
        callVoidFunc(fnamePtr);

        // Free the unmanaged memory
        Marshal.FreeHGlobal(fnamePtr);
#else
        Debug.Log($"(editor) Call to JS function '{functionName}'");
#endif
    }

    public static void CallFuncWithStringArgs(string functionName, params string[] args) {
#if UNITY_WEBGL && !UNITY_EDITOR
        // Convert the function name to a pointer
        IntPtr fnamePtr = Marshal.StringToHGlobalAnsi(functionName);

        // Convert the arguments to a pointer
        IntPtr[] argPtrs = new IntPtr[args.Length];
        for (int i = 0; i < args.Length; i++) {
            argPtrs[i] = Marshal.StringToHGlobalAnsi(args[i]);
        }

        // Allocate memory for the arguments
        IntPtr argsPtr = Marshal.AllocHGlobal(IntPtr.Size * args.Length);
        for (int i = 0; i < args.Length; i++) {
            Marshal.WriteIntPtr(argsPtr, i * IntPtr.Size, argPtrs[i]);
        }

        callVoidFuncWithStringArgs(fnamePtr, argsPtr, args.Length);

        for (int i = 0; i < args.Length; i++) {
            Marshal.FreeHGlobal(argPtrs[i]);
        }
        Marshal.FreeHGlobal(argsPtr);
        Marshal.FreeHGlobal(fnamePtr);
#else
        if (args == null) {
            Debug.Log($"(editor) Call to JS function '{functionName}' without args");
        } else {
            Debug.Log($"(editor) Call to JS function '{functionName}' with args: {string.Join(", ", args)}");
        }
#endif
    }
    public static void SendAction(string action) {
        WriteLog_UnityAssembly.Log($"SendAction: {action}");
#if UNITY_WEBGL && !UNITY_EDITOR
        sendAction(action);
#else
        Debug.Log($"(editor) Send action: {action}");
#endif
    }

    public static void SendActionWithParam(string action, string param) {
        WriteLog_UnityAssembly.Log($"SendActionWithParam: action={action} param={param}");
#if UNITY_WEBGL && !UNITY_EDITOR
        sendActionWithParam(action, param);
#else
        Debug.Log($"(editor) Send action with param: action={action} param={param}");
#endif
    }
    /// <summary>
    /// 回傳是否為行動瀏覽器（true=Mobile, false=Desktop）
    /// </summary>
    public static bool IsMobileBrowser() {
#if UNITY_WEBGL && !UNITY_EDITOR
        return isMobileBrowser() == 1;
#else
        // Editor 下預設 false
        return false;
#endif
    }

    /// <summary>
    /// 回傳當前時區（整數小時，例如 +8、-5）
    /// </summary>
    public static int GetTimeZoneOffsetHours() {
#if UNITY_WEBGL && !UNITY_EDITOR
        return getTimeZoneOffsetHours();
#else
        // Editor 下預設本機時區
        return (int)System.TimeZoneInfo.Local.BaseUtcOffset.TotalHours;
#endif
    }
    public static void CopyToClipboard(string text) {
        WriteLog_UnityAssembly.Log("Call CopyToClipboard");
#if UNITY_WEBGL && !UNITY_EDITOR
    copyText(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}
