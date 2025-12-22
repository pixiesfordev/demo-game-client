using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindMissingScripts : EditorWindow {
    [MenuItem("Scoz/Utility/Find Missing Scripts")]
    public static void Open() {
        GetWindow<FindMissingScripts>("Find Missing Scripts");
    }

    struct Entry {
        public GameObject go;  // 物件
        public string path;    // 階層路徑 Root/Child/SubChild
        public int index;      // 該物件上 Missing Script 的 Component 索引（0-based，對齊 Inspector）
    }

    List<Entry> results = new List<Entry>();
    Vector2 scroll;
    bool includeInactive = true;

    void OnEnable() {
        // 變更選取時自動刷新
        Selection.selectionChanged += AutoRefreshOnSelectionChanged;
        RefreshFromSelection();
    }

    void OnDisable() {
        Selection.selectionChanged -= AutoRefreshOnSelectionChanged;
    }

    void AutoRefreshOnSelectionChanged() {
        RefreshFromSelection();
        Repaint();
    }

    void OnGUI() {
        using (new EditorGUILayout.HorizontalScope()) {
            includeInactive = EditorGUILayout.ToggleLeft("Include Inactive", includeInactive, GUILayout.Width(140));

            if (GUILayout.Button("Scan Selection", GUILayout.Height(22)))
                RefreshFromSelection();

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Found: {results.Count}", GUILayout.Width(100));
        }

        EditorGUILayout.Space(4);

        scroll = GUILayout.BeginScrollView(scroll);
        {
            if (results.Count == 0) {
                EditorGUILayout.HelpBox("No Missing Scripts found under current selection.", MessageType.Info);
            } else {
                for (int i = 0; i < results.Count; i++) {
                    var r = results[i];
                    using (new EditorGUILayout.HorizontalScope("box")) {
                        if (GUILayout.Button("Show", GUILayout.Width(60)))
                            EditorGUIUtility.PingObject(r.go);

                        GUILayout.Label(r.path, GUILayout.MinWidth(160));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"Component Index: {r.index}", GUILayout.Width(160));
                    }
                }
            }
        }
        GUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Component Index 為該 GameObject 上 Missing Script 的順位（0 起算），" +
            "與 Inspector 顯示順序一致。可以在 Inspector 齒輪選單 Remove Component 清除空槽。",
            MessageType.None);
    }

    void RefreshFromSelection() {
        results.Clear();

        var selected = Selection.gameObjects;
        if (selected == null || selected.Length == 0) return;

        foreach (var root in selected) {
            if (root == null) continue;

            // 走訪所有子物件
            var transforms = root.GetComponentsInChildren<Transform>(includeInactive);
            foreach (var t in transforms) {
                var go = t.gameObject;
                // 先快速檢查是否可能有 null 元件
                var comps = go.GetComponents<Component>();
                bool hasNull = false;
                for (int i = 0; i < comps.Length; i++) {
                    if (comps[i] == null) { hasNull = true; break; }
                }
                if (!hasNull) continue;

                // 用 SerializedObject 對齊 Inspector 順序，精準找出 null 的索引
                var so = new SerializedObject(go);
                var mComponent = so.FindProperty("m_Component");
                if (mComponent == null) continue;

                for (int i = 0; i < mComponent.arraySize; i++) {
                    var elem = mComponent.GetArrayElementAtIndex(i);
                    var refProp = elem.FindPropertyRelative("component");
                    if (refProp != null && refProp.objectReferenceValue == null) {
                        results.Add(new Entry {
                            go = go,
                            path = GetHierarchyPath(t),
                            index = i
                        });
                    }
                }
            }
        }
    }

    static string GetHierarchyPath(Transform t) {
        // 生成類似 "Root/Child/SubChild" 的路徑
        var stack = new System.Collections.Generic.Stack<string>();
        while (t != null) {
            stack.Push(t.name);
            t = t.parent;
        }
        return string.Join("/", stack.ToArray());
    }
}
