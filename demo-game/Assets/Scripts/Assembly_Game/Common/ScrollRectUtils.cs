using Scoz.Func;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ScrollRectUtils {
    public static void ScrollToTargetY(ScrollRect scrollRect, RectTransform target, float offsetY) {
        if (scrollRect == null || target == null) {
            Debug.LogWarning("ScrollRect or target RectTransform is null.");
            return;
        }

        Canvas.ForceUpdateCanvases();

        var contentRT = scrollRect.content;
        var viewportRT = scrollRect.viewport;
        Vector2 contentLocalPos = (Vector2)contentRT.InverseTransformPoint(target.position) - (new Vector2(0.5f, 0) - contentRT.pivot) * contentRT.rect.size;
        float offset = contentLocalPos.y - offsetY;
        float t = Mathf.InverseLerp(0, contentRT.rect.height - viewportRT.rect.height, offset);
        float normalizedY = Mathf.Clamp01(t);
        scrollRect.verticalNormalizedPosition = normalizedY;
    }
}
