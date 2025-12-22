using DG.Tweening;
using Scoz.Func;
using TMPro;
using UnityEngine;

namespace tower.main {
    public class TipPopup : MonoBehaviour {
        [SerializeField] TextMeshProUGUI Txt_Info;
        RectTransform rect_popup;

        private CanvasGroup canvasGroup;
        private Sequence currentSequence;
        private Vector2 centerPosition;

        const float DURATION = 2f; // tip顯示時間
        const float SLIDE_DISTANCE = 50f; // 滑入滑出距離
        const float FADE_DURATION = 0.1f; // 淡入淡出時間
        const float SLIDE_DURATION = 0.3f; // 滑入滑出時間
        const float OVERSHOOT_RATIO = 0f; // 超過正中央的距離比例

        public void Init() {
            rect_popup = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();

            // 設置anchor到中心
            if (rect_popup.anchorMin != new Vector2(0.5f, 0.5f) || rect_popup.anchorMax != new Vector2(0.5f, 0.5f)) {
                rect_popup.anchorMin = new Vector2(0.5f, 0.5f);
                rect_popup.anchorMax = new Vector2(0.5f, 0.5f);
                rect_popup.pivot = new Vector2(0.5f, 0.5f);
            }

            // 記錄中心位置
            centerPosition = rect_popup.anchoredPosition;

            gameObject.SetActive(false);
        }

        public void ShowTip(string _text, float _duration = 0) {
            // 先停掉之前的動態
            if (currentSequence != null && currentSequence.IsActive()) {
                currentSequence.Kill();
            }

            Txt_Info.text = _text;

            gameObject.SetActive(true);
            canvasGroup.alpha = 0;
            rect_popup.anchoredPosition = new Vector2(centerPosition.x - SLIDE_DISTANCE, centerPosition.y); // 從左邊近來

            currentSequence = DOTween.Sequence();
            float overshootDistance = SLIDE_DISTANCE * OVERSHOOT_RATIO; // 計算滑超過正中心的距離
            float overshootPositionX = centerPosition.x + overshootDistance; // 超過中心的位置

            // 淡入+滑入
            currentSequence.Append(
                rect_popup.DOAnchorPosX(overshootPositionX, SLIDE_DURATION)
                    .SetEase(Ease.OutQuad)
            ).Join(
                canvasGroup.DOFade(1f, FADE_DURATION)
                    .SetEase(Ease.Linear)
            );

            // 回彈
            currentSequence.Append(
                rect_popup.DOAnchorPosX(centerPosition.x, SLIDE_DURATION)
                    .SetEase(Ease.OutBack)
            );

            // 等待顯示時間
            currentSequence.AppendInterval(_duration != 0 ? _duration : DURATION);

            // 淡出+滑出
            currentSequence.Append(
                rect_popup.DOAnchorPosX(centerPosition.x + SLIDE_DISTANCE, SLIDE_DURATION)
                    .SetEase(Ease.Linear)
            ).Join(
                canvasGroup.DOFade(0f, FADE_DURATION)
                    .SetEase(Ease.Linear)
            ).OnComplete(() => {
                gameObject.SetActive(false);
                currentSequence = null;
            });

            // 播放動畫
            currentSequence.Play();
        }
    }
}