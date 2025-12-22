using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Scoz.Func;

public class ReplaceTextWithTMP : EditorWindow {
    // key統一用小寫
    private static Dictionary<string, string> fontMapping = new Dictionary<string, string>()
    {
        { "notosanstc-bold", "Assets/AddressableAssets/Fonts/pingfang-sc-regular SDF/pingfang-sc-regular SDF.asset" },
        { "inter-variablefont_opsz,wght", "Assets/AddressableAssets/Fonts/pingfang-sc-regular SDF/pingfang-sc-regular SDF.asset"  },
        { "legacyruntime", "Assets/AddressableAssets/Fonts/pingfang-sc-regular SDF/pingfang-sc-regular SDF.asset"  },
    };
    [MenuItem("Scoz/Utility/TextToTMP")]
    public static void ShowWindow() {
        GetWindow<ReplaceTextWithTMP>("Replace Text With TMP").Show();
    }

    private void OnGUI() {
        GUILayout.Label("將場景內所有 UI Text 替換為 TextMeshProUGUI", EditorStyles.boldLabel);

        if (GUILayout.Button("開始替換")) {
            ReplaceAllText();
        }
    }

    private static void ReplaceAllText() {
        // 找到場景中所有的 Text
        Text[] allTexts = GameObject.FindObjectsOfType<Text>(true);
        int count = 0;

        foreach (var uiText in allTexts) {
            GameObject go = uiText.gameObject;

            // 1. 讀取原屬性
            string content = uiText.text;
            int fontSize = uiText.fontSize;
            Color color = uiText.color;
            TextAnchor alignment = uiText.alignment;
            Font origFont = uiText.font;

            // 2. 先刪除原本的 Text component
            Undo.RecordObject(go, "Remove UI.Text");
            Object.DestroyImmediate(uiText, true);

            // 3. 新增 TextMeshProUGUI
            var tmp = Undo.AddComponent<TextMeshProUGUI>(go);

            // 4. 設定文字內容、大小、顏色
            tmp.text = content;
            tmp.fontSize = fontSize;
            tmp.color = color;

            // 5. 對齊方式轉換
            tmp.alignment = ConvertAlignment(alignment);

            // 6. 字體資源對應
            if (origFont != null) {
                string fontName = origFont.name.ToLower();
                if (fontMapping.ContainsKey(fontName)) {
                    string path = fontMapping[fontName];
                    TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                    if (fontAsset != null) {
                        tmp.font = fontAsset;
                    } else {
                        WriteLog.LogError($"找不到 TMP Font Asset: {path}");
                    }
                } else {
                    WriteLog.LogError($"未定義要轉換的字體: {fontName}");
                }
            }


            count++;
        }

        WriteLog.Log($"共替換 {count} 個 UI Text");
    }

    // 將 UnityEngine.UI.Text 的 TextAnchor 轉為 TextMeshPro 的 TextAlignmentOptions
    private static TextAlignmentOptions ConvertAlignment(TextAnchor anchor) {
        switch (anchor) {
            case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
            case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
            case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
            default: return TextAlignmentOptions.TopLeft;
        }
    }
}
