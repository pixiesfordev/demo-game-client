using LitJson;
using Scoz.Func;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace tower.Main {
    public class BaseManager : MonoBehaviour {
        [SerializeField] AssetReference GameManagerAsset;
        public static BaseManager Instance { get; private set; }
        public static bool IsInit { get; private set; } = false;
        public static Language UsingLanguage { get; private set; }

        public static BaseManager CreateNewInstance() {

            //在每一個場景的開使都會先呼叫BaseManager的CreateNewInstance
            //如果還沒初始化過(Instance為null)就會跑正式流程: 建立BaseManager > 下載資源包 > 建立GameManager
            //如果已經初始化過(Instance不為null)就會跳果載包等流程直接透過反射來呼叫GameManager的SpawnSceneUI方法

            if (Instance != null) {
                CallGameManagerFunc("SpawnSceneUI");
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/BaseManager");
                GameObject go = Instantiate(prefab);
                go.name = "BaseManager";
                Instance = go.GetComponent<BaseManager>();
                Instance.Init();
            }
            return Instance;
        }

        /// <summary>
        /// 呼叫GameAssembly的GameManager的靜態方法
        /// </summary>
        static void CallGameManagerFunc(string _func) {
            Assembly gameAssembly = Assembly.Load("Game");
            Type gameManager = gameAssembly.GetType("Scoz.Func.GameManager");
            MethodInfo spawnFunc = gameManager.GetMethod(_func);
            spawnFunc.Invoke(null, null);
        }

        void Init() {
            if (IsInit) return;
            IsInit = true;
            setURLLanguageSetting();
            DontDestroyOnLoad(gameObject);
            //建立遊戲資料字典
            //先初始化字典因為這樣會預先載入本機String表與GameSetting，之後在addressable載入後會取代本來String跟GameSetting
            GameDictionary_UnityAssembly.CreateNewInstance();

            SpawnSceneObjs();//生成場景限定
            SetJsonMapper();//設定LiteJson的JsonMapper    
            //建立AddressableManage並生成GameManager
            StartDownloadAddressablesAndSpawnGameManager();
        }

        public void setURLLanguageSetting() {
            var str = URLParamReader_UnityAssembly.GetStr("language");
            if (string.IsNullOrEmpty(str)) {
                if (!Application.isEditor) UsingLanguage = Language.EN;
                else UsingLanguage = Language.EN;
            } else {
                switch (str) {
                    case "en": UsingLanguage = Language.EN; break;
                    case "vn": UsingLanguage = Language.VN; break;
                    case "zh": UsingLanguage = Language.CH; break;
                    default: UsingLanguage = Language.EN; break;
                }
            }
        }

        /// <summary>
        /// 生成場景限定
        /// </summary>
        void SpawnSceneObjs() {

            var myScene = MyEnum_UnityAssembly.ParseEnum<MyScene>(SceneManager.GetActiveScene().name);
            switch (myScene) {
                case MyScene.MainScene:
                    //建立Popup_Local
                    //PopupUI_Local.CreateNewInstance();
                    //建立InternetChecker
                    gameObject.AddComponent<InternetChecker_UnityAssembly>().Init();
                    break;
            }
        }

        public void SetJsonMapper() {
            JsonMapper.RegisterImporter<int, long>((int value) => {
                return (long)value;
            });
        }

        /// <summary>
        /// 下載Buindle, 下載好後之後產生 GameManager, 之後都由GameAssembly的GameManager處理
        /// </summary>
        void StartDownloadAddressablesAndSpawnGameManager() {
            AddressableManage_UnityAssembly.CreateNewAddressableManage();
            WriteLog_UnityAssembly.LogColor("開始載Dll包", WriteLog_UnityAssembly.LogType.Addressable);
            DateTime before = DateTime.Now;
            AddressableManage_UnityAssembly.Instance.StartLoadAsset(async () => {
                WriteLog_UnityAssembly.LogColor($"Dll 下載花費: {(DateTime.Now - before).TotalSeconds} 秒", WriteLog_UnityAssembly.LogType.Addressable);
                before = DateTime.Now;
                await HybridCLRManager.LoadAssembly();//載入GameDll
                WriteLog_UnityAssembly.LogColor($"Dll 解析花費: {(DateTime.Now - before).TotalSeconds} 秒", WriteLog_UnityAssembly.LogType.HybridCLR);
                before = DateTime.Now;
                AddressablesLoader_UnityAssebly.GetPrefabByRef(GameManagerAsset, (gameManagerPrefab, handle) => {
                    WriteLog_UnityAssembly.LogColor($"GameManager建立花費: {(DateTime.Now - before).TotalSeconds} 秒", WriteLog_UnityAssembly.LogType.Addressable);
                    before = DateTime.Now;
                    var gameManager = Instantiate(gameManagerPrefab);
                });
            });
        }


        /// <summary>
        /// 將自己的camera加入到目前場景上的MainCameraStack中
        /// </summary>
        public void AddCamStack(Camera _cam) {
            if (_cam == null) return;
            Camera mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (mainCam == null) return;
            var cameraData = mainCam.GetUniversalAdditionalCameraData();
            if (cameraData == null) return;
            cameraData.cameraStack.Add(_cam);
        }
    }
}