using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class Cookie {
    public string name;
    public string value;
    public string path;
    public string domain;
    public string expires;
    public bool secure;
    public bool httpOnly;
    public string sameSite;

    public Cookie(string name, string value, string path, string domain,
                  DateTime? expires = null, bool secure = false, bool httpOnly = false, string sameSite = "Lax") {
        this.name = name;
        this.value = value;
        this.path = path;
        this.domain = domain;
        this.expires = expires.HasValue ? expires.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : "None";
        this.secure = secure;
        this.httpOnly = httpOnly;
        this.sameSite = sameSite;
    }

    public override string ToString() {
        return $"Cookie Info:\n" +
               $"Name: {name}\n" +
               $"Value: {value}\n" +
               $"Path: {path}\n" +
               $"Domain: {domain}\n" +
               $"Expires: {expires}\n" +
               $"Secure: {secure}\n" +
               $"HttpOnly: {httpOnly}\n" +
               $"SameSite: {sameSite}";
    }

    public void PrintCookieInfo() {
        Debug.Log(ToString());
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }
}

public static class CookieWrapper {
    // Extern function to set a cookie.
    // The cookie is passed as a JSON string.
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void setCookie(string cookieJson);

    [DllImport("__Internal")]
    private static extern System.IntPtr getCookie(string cookieName);
#endif

    /// <summary>
    /// Sets a cookie by sending the Cookie struct (as JSON) to JavaScript.
    /// </summary>
    public static void SetCookie(Cookie cookie) {
        string json = cookie.ToJson();
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("(webgl) SetCookie : " + json);
        setCookie(json);
#else
        Debug.Log("(editor) SetCookie : " + json);
#endif
    }

    /// <summary>
    /// Gets the cookie value for the specified name.
    /// Returns the cookie value as a string.
    /// </summary>
    public static string GetCookie(string name) {
        string cookieValue = string.Empty;

#if UNITY_WEBGL && !UNITY_EDITOR
        System.IntPtr cookiePtr = getCookie(name);
        if (cookiePtr != System.IntPtr.Zero)
        {
            cookieValue = Marshal.PtrToStringUTF8(cookiePtr);
        }
        else
        {
            Debug.Log($"Cookie '{name}' not found.");
        }
#else
        Debug.Log($"(editor) GetCookie called for: '{name}'");
#endif

        return cookieValue;
    }

}
