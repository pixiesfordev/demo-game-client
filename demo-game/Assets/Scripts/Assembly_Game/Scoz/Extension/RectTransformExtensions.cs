using UnityEngine;

public static class RectTransformExtensions {
    static readonly Vector3[] _corners = new Vector3[4];

    /// <summary>
    /// 改 pivot 並補償位置，確保畫面不位移。
    /// 以世界座標的矩形中心為參考，適用 Stretch/旋轉/縮放 等各種情況。
    /// </summary>
    public static void SetPivotWithoutMoving(this RectTransform rt, Vector2 newPivot) {
        if (rt == null) return;
        if (rt.pivot == newPivot) return;

        // 1) 記下改 pivot 前的世界中心
        rt.GetWorldCorners(_corners);
        Vector3 worldCenterBefore = (_corners[0] + _corners[2]) * 0.5f;

        // 2) 改 pivot
        rt.pivot = newPivot;
        Canvas.ForceUpdateCanvases(); // 讓幾何立即更新

        // 3) 計算改後世界中心，並反向補償 Transform 的世界位置
        rt.GetWorldCorners(_corners);
        Vector3 worldCenterAfter = (_corners[0] + _corners[2]) * 0.5f;
        Vector3 delta = worldCenterAfter - worldCenterBefore;

        rt.position -= delta; // 用世界座標補償 → 目視不會動
    }
}
