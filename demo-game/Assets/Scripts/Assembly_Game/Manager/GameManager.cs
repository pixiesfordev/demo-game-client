using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.Rendering.Universal;
using tower.main;
using Cysharp.Threading.Tasks;
using System.Globalization;

namespace Scoz.Func {
    public enum DataLoad {
        GameDic,
        FirestoreData,
        AssetBundle
    }

    public class GameManager : MonoBehaviour {
        public static GameManager Instance;
        public static bool IsInit { get; private set; } = false;
        private static bool IsFinishedLoadAsset = false; //是否已經完成初始載包

        /// <summary>
        /// 取得遊戲的完整版號 E.X. 1.2.3中 1.2是主程式版本3是Bundle版本
        /// 主程式版本是包主程式跟NewBuild前自己填
        /// Bundle版本是NewBuild時會重置回1，之後UpdatePreviousBuild會自動加版號
        /// </summary>
        public static string GameFullVersion {
            get {
                var largeVer = Application.version; // 主程式版本
                var env = GetEnvVersion.ToString().ToUpper(); // 環境
                var plat = GetPlatformUpperStr(); // 平台
                var bundle = BundleVersion.GetBundleVer(env, plat, largeVer); // Bundle 版本
                return $"{largeVer}.{bundle}";
            }
        }

        public const string PROJECT_NAME = "waifu-tower";

        public static string GetPlatformUpperStr() {
#if UNITY_WEBGL
            return "WEBGL";
#elif UNITY_ANDROID
        return "ANDROID";
#elif UNITY_IOS
        return "IOS";
#else
            return "Error";
#endif
        }

        public static EnvVersion GetEnvVersion {
            get {
#if Dev
                return EnvVersion.Dev;
#elif Test
                return EnvVersion.Test;
#elif Release
                return EnvVersion.Release;
#else
                return EnvVersion.Dev;
#endif
            }
        }

        public static bool IsMobileDevice { get; private set; } = true; // 是否跑在行動裝置上
        public static int TimeZoneOffsetHours { get; private set; } = 0; // 時區預設+0

        [HeaderAttribute("==============AddressableAssets==============")]
        [SerializeField]
        private AssetReference PopupUIAsset;

        [SerializeField] private AssetReference AddressableManageAsset;
        [SerializeField] private AssetReference GameDictionaryAsset;
        [SerializeField] private AssetReference UICanvasAsset;
        [SerializeField] private AssetReference UICamAsset;
        [SerializeField] private AssetReference TestToolAsset;


        [HeaderAttribute("==============場景對應入口UI==============")]
        [SerializeField]
        private AssetReference Asset_MainSceneUI;

        public static Dictionary<MyScene, AssetReference> SceneUIAssetDic = new(); //字典對應UI字典


        [HeaderAttribute("==============遊戲設定==============")]
        public int TargetFPS = 60;

        [SerializeField] private TMPFontSetter MyTMPFontSetter;
        public static bool InitGetTokenSuccess { get; private set; } // 開始遊戲時會先取Token如果這一步失敗就不用跑後續

        public static EnvVersion CurVersion {
            //取得目前版本
            get {
#if Dev
                return EnvVersion.Dev;
#elif Test
                return EnvVersion.Test;
#elif Release
                return EnvVersion.Release;
#else
                return EnvVersion.Dev;
#endif
            }
        }

        private DateTimeOffset LastServerTime;
        private DateTimeOffset LastClientTime;

        public DateTimeOffset NowTime {
            get {
                var span = DateTimeOffset.Now - LastClientTime;
                return LastServerTime.AddSeconds(span.TotalSeconds);
            }
        }

        /// <summary>
        /// 返回本機時間與UTC+0的時差
        /// </summary>
        public int LocoHourOffset => (int)TimeZoneInfo.Local.BaseUtcOffset.TotalHours;

        /// <summary>
        /// 返回本機時間與Server的時差
        /// </summary>
        public int LocoHourOffsetToServer => (int)(DateTimeOffset.Now - NowTime).TotalHours;

        private void Start() {
            WriteLog.LogColor("GameAssembly的GameManager載入成功", WriteLog.LogType.Addressable);
            Instance = this;
            Instance.Init();
        }


        public void SetTime(DateTimeOffset _serverTime) {
            LastServerTime = _serverTime;
            LastClientTime = DateTimeOffset.Now;
            WriteLog.Log("Get Server Time: " + LastServerTime);
        }

        public void Init() {
            if (IsInit) return;
            SceneUIAssetDic.Add(MyScene.MainScene, Asset_MainSceneUI);
            setCulture();
            Instance = this;
            IsInit = true;
            WriteLog.Log($"GameFullVersion: {GameFullVersion}");
            DontDestroyOnLoad(gameObject);
            //設定FPS與垂直同步
#if Dev
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 100;
#else
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFPS;
#endif
            // 產生一個新玩家
            new GamePlayer();
            // 初始化字體設定工具
            MyTMPFontSetter.Init().Forget();
            //建立InternetChecker
            gameObject.AddComponent<InternetChecker>().Init();
            //建立DeviceManager
            gameObject.AddComponent<DeviceManager>();
            // 建立AudioPlayer
            AudioPlayer.Caeate();
            // 初始化UnityAssemblyCaller
            UnityAssemblyCaller.Init();
            //UserAPIWrapper.Init();

            //初始化文字取代工具
            StringReplacer.Init();

            // 建立AddressableManage並開始載包
            StartDownloadAddressableAsync().Forget();
            setIsMobileDevice();
            setTimezone();
        }

        /// <summary>
        /// 越南文中的.是, 例如3.14 會顯示成3,14 所以忽略瀏覽器的語系設定
        /// </summary>
        private void setCulture() {
            WriteLog.Log("set InvariantCulture");
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        private void setIsMobileDevice() {
            var result = UnityAssemblyCaller.Invoke("JSFuncWrapper", "IsMobileBrowser", true);
            IsMobileDevice = result is bool b && b;
            WriteLog.Log($"是否在行動裝上執行: {IsMobileDevice}");
        }

        private void setTimezone() {
            var result = UnityAssemblyCaller.Invoke("JSFuncWrapper", "GetTimeZoneOffsetHours", true);
            TimeZoneOffsetHours = Convert.ToInt32(result);
            WriteLog.Log($"本地時區為: {TimeZoneOffsetHours}");
        }

        private async UniTask<(bool, string)> initConnector() {
            // 建立連線物件
            gameObject.AddComponent<GameConnetor>().Init();
            //var gotToken = await GameConnetor.Instance.TokenCheck();
            //if (!gotToken) {
            //    return (false, "empty token");
            //}
            GameAPIManager.InitPing().Forget(); // 開始遊戲時會嘗試Ping測試延遲
            //var (refreshResult, errorStr) = await GameConnetor.Instance.RefreshToken();
            //if (refreshResult != RefreshTokenResult.OK) {
            //    return (false, errorStr);
            //}
            var (success, error) = await GameConnetor.Instance.Connect();
            return (success, error);
        }

        public async UniTask StartDownloadAddressableAsync() {
            try {
                var before = DateTime.Now;

                // 1. 載入 AddressableManage
                var addressableManagerGO = await LoadPrefabInstanceAsync(AddressableManageAsset);
                addressableManagerGO.GetComponent<AddressableManage>().Init();
                WriteLog.LogColor($"載入 AddressableManage 花費 {(DateTime.Now - before).TotalSeconds} 秒",
                    WriteLog.LogType.Addressable);
                before = DateTime.Now;

                // 2. 載入 GameDictionary
                var dicGO = await LoadPrefabInstanceAsync(GameDictionaryAsset);
                dicGO.GetComponent<GameDictionary>().InitDic();
                WriteLog.LogColor($"載入 GameDictionary 花費 {(DateTime.Now - before).TotalSeconds} 秒",
                    WriteLog.LogType.Addressable);
                before = DateTime.Now;

                // 3. 載入 JSON 資料
                await LoadJsonDataToDicAsync();
                WriteLog.LogColor($"載入 JSON 花費 {(DateTime.Now - before).TotalSeconds} 秒", WriteLog.LogType.Addressable);
                before = DateTime.Now;

                // 4. 初始化連線與玩家本地資料
                GamePlayer.Instance.LoadLocoData();
                var (initGetTokenSuccess, connErrorStr) = await initConnector();
                InitGetTokenSuccess = initGetTokenSuccess;
                WriteLog.LogColor($"initConnector 花費 {(DateTime.Now - before).TotalSeconds} 秒",
                    WriteLog.LogType.Connection);
                before = DateTime.Now;

                // 5. 載入 UICam
                var camGo = await LoadPrefabInstanceAsync(UICamAsset);
                camGo.GetComponent<UICam>().Init();
                WriteLog.LogColor($"載入 UICam 花費 {(DateTime.Now - before).TotalSeconds} 秒",
                    WriteLog.LogType.Addressable);
                before = DateTime.Now;

                // 6. 預載其他資源
                await StartPreloadAssetsAsync();
                WriteLog.LogColor($"預載其他資源 花費 {(DateTime.Now - before).TotalSeconds} 秒", WriteLog.LogType.Addressable);
                before = DateTime.Now;


                // 7. 載入 Popup UI
                await SpawnPopupUIAsync();


                // 8. 開始跑連線刷新Loop (要在SpawnPopupUIAsync之後 不然有錯誤也沒辦法呼叫彈窗)
                GameConnetor.Instance.StartRefreshLoop();

                // 初始連線Token驗證沒過就不用跑後續
                if (InitGetTokenSuccess == false) {
                    await UniTask.SwitchToMainThread();
                    GameConnetor.Instance.LeaveGame(
                        $"{JsonString.GetUIString("AuthFail_Content")} error_code: {connErrorStr}");
                    return;
                }

                // 9. 建立其他物件
                CreateAddressableObjs();
                WriteLog.LogColor($"建立其他物件 花費 {(DateTime.Now - before).TotalSeconds} 秒", WriteLog.LogType.Addressable);
                before = DateTime.Now;
                IsFinishedLoadAsset = true;
                SpawnSceneUI();
            } catch (Exception ex) {
                Debug.LogError($"載入流程發生錯誤：{ex}");
            }
        }

        private UniTask<GameObject> LoadPrefabInstanceAsync(AssetReference _asset) {
            var tcs = new UniTaskCompletionSource<GameObject>();
            AddressablesLoader.GetPrefabByRef(_asset, (prefab, handle) => {
                var go = Instantiate(prefab);
                tcs.TrySetResult(go);
                Addressables.Release(handle);
            });
            return tcs.Task;
        }

        // callback 版載入 JSON
        private UniTask LoadJsonDataToDicAsync() {
            var tcs = new UniTaskCompletionSource();
            GameDictionary.LoadJsonDataToDic(() => tcs.TrySetResult());
            return tcs.Task;
        }

        // SpawnPopupUI
        private UniTask SpawnPopupUIAsync() {
            var tcs = new UniTaskCompletionSource();
            Instance.SpawnPopupUI(() => tcs.TrySetResult());
            return tcs.Task;
        }

        // 預載其他 Addressable
        private UniTask StartPreloadAssetsAsync() {
            var tcs = new UniTaskCompletionSource();
            AddressableManage.Instance.StartLoadAsset(() => tcs.TrySetResult());
            return tcs.Task;
        }

        /// <summary>
        /// 根據所在Scene產生UI
        /// 1. 開始遊戲後GameManager跑StartDownloadAddressable完最後會跑這個func
        /// 2. 切換場景會跑這個func(透過AOT反射呼叫)
        /// </summary>
        public static void SpawnSceneUI() {
            if (!IsFinishedLoadAsset) return;
            AddressablesLoader.GetPrefabByRef(Instance.UICanvasAsset, (canvasPrefab, handle) => {
                //載入UICanvas
                var canvasGO = Instantiate(canvasPrefab);
                canvasGO.GetComponent<UICanvas>().Init();
                var myScene = MyEnum.ParseEnum<MyScene>(SceneManager.GetActiveScene().name);
                AddressablesLoader.GetPrefabByRef(SceneUIAssetDic[myScene], (prefab, handle) => {
                    var go = Instantiate(prefab);
                    go.transform.SetParent(canvasGO.transform);
                    go.transform.localPosition = prefab.transform.localPosition;
                    go.transform.localScale = prefab.transform.localScale;
                    var rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0, 0);
                    rect.anchorMax = new Vector2(1, 1);
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                });
            });
        }

        private void SpawnPopupUI(Action _ac) {
            //載入PopupUI
            AddressablesLoader.GetPrefabByRef(Instance.PopupUIAsset, (prefab, handle) => {
                var go = Instantiate(prefab);
                go.GetComponent<PopupUI>().Init();
                go.transform.localPosition = Vector2.zero;
                go.transform.localScale = Vector3.one;
                var rect = go.GetComponent<RectTransform>();
                rect.offsetMin = Vector2.zero; //Left、Bottom
                rect.offsetMax = Vector2.zero; //Right、Top
                Addressables.Release(handle);
                _ac?.Invoke();
            });
        }

        private void CreateAddressableObjs() {
#if Dev
            //載入TestTool
            AddressablesLoader.GetPrefabByRef(Instance.TestToolAsset, (prefab, handle) => {
                var go = Instantiate(prefab);
                go.GetComponent<TestTool>().Init();
                Addressables.Release(handle);
            });
#endif
        }

        /// <summary>
        /// 將指定camera加入到目前場景上的MainCameraStack中
        /// </summary>
        public void AddCamStack(Camera _cam) {
            if (_cam == null) return;
            var mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (mainCam == null) return;
            var cameraData = mainCam.GetUniversalAdditionalCameraData();
            if (cameraData == null) return;
            cameraData.cameraStack.Add(_cam);
        }
    }
}