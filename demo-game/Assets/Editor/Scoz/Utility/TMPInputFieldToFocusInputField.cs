#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public static class TMPInputFieldToFocusInputField {
    private const string MenuRoot = "Scoz/Utility/TMP_InputField To FocusTMPInputField";

    [MenuItem(MenuRoot + " (Selection Only)")]
    private static void ReplaceSelectionOnly() {
        ReplaceInSelection(includeChildren: false);
    }

    [MenuItem(MenuRoot + " (Include Children)")]
    private static void ReplaceSelectionWithChildren() {
        ReplaceInSelection(includeChildren: true);
    }

    private static void ReplaceInSelection(bool includeChildren) {
        var roots = Selection.gameObjects;
        if (roots == null || roots.Length == 0) {
            EditorUtility.DisplayDialog("Replace TMP_InputField",
                "請先在 Hierarchy 選取至少一個物件。", "OK");
            return;
        }

        // 收集目標
        var targets = roots
            .SelectMany(go => includeChildren ? go.GetComponentsInChildren<TMP_InputField>(true)
                                              : go.GetComponents<TMP_InputField>())
            .Where(t => t != null)
            .Distinct()
            .ToArray();

        if (targets.Length == 0) {
            EditorUtility.DisplayDialog("Replace TMP_InputField",
                "選取範圍內沒有 TMP_InputField 可替換。", "OK");
            return;
        }

        int replaced = 0;
        try {
            Undo.IncrementCurrentGroup();

            for (int i = 0; i < targets.Length; i++) {
                var oldIF = targets[i];
                if (oldIF == null) continue;

                EditorUtility.DisplayProgressBar("Replacing TMP_InputField",
                    $"{oldIF.name} ({i + 1}/{targets.Length})", (float)(i + 1) / targets.Length);

                try {
                    var go = oldIF.gameObject;
                    if (go == null) {
                        Debug.LogWarning($"[ReplaceTMPInputField] 目標遺失 GameObject，索引 {i} 已跳過。");
                        continue;
                    }

                    // 已經有 FocusTMPInputField 就略過，避免重覆
                    if (go.GetComponent<FocusTMPInputField>() != null)
                        continue;

                    Undo.RegisterCompleteObjectUndo(go, "Replace TMP_InputField");

                    // 1) 先複製到剪貼快取並快取必要引用
                    bool copied = ComponentUtility.CopyComponent(oldIF);
                    if (!copied) {
                        Debug.LogWarning($"[ReplaceTMPInputField] 無法 CopyComponent：{oldIF.name}", go);
                    }
                    TMP_Text cachedText = oldIF.textComponent;          // << 正確型別
                    Graphic cachedPlaceholder = oldIF.placeholder;

                    // 2) 刪除舊的（避免 Selectable 衝突）
                    Undo.DestroyObjectImmediate(oldIF);

                    // 3) 新增 FocusTMPInputField
                    var newIF = Undo.AddComponent<FocusTMPInputField>(go);
                    if (newIF == null) {
                        Debug.LogError($"[ReplaceTMPInputField] 無法在 {go.name} 新增 FocusTMPInputField。", go);
                        continue;
                    }

                    // 4) 貼回可相容欄位
                    if (copied) {
                        if (!ComponentUtility.PasteComponentValues(newIF)) {
                            Debug.LogWarning($"[ReplaceTMPInputField] PasteComponentValues 失敗：{go.name}", go);
                        }
                    }

                    // 5) 保險補齊必要引用
                    EnsureReferences(newIF, cachedText, cachedPlaceholder);

                    EditorUtility.SetDirty(go);
                    replaced++;
                } catch (Exception exPerItem) {
                    Debug.LogError($"[ReplaceTMPInputField] {oldIF?.name} 發生例外，已跳過：\n{exPerItem}", oldIF);
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        } finally {
            EditorUtility.ClearProgressBar();
        }

        EditorUtility.DisplayDialog("Replace TMP_InputField",
            $"完成！成功替換 {replaced} 個元件。", "OK");
    }

    // 安全性檢查：只有在選取範圍有 TMP_InputField 才啟用選單
    [MenuItem(MenuRoot + " (Selection Only)", true)]
    [MenuItem(MenuRoot + " (Include Children)", true)]
    private static bool ValidateMenu() {
        return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
    }

    /// <summary>
    /// 盡量補齊 TMP_InputField 需要的引用，避免因為原本是壞的而 NRE。
    /// </summary>
    private static void EnsureReferences(FocusTMPInputField newIF, TMP_Text cachedText, Graphic cachedPlaceholder) {
        // textComponent
        if (newIF.textComponent == null) {
            if (cachedText != null) {
                newIF.textComponent = cachedText;
            } else {
                // 找第一個 TMP_Text（TextMeshProUGUI/TMP_SubMeshUI 皆為 TMP_Text）
                var tc = newIF.GetComponentsInChildren<TMP_Text>(true)
                              .FirstOrDefault(t => t != null && t.gameObject != newIF.gameObject);
                if (tc != null) newIF.textComponent = tc;
            }
        }

        // placeholder
        if (newIF.placeholder == null) {
            if (cachedPlaceholder != null) {
                newIF.placeholder = cachedPlaceholder;
            } else {
                // 先找名為 "Placeholder" 的子物件
                var phTr = newIF.transform.Find("Placeholder");
                if (phTr != null) {
                    var gr = phTr.GetComponent<Graphic>();
                    if (gr != null) newIF.placeholder = gr;
                }
                // 再退而求其次：找任一個 Graphic（但不要跟 textComponent 同物件）
                if (newIF.placeholder == null) {
                    var anyGraphic = newIF.GetComponentsInChildren<Graphic>(true)
                        .FirstOrDefault(g => newIF.textComponent == null || g.gameObject != newIF.textComponent.gameObject);
                    if (anyGraphic != null) newIF.placeholder = anyGraphic;
                }
            }
        }

        // targetGraphic（避免 Selectable 警告／NRE）
        var selectable = newIF as Selectable;
        if (selectable != null && selectable.targetGraphic == null) {
            var img = newIF.GetComponent<Graphic>();
            if (img != null) selectable.targetGraphic = img;
            else {
                var any = newIF.GetComponentsInChildren<Graphic>(true).FirstOrDefault();
                if (any != null) selectable.targetGraphic = any;
            }
        }
    }
}
#endif
