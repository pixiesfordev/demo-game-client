using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;


namespace Scoz.Func {
    public partial class TestTool : MonoBehaviour {
        public static TestTool Instance;
        [SerializeField] private TextMeshProUGUI EnvText;
        [SerializeField] private TextMeshProUGUI Text_FPS;
        [SerializeField] private TextMeshProUGUI VersionText;
        [SerializeField] private TextMeshProUGUI ResolutionText;
        public float InfoRefreshInterval = 0.5f;

        private int FrameCount = 0;
        private float PassTimeByFrames = 0.0f;
        private float LastFrameRate = 0.0f;

        public Camera GetCamera() {
            return GetComponent<Camera>();
        }

        private void OnDestroy() {
        }

        public void Init() {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            VersionText.text = "Ver: " + GameManager.GameFullVersion;
            ResolutionText.text = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
            showResolution().Forget();


#if Dev
            EnvText.text = EnvVersion.Dev.ToString();
#elif Test
            EnvText.text = EnvVersion.Test.ToString();
#elif Release
            EnvText.text = EnvVersion.Release.ToString();
#endif
        }

        /// <summary>
        /// Unity 內部 FPS 計算
        /// </summary>
        private void FPSCalc() {
            if (Text_FPS == null || !Text_FPS.isActiveAndEnabled) return;
            if (PassTimeByFrames < InfoRefreshInterval) {
                PassTimeByFrames += Time.deltaTime;
                FrameCount++;
            } else {
                LastFrameRate = (float)FrameCount / PassTimeByFrames;
                FrameCount = 0;
                PassTimeByFrames = 0.0f;
            }

            Text_FPS.text = string.Format("FPS: {0}", Mathf.Round(LastFrameRate).ToString());
        }

        private void Update() {
            FPSCalc();
        }

        private async UniTaskVoid showResolution() {
            while (isActiveAndEnabled) {
                await UniTask.Delay(5000);
                ResolutionText.text = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
            }
        }
    }
}