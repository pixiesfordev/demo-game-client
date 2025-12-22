using Cysharp.Threading.Tasks;
using tower.main;
using Scoz.Func;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TMPFontSetter : MonoBehaviour {
    [Header("TMP_FontAssets")]
    [SerializeField] AssetReference Asset_pingfang;
    [SerializeField] AssetReference Asset_SFPRODISPLAYREGULAR;
    [SerializeField] AssetReference Asset_pingfang_Signpost;
    [SerializeField] AssetReference Asset_SFPRODISPLAYREGULAR_Signpost;

    static List<TextMeshProUGUI> tmpList_waitToUpdate;

    [Serializable]
    public class FontRule {
        public string suffix = "_lang";
        public AssetReference Asset_EN;
        public AssetReference Asset_CH;
        public AssetReference Asset_VN;

        public AssetReference GetAssetRef(Language _lang) {
            switch (_lang) {
                case Language.EN: return Asset_EN;
                case Language.CH: return Asset_CH;
                case Language.VN: return Asset_VN;
                default: return null;
            }
        }
    }

    [SerializeField] List<FontRule> FontRules = new List<FontRule>();

    static string[] suffixs = new string[] { "_lang", "_lang_signpost" };
    static TMP_FontAsset fontAsset;
    static readonly Dictionary<string, TMP_FontAsset> suffixToFontAsset = new Dictionary<string, TMP_FontAsset>();

    List<FontRule> GetRuntimeRules() {
        if (FontRules != null && FontRules.Count > 0) return FontRules;

        return new List<FontRule>() {
            new FontRule() {
                suffix = "_lang",
                Asset_EN = Asset_SFPRODISPLAYREGULAR,
                Asset_CH = Asset_pingfang,
                Asset_VN = Asset_SFPRODISPLAYREGULAR,
            },
            new FontRule() {
                suffix = "_lang_signpost",
                Asset_EN = Asset_SFPRODISPLAYREGULAR_Signpost,
                Asset_CH = Asset_pingfang_Signpost,
                Asset_VN = Asset_SFPRODISPLAYREGULAR_Signpost,
            },
        };
    }

    static string[] BuildSuffixs(List<FontRule> _rules) {
        if (_rules == null || _rules.Count == 0) return Array.Empty<string>();

        var list = new List<string>(_rules.Count);
        foreach (var r in _rules) {
            if (r == null) continue;
            if (string.IsNullOrEmpty(r.suffix)) continue;
            if (list.Contains(r.suffix)) continue;
            list.Add(r.suffix);
        }
        list.Sort((a, b) => b.Length.CompareTo(a.Length));
        return list.ToArray();
    }

    static bool TryGetFontAssetByName(string _objName, bool _ignoreCase, out TMP_FontAsset _fontAsset) {
        _fontAsset = null;

        if (string.IsNullOrEmpty(_objName)) return false;
        if (suffixToFontAsset == null || suffixToFontAsset.Count == 0) return false;
        if (suffixs == null || suffixs.Length == 0) return false;

        for (int i = 0; i < suffixs.Length; i++) {
            string suffix = suffixs[i];
            if (string.IsNullOrEmpty(suffix)) continue;

            bool match = _ignoreCase
                ? _objName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                : _objName.EndsWith(suffix);

            if (!match) continue;

            return suffixToFontAsset.TryGetValue(suffix, out _fontAsset) && _fontAsset != null;
        }
        return false;
    }

    public async UniTask Init() {
        tmpList_waitToUpdate = new List<TextMeshProUGUI>();
        AsyncOperationHandle handle;

        WriteLog.Log($"TmpFont 語系為 {GamePlayer.Instance.UsingLanguage}");
        switch (GamePlayer.Instance.UsingLanguage) {
            case Language.EN:
            case Language.CH:
            case Language.VN:
                break;
            default:
                WriteLog.LogError($"setFontAsset 有尚未實作的語系: {GamePlayer.Instance.UsingLanguage}");
                return;
        }

        var rules = GetRuntimeRules();
        suffixs = BuildSuffixs(rules);
        suffixToFontAsset.Clear();

        foreach (var rule in rules) {
            if (rule == null) continue;
            if (string.IsNullOrEmpty(rule.suffix)) continue;

            var assetRef = rule.GetAssetRef(GamePlayer.Instance.UsingLanguage);
            if (assetRef == null) {
                WriteLog.LogError($"setFontAsset 找不到對應字體: {rule.suffix} / {GamePlayer.Instance.UsingLanguage}");
                suffixToFontAsset[rule.suffix] = null;
                continue;
            }

            TMP_FontAsset loaded;
            (loaded, handle) = await AddressablesLoader.GetAssetRefAsync<TMP_FontAsset>(assetRef);
            if (loaded == null) WriteLog.LogError($"載入字體錯誤 suffix={rule.suffix} fontAsset: {loaded} ");
            suffixToFontAsset[rule.suffix] = loaded;
        }

        suffixToFontAsset.TryGetValue("_lang", out fontAsset);

        if (fontAsset == null) WriteLog.LogError($"載入字體錯誤 fontAsset: {fontAsset} ");
        WriteLog.Log($"TmpFont 字體使用: {fontAsset} ");
    }

    public static void SetFontAssetUnder(GameObject _root, bool _includeInactive = true, bool _ignoreCase = true) {
        if (_root == null) {
            Debug.LogWarning("SetFontAssetUnder: root 為 null。");
            return;
        }

        if (suffixToFontAsset == null || suffixToFontAsset.Count == 0) {
            Debug.LogWarning("SetFontAssetUnder: suffixToFont 未指定");
            return;
        }

        int count = 0;
        var tmps = _root.GetComponentsInChildren<TextMeshProUGUI>(_includeInactive);

        foreach (var tmp in tmps) {
            if (tmp == null) continue;
            string objName = tmp.gameObject.name;

            if (TryGetFontAssetByName(objName, _ignoreCase, out var targetFontAsset)) {
                tmp.font = targetFontAsset;
                setMaterialPreset(tmp);
                // WriteLog.Log($"tmp={tmp.name} font={fontAsset.name}");
                tmp.SetAllDirty();
                tmpList_waitToUpdate.Add(tmp);
                count++;
            }
        }
        UpdateTMPs();
    }
    public static void SetTarget(TextMeshProUGUI _tmp) {
        if (_tmp == null) return;

        if (!TryGetFontAssetByName(_tmp.gameObject.name, true, out var targetFontAsset)) {
            targetFontAsset = fontAsset;
        }
        if (targetFontAsset == null) return;

        _tmp.font = targetFontAsset;
        setMaterialPreset(_tmp);
        _tmp.SetAllDirty();
        tmpList_waitToUpdate.Add(_tmp);
        UpdateTMPs();
    }

    /// <summary>
    /// 更新TMP FontAsset 不然更改過FontAsset後可能會變亂碼
    /// </summary>
    static void UpdateTMPs() {
        if (tmpList_waitToUpdate == null || tmpList_waitToUpdate.Count == 0) return;
        foreach (var tmp in tmpList_waitToUpdate) {
            tmp.UpdateFontAsset();
        }
        tmpList_waitToUpdate.Clear();
    }

    static void setMaterialPreset(TextMeshProUGUI _tmp) {
        if (_tmp == null) return;
        var tmpExtraData = _tmp.GetComponent<TMPTextExtraData>();
        if (tmpExtraData == null) return;
        //WriteLog.LogError(_tmp.gameObject.name);
        switch (GamePlayer.Instance.UsingLanguage) {
            case Language.EN:
                _tmp.fontSharedMaterial = tmpExtraData.Mat_EN;
                break;
            case Language.CH:
                _tmp.fontSharedMaterial = tmpExtraData.Mat_CH;
                break;
            case Language.VN:
                _tmp.fontSharedMaterial = tmpExtraData.Mat_VN;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Tmp字體設定 使用物件名字尾來全遊戲的設定 FontAsset
    /// </summary>
    public static void SetAllFontAssets() {
        DateTime before = DateTime.Now;
        if (suffixToFontAsset == null || suffixToFontAsset.Count == 0) return;
        TMPFontSetter.SetFontAsset(false);
        UpdateTMPs();
        WriteLog.Log($"完成 setFontAsset 花費: {(DateTime.Now - before).TotalMilliseconds} 毫秒");
    }


    public static int SetFontAsset(bool _ignoreCase = true) {
        if (suffixToFontAsset == null || suffixToFontAsset.Count == 0) {
            Debug.LogWarning("fontAsset 未指定");
            return 0;
        }

        int count = 0;
        var tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var tmp in tmps) {
            if (tmp == null) continue;
            string objName = tmp.gameObject.name;

            if (TryGetFontAssetByName(objName, _ignoreCase, out var targetFontAsset)) {
                tmp.font = targetFontAsset;
                setMaterialPreset(tmp);
                //WriteLog.Log($"tmp={tmp.name} font={fontAsset.name}");
                tmp.SetAllDirty();
                tmpList_waitToUpdate.Add(tmp);
                count++;
            }
        }
        return count;
    }

}
