using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Scoz.Func;

/// <summary>
///  GameObject 物件池：以 prefabName 為 key
/// GetAsync：若已存在則重新使用並 SetActive(true)，並設定 parent 與 localPosition
/// </summary>
public class GameObjectPool {
    private readonly Dictionary<string, GameObject> pool = new();

    /// <summary>
    /// 取得(或建立)指定 prefabName 的實例，並放在指定 parent / localPosition
    /// 回傳<物件,是否產生新的物件>
    /// </summary>
    public async UniTask<(GameObject, bool)> GetAsync(string prefabName, Transform parent, Vector3 localPosition) {
        if (pool.TryGetValue(prefabName, out var go) && go != null) {
            if (go.transform.parent != parent) go.transform.SetParent(parent, false);

            go.transform.localPosition = localPosition;
            go.SetActive(true);
            return (go, false);
        }

        // 沒有可以用的就載入並建立新實例
        var (prefab, _) = await AddressablesLoader.GetPrefab(prefabName);
        var newObj = GameObject.Instantiate(prefab, parent);
        newObj.transform.localPosition = localPosition;
        newObj.SetActive(true);

        pool[prefabName] = newObj;
        return (newObj, true);
    }

    /// <summary>
    /// 將指定 prefabName 的物件設為不啟用(不 Destroy)
    /// </summary>
    public void SetActive(string prefabName) {
        if (pool.TryGetValue(prefabName, out var go) && go != null)
            go.SetActive(false);
    }

    /// 清空物件池
    public void Clear() {
        foreach (var kv in pool) if (kv.Value != null) GameObject.Destroy(kv.Value);
        pool.Clear();
    }
}
