using UnityEngine;
using UnityEngine.UI;

public class Debugger : MonoBehaviour {
    [Header("UI")]
    [SerializeField]
    private Text fpsText;

    [Header("顯示設定")]
    [SerializeField]
    private string format = "FPS: {0:0}";

    [SerializeField]
    private float refreshInterval = 0.2f;

    private float timer;
    private int frameCount;

    void Update() {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer < refreshInterval) return;

        float fps = frameCount / timer;

        if (fpsText != null) {
            fpsText.text = string.Format(format, fps);
        }

        frameCount = 0;
        timer = 0f;
    }
}
