using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Cysharp.Threading.Tasks;
using tower.Main;

namespace Scoz.Func {

    [Serializable]
    public sealed class AddressableManage_UnityAssembly : MonoBehaviour {
        public static AddressableManage_UnityAssembly Instance;

        public List<string> Keys = null;
        [SerializeField] Image ProgressImg = null;
        [SerializeField] TextMeshProUGUI Txt_Progress;
        [SerializeField] GameObject GO_LoadingUI = null;
        [SerializeField] GameObject GO_Bar = null;
        [SerializeField] GameObject GO_LoadingFX;
        [SerializeField] Image Img_Logo;
        Coroutine CheckInternetCoroutine = null;
        Action FinishedAction = null;

        const int CHECK_DOWNLOAD_MILISEC = 500; // 等待是否顯示下載條(毫秒) Webgl似乎使用Addressable沒有可靠方法取到真實要下載大小(即使載過了還是會顯示原大小)

        static HashSet<AsyncOperationHandle> ResourcesToReleaseWhileChangingScene = new HashSet<AsyncOperationHandle>();//加入到此清單的資源Handle會在切場景時一起釋放
        void Awake() {
            Instance = this;
            var lang = BaseManager.UsingLanguage;
            Img_Logo.sprite = Resources.Load<Sprite>($"Sprites/logo_{lang}");
            ShowBar(false);
            setSortingLayer(GO_LoadingFX);
            ShowDownloadUI(true);
        }
        void Start() {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
        }

        /// <summary>
        /// 設定特效物件放在UI上
        /// </summary>
        void setSortingLayer(GameObject _go) {
            var (found, sortingLyaer, sortingOrder) = ParticleSystemLayerSetter_UnityAssembly.TryGetCanvasSorting(_go);
            if (!found) return;
            ParticleSystemLayerSetter_UnityAssembly.SetParticleLayerAndOrder(_go, sortingLyaer, sortingOrder + 1);
        }

        /// <summary>
        /// 加入到此清單的資源Handle會在切場景時一起釋放
        /// </summary>
        /// <param name="_handle">要釋放的Addressables Handle</param>
        public static void SetToChangeSceneRelease(AsyncOperationHandle _handle) {
            if (ResourcesToReleaseWhileChangingScene.Contains(_handle)) return;
            ResourcesToReleaseWhileChangingScene.Add(_handle);
        }


        public static AddressableManage_UnityAssembly CreateNewAddressableManage() {
            if (Instance != null) {
            } else {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Common/AddressableManage_UnityAssembly");
                GameObject go = Instantiate(prefab);
                go.name = "AddressableManage_UnityAssembly";
                Instance = go.GetComponent<AddressableManage_UnityAssembly>();
                DontDestroyOnLoad(Instance.gameObject);
            }
            return Instance;
        }
        async UniTask ClearAllCache(Action _cb) {
            await UniTask.Yield();
            //顯示載入進度文字
            Txt_Progress.text = StringJsonData_UnityAssembly.GetUIString("Loading");
            WriteLog_UnityAssembly.Log("重新載入中....................");
            _cb?.Invoke();
        }
        Coroutine Downloader;
        public void StartLoadAsset(Action _action) {
            WriteLog_UnityAssembly.LogColor("LoadAsset-Start", WriteLog_UnityAssembly.LogType.Addressable);
            Keys.RemoveAll(a => a == "");
#if UNITY_EDITOR
            Keys.RemoveAll(a => a == "Dlls");//編輯器不需要載入Dlls
#endif
            FinishedAction = _action;
            if (Downloader != null) StopCoroutine(Downloader);
            LoadAssets().Forget();//不輕快取用這個(正式版)
        }
        void OnClearCatchCB() {
            if (Downloader != null) StopCoroutine(Downloader);
            LoadAssets().Forget();
        }
        public void ReDownload() {
            if (Downloader != null) StopCoroutine(Downloader);
            ClearAllCache(OnClearCatchCB).Forget();
        }

        async UniTask LoadAssets() {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

            if (Keys == null || Keys.Count == 0) {
                OnFinishedDownload();
                return;
            }

            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(Keys);
            await getDownloadSize.Task;
            long totalSize = getDownloadSize.Result;
            WriteLog_UnityAssembly.LogColorFormat("LoadAsset-TotalSize={0}", WriteLog_UnityAssembly.LogType.Addressable, MyMath_UnityAssembly.BytesToMB(totalSize).ToString("0.00"));

            if (CheckInternetCoroutine != null)
                StopCoroutine(CheckInternetCoroutine);

            var curDownloading = Addressables.DownloadDependenciesAsync(Keys, Addressables.MergeMode.Union);

            if (curDownloading.IsDone) {
                Addressables.Release(curDownloading);
                OnFinishedDownload();
                return;
            }

            float deadline = Time.realtimeSinceStartup + CHECK_DOWNLOAD_MILISEC / 1000f;
            while (Time.realtimeSinceStartup < deadline) {
                if (curDownloading.IsDone) {
                    ShowBar(false);
                    Addressables.Release(curDownloading);
                    OnFinishedDownload();
                    return;
                }
                await UniTask.Yield();
            }

            ShowBar(true);
            await Loading(curDownloading, totalSize);
        }
        async UniTask Loading(AsyncOperationHandle curDownloading, long _totalSize) {
            bool downloading = true;
            while (downloading) {
                float curDownloadPercent = curDownloading.GetDownloadStatus().Percent;
                long curDownloadSize = (long)(curDownloadPercent * _totalSize);

                //顯示載入進度與文字
                ProgressImg.fillAmount = curDownloadPercent;
                Txt_Progress.text = string.Format(StringJsonData_UnityAssembly.GetUIString("AssetUpdating"), MyMath_UnityAssembly.BytesToMB(curDownloadSize).ToString("0.00"), MyMath_UnityAssembly.BytesToMB(_totalSize).ToString("0.00"));
                //完成後跳出迴圈

                if (curDownloading.IsDone) {
                    Addressables.Release(curDownloading); // Addressable1.21.15版本更新後，必須要在載完資源後釋放，否則LoadAssetAsync會取不到資源
                    downloading = false;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
            OnFinishedDownload();
        }

        void OnFinishedDownload() {
            WriteLog_UnityAssembly.LogColorFormat("LoadAsset-Finished", WriteLog_UnityAssembly.LogType.Addressable);
            FinishedAction?.Invoke();

        }
        public static void PreLoadToMemory(Action _ac = null) {
            DateTime now = DateTime.Now;
            WriteLog_UnityAssembly.LogErrorFormat("開始下載MaJam資源圖");
            //初始化UI
            Addressables.LoadAssetsAsync<Texture>("MaJam", null).Completed += handle => {
                WriteLog_UnityAssembly.LogErrorFormat("載入MaJam花費: {0}秒", (DateTime.Now - now).TotalSeconds);
                _ac?.Invoke();
            };
        }

        public static void ShowDownloadUI(bool _show) {
            Instance.GO_LoadingUI.gameObject.SetActive(_show);
        }

        public void ShowBar(bool _show) {
            GO_Bar.gameObject.SetActive(_show);
            if (_show) {
                ProgressImg.fillAmount = 0;
                Instance.Txt_Progress.text = StringJsonData_UnityAssembly.GetUIString("Loading");

            }
        }



        /// <summary>
        /// 傳入Addressable的key確認此Addressable是否存在
        /// </summary>
        public void CheckIfAddressableExist(List<string> _keys, Action<bool> _cb) {
            CheckIfAddressableExistCoroutine(_keys, _cb).Forget();
        }
        async UniTaskVoid CheckIfAddressableExistCoroutine(List<string> _keys, Action<bool> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(false);
                return;
            }

            var locationHandle = Addressables.LoadResourceLocationsAsync(_keys, Addressables.MergeMode.Union);
            await locationHandle.Task;
            if (locationHandle.Status != AsyncOperationStatus.Succeeded || locationHandle.Result == null || locationHandle.Result.Count == 0) {
                _cb?.Invoke(false);
                Addressables.Release(locationHandle);
                return;
            }
            _cb?.Invoke(true);
        }

        /// <summary>
        /// 傳入Addressable的key取得下載大小(mb)(用於App玩到一半才有載入需求的資源)
        /// </summary>
        public void GetDownloadAddressableSize(List<string> _keys, Action<float> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    GetDownloadAddressableSizeCoroutine(_keys, _cb).Forget();
                } else {
                    _cb?.Invoke(0);
                }
            });

        }
        async UniTaskVoid GetDownloadAddressableSizeCoroutine(List<string> _keys, Action<float> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(0);
                return;
            }

            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            await getDownloadSize.Task;
            long totalSize = getDownloadSize.Result;
            float mb = MyMath_UnityAssembly.BytesToMB(totalSize);
            _cb?.Invoke(mb);
        }

        /// <summary>
        /// 傳入Addressable的key目標下載大小(mb)(用於App玩到一半才有載入需求的資源)
        /// </summary>
        public void DownloadAddressable(List<string> _keys, Action<bool> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    DownloadAddressableCheck(_keys, _cb).Forget();
                } else {
                    _cb?.Invoke(false);
                }
            });
        }
        async UniTaskVoid DownloadAddressableCheck(List<string> _keys, Action<bool> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(false);
                return;
            }

            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            await getDownloadSize.Task;

            long totalSize = getDownloadSize.Result;
            WriteLog_UnityAssembly.Log("Download TotalSize=" + totalSize);
            if (totalSize > 0) {//有要下載跳訊息
                ShowBar(true);
                await DownloadingAddressable(_keys, totalSize, _cb);
            } else {//沒需要下載就直接跳到完成
                ShowBar(false);
                _cb?.Invoke(true);
            }
        }
        async UniTask DownloadingAddressable(List<string> _keys, long _totalSize, Action<bool> _cb) {
            AsyncOperationHandle curDownloading = new AsyncOperationHandle();
            curDownloading = Addressables.DownloadDependenciesAsync(_keys, Addressables.MergeMode.Union);
            bool downloading = true;
            while (downloading) {

                float curDownloadPercent = curDownloading.GetDownloadStatus().Percent;
                long curDownloadSize = (long)(curDownloadPercent * _totalSize);

                //顯示載入進度與文字
                ProgressImg.fillAmount = curDownloadPercent;
                Txt_Progress.text = string.Format(StringJsonData_UnityAssembly.GetUIString("AssetUpdating"), MyMath_UnityAssembly.BytesToMB(curDownloadSize).ToString("0.00"), MyMath_UnityAssembly.BytesToMB(_totalSize).ToString("0.00"));
                //完成後跳出迴圈
                if (curDownloading.IsDone) {
                    Addressables.Release(curDownloading); // Addressable1.21.15版本更新後，必須要在載完資源後釋放，否則LoadAssetAsync會取不到資源
                    downloading = false;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }

            ShowBar(false);
            _cb?.Invoke(true);
        }

    }
}
