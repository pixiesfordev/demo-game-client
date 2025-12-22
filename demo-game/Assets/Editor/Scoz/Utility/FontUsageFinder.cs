using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class FontUsageFinder : EditorWindow {
    // 要搜尋的字體
    Font uiFont;
#if TMP_PRESENT
    TMP_FontAsset tmpFont;
#endif

    // 搜到的 Prefab 路徑
    Vector2 scrollPos;
    List<string> matchedPrefabs = new List<string>();

    [MenuItem("Scoz/Utility/FindUsingFontInPrefab")]
    static void OpenWindow() {
        GetWindow<FontUsageFinder>("字體使用檢索");
    }

    void OnGUI() {
        GUILayout.Label("選擇要搜尋的字體", EditorStyles.boldLabel);
        uiFont = (Font)EditorGUILayout.ObjectField("UI Text 字體", uiFont, typeof(Font), false);

#if TMP_PRESENT
        tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TextMeshPro 字體", tmpFont, typeof(TMP_FontAsset), false);
#endif

        if (GUILayout.Button("開始搜尋")) {
            FindPrefabsUsingFont();
        }

        GUILayout.Space(10);
        GUILayout.Label($"找到 {matchedPrefabs.Count} 個 Prefab:", EditorStyles.label);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var path in matchedPrefabs) {
            if (GUILayout.Button(path, EditorStyles.linkLabel)) {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void FindPrefabsUsingFont() {
        matchedPrefabs.Clear();

        // 取得所有 Prefab 資產 GUID
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        for (int i = 0; i < guids.Length; i++) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            bool isMatch = false;

            // 檢查 UnityEngine.UI.Text
            var texts = go.GetComponentsInChildren<Text>(true);
            foreach (var txt in texts) {
                if (uiFont != null && txt.font == uiFont) {
                    isMatch = true;
                    break;
                }
            }

#if TMP_PRESENT
            if (!isMatch && tmpFont != null)
            {
                // 檢查 TextMeshPro
                var tmps = go.GetComponentsInChildren<TMP_Text>(true);
                foreach (var t in tmps)
                {
                    if (t.font == tmpFont)
                    {
                        isMatch = true;
                        break;
                    }
                }
            }
#endif

            if (isMatch) {
                matchedPrefabs.Add(assetPath);
            }
        }

        Repaint();
        Debug.Log($"字體檢索完成，共找到 {matchedPrefabs.Count} 個 Prefab。");
    }
}