using Scoz.Func;
using UnityEngine;

public static class ParticleSystemLayerSetter_UnityAssembly {
    /// <summary>
    /// 遞迴設定指定 GameObject (含子物件) 裡所有 ParticleSystem 的 Renderer 屬性
    /// _addOrderInLayer 代表要在原本的 sortingOrder上增加多少
    /// </summary>
    public static void SetParticleLayerAndOrder(GameObject _root, string _layerName, int _addOrderInLayer) {
        if (_root == null) return;

        // 找出所有子物件的 ParticleSystemRenderer
        var renderers = _root.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true);
        foreach (var renderer in renderers) {
            renderer.sortingLayerName = _layerName;
            renderer.sortingOrder = renderer.sortingOrder + _addOrderInLayer;
        }
    }

    /// <summary>
    /// 找到父層中有效的 Canvas 的 sortingLayer 跟 orderInLayer
    /// </summary>
    public static (bool, string, int) TryGetCanvasSorting(GameObject _go) {
        if (_go == null) return (false, null, 0);

        // 抓所有父層 Canvas (包含自己)
        var canvases = _go.GetComponentsInParent<Canvas>(includeInactive: true);
        if (canvases == null || canvases.Length == 0) return (false, null, 0);

        // 先找第一個 overrideSorting == true 的
        foreach (var c in canvases) {
            if (c.overrideSorting) {
                return (true, c.sortingLayerName, c.sortingOrder);
            }
        }
        // 如果都沒有 overrideSorting，就回傳最近的
        var nearest = canvases[0];
        return (true, nearest.sortingLayerName, nearest.sortingOrder);
    }
}
