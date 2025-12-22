using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Reflection;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using tower.main;

namespace Scoz.Func {

    [Serializable]
    public sealed class AddressableManage : MonoBehaviour {
        public static AddressableManage Instance;

        public List<string> Keys = null;
        [SerializeField] Image ProgressImg = null;
        [SerializeField] TextMeshProUGUI Txt_Progress, Txt_Ver;
        [SerializeField] GameObject GO_LoadingUI = null;
        [SerializeField] GameObject GO_Bar = null;
        [SerializeField] GameObject GO_LoadingFX;
        [SerializeField] Image Img_Logo;
        CancellationTokenSource CheckInternetCTS = null;
        Action FinishedAction = null;

        const int CHECK_DOWNLOAD_MILISEC = 500; // 等待是否顯示下載條(毫秒) Webgl似乎使用Addressable沒有可靠方法取到真實要下載大小(即使載過了還是會顯示原大小)

        static HashSet<AsyncOperationHandle> ResourcesToReleaseWhileChangingScene = new HashSet<AsyncOperationHandle>();//加入到此清單的資源Handle會在切場景時一起釋放
        public void Init() {
            Instance = this;
            setLogo();
            Txt_Ver.text = string.Format($"Ver. {GameManager.GameFullVersion}");
            DontDestroyOnLoad(Instance.gameObject);
            ShowBar(false);
            setSortingLayer(GO_LoadingFX);

            // 先關閉物件等Canvas設好後再打開(不然會瞬間壓過 AddressableManage_UnityAssembly 介面)
            ShowDownloadUI(false);
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }
        void setLogo() {
            Img_Logo.sprite = Resources.Load<Sprite>($"Sprites/logo_{GamePlayer.Instance.UsingLanguage}");
        }

        /// <summary>
        /// 設定特效物件放在UI上
        /// </summary>
        void setSortingLayer(GameObject _go) {
            var (found, sortingLyaer, sortingOrder) = ParticleSystemLayerSetter.TryGetCanvasSorting(_go);
            if (!found) return;
            ParticleSystemLayerSetter.SetParticleLayerAndOrder(_go, sortingLyaer, sortingOrder + 1);
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            WriteLog.LogColorFormat("開始釋放{0}個Addressables資源", WriteLog.LogType.Addressable, ResourcesToReleaseWhileChangingScene.Count);
            foreach (var handle in ResourcesToReleaseWhileChangingScene) Addressables.Release(handle);
            ResourcesToReleaseWhileChangingScene.Clear();
        }

        /// <summary>
        /// 加入到此清單的資源Handle會在切場景時一起釋放
        /// </summary>
        /// <param name="_handle">要釋放的Addressables Handle</param>
        public static void SetToChangeSceneRelease(AsyncOperationHandle _handle) {
            if (ResourcesToReleaseWhileChangingScene.Contains(_handle)) return;
            ResourcesToReleaseWhileChangingScene.Add(_handle);
        }

        async UniTask ClearAllCache(Action _cb) {
            await UniTask.Yield();
            Txt_Progress.text = JsonString.GetUIString("Loading");
            WriteLog.Log("重新載入中....................");
            _cb?.Invoke();
        }

        CancellationTokenSource DownloaderCTS;
        public async void StartLoadAsset(Action _action) {
            WriteLog.LogColor("LoadAsset-Start", WriteLog.LogType.Addressable);
            Keys.RemoveAll(a => a == "");
            FinishedAction = _action;
            DownloaderCTS?.Cancel();
            DownloaderCTS = new CancellationTokenSource();
            WriteLog.Log("設定AddressableManager Canvas");
            GetComponent<Canvas>().worldCamera = UICam.Instance.MyCam;
            ShowDownloadUI(true);
            LoadAssets(DownloaderCTS.Token).Forget();
            closeAssemblyCSharpAddressableManager();

        }
        void closeAssemblyCSharpAddressableManager() {
            WriteLog.Log("closeAssemblyCSharpAddressableManager");
            try {
                var asm = Assembly.Load("Assembly-CSharp");
                var t = asm.GetType("Scoz.Func.AddressableManage_UnityAssembly");
                var m = t.GetMethod("ShowDownloadUI", BindingFlags.Public | BindingFlags.Static);
                m.Invoke(null, new object[] { false });
            } catch (Exception ex) {
                WriteLog.LogError($"呼叫AddressableManage_UnityAssembly錯誤: {ex}");
            }
        }
        void OnClearCatchCB() {
            StartLoadAsset(FinishedAction);
        }
        public void ReDownload() {
            DownloaderCTS?.Cancel();
            ClearAllCache(OnClearCatchCB).Forget();
        }
        async UniTask WaitForCheckingBundle(CancellationToken token) {
            await UniTask.Delay(TimeSpan.FromSeconds(20), cancellationToken: token);
            PopupUI.ShowClickCancel(JsonString.GetUIString("Popup_SysInfo"), JsonString.GetUIString("Popup_GetBundleFail"), () => {
                Application.Quit();
            });
        }

        async UniTask LoadAssets(CancellationToken token) {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);

            CheckInternetCTS?.Cancel();
            CheckInternetCTS = new CancellationTokenSource();
            WaitForCheckingBundle(CheckInternetCTS.Token).Forget();

            long totalSize = 0;
#if !(UNITY_WEBGL && !UNITY_EDITOR)
            var getDownloadSize = Addressables.GetDownloadSizeAsync(Keys);
            await getDownloadSize.Task;
            totalSize = getDownloadSize.Result;
            WriteLog.LogColorFormat("LoadAsset-TotalSize={0}", WriteLog.LogType.Addressable, MyMath.BytesToMB(totalSize).ToString("N2"));
            Addressables.Release(getDownloadSize);
#endif

            CheckInternetCTS?.Cancel();

            var curDownloading = Addressables.DownloadDependenciesAsync(Keys, Addressables.MergeMode.Union);

            if (curDownloading.IsDone) {
                ShowBar(false);
                Addressables.Release(curDownloading);
                OnFinishedDownload();
                return;
            }

            if (await ShouldShowProgress(curDownloading, token)) {
                ShowBar(true);
                await Loading(curDownloading, totalSize, token);
            } else {
                ShowBar(false);
                Addressables.Release(curDownloading);
                OnFinishedDownload();
            }
        }

        async UniTask<bool> ShouldShowProgress(AsyncOperationHandle handle, CancellationToken token) {
            var deadline = Time.realtimeSinceStartup + CHECK_DOWNLOAD_MILISEC / 1000f;
            while (Time.realtimeSinceStartup < deadline) {
                if (handle.IsDone) return false;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            return !handle.IsDone;
        }

        async UniTask Loading(AsyncOperationHandle curDownloading, long _totalSize, CancellationToken token) {
            bool downloading = true;
            while (downloading) {
                var st = curDownloading.GetDownloadStatus();
                float curDownloadPercent = st.Percent;
                long downloaded = (long)st.DownloadedBytes;
                long total = _totalSize > 0 ? _totalSize : (long)(st.TotalBytes > 0 ? st.TotalBytes : st.DownloadedBytes);

                ProgressImg.fillAmount = curDownloadPercent;
                if (total > 0) {
                    Txt_Progress.text = string.Format(JsonString.GetUIString("AssetUpdating"), MyMath.BytesToMB(downloaded).ToString("N2"), MyMath.BytesToMB(total).ToString("N2"));
                } else {
                    Txt_Progress.text = JsonString.GetUIString("Loading");
                }

                if (curDownloading.IsDone) {
                    Addressables.Release(curDownloading);
                    downloading = false;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: token);
            }
            OnFinishedDownload();
        }

        void OnFinishedDownload() {
            WriteLog.LogColorFormat("LoadAsset-Finished", WriteLog.LogType.Addressable);
            FinishedAction?.Invoke();
        }
        public void ShowDownloadUI(bool _show) {
            GO_LoadingUI.gameObject.SetActive(_show);
        }

        public void ShowBar(bool _show) {
            GO_Bar.gameObject.SetActive(_show);
            if (_show) {
                ProgressImg.fillAmount = 0;
                Txt_Progress.text = JsonString.GetUIString("Loading");
            }
        }

        /// <summary>
        /// 傳入Addressable的key確認此Addressable是否存在
        /// </summary>
        public void CheckIfAddressableExist(List<string> _keys, Action<bool> _cb) {
            CheckIfAddressableExistAsync(_keys, _cb).Forget();
        }
        async UniTask CheckIfAddressableExistAsync(List<string> _keys, Action<bool> _cb) {
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
            Addressables.Release(locationHandle);
        }

        /// <summary>
        /// 傳入Addressable的key取得下載大小(mb)
        /// </summary>
        public void GetDownloadAddressableSize(List<string> _keys, Action<float> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    GetDownloadAddressableSizeAsync(_keys, _cb).Forget();
                } else {
                    _cb?.Invoke(0);
                }
            });
        }
        async UniTask GetDownloadAddressableSizeAsync(List<string> _keys, Action<float> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(0);
                return;
            }
            var getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            await getDownloadSize.Task;
            long totalSize = getDownloadSize.Result;
            float mb = MyMath.BytesToMB(totalSize);
            _cb?.Invoke(mb);
            Addressables.Release(getDownloadSize);
        }

        /// <summary>
        /// 傳入Addressable的key目標下載大小(mb)
        /// </summary>
        public void DownloadAddressable(List<string> _keys, Action<bool> _cb) {
            CheckIfAddressableExist(_keys, result => {
                if (result == true) {
                    DownloadAddressableCheckAsync(_keys, _cb).Forget();
                } else {
                    _cb?.Invoke(false);
                }
            });
        }
        async UniTask DownloadAddressableCheckAsync(List<string> _keys, Action<bool> _cb) {
            if (_keys == null || _keys.Count == 0) {
                _cb?.Invoke(false);
                return;
            }
            var getDownloadSize = Addressables.GetDownloadSizeAsync(_keys);
            await getDownloadSize.Task;

            if (getDownloadSize.Status != AsyncOperationStatus.Succeeded) {
                Addressables.Release(getDownloadSize);
                _cb?.Invoke(false);
                return;
            }

            long totalSize = getDownloadSize.Result;
            WriteLog.Log("Download TotalSize=" + totalSize);
            Addressables.Release(getDownloadSize);
            if (totalSize > 0) {
                ShowBar(true);
                DownloadingAddressableAsync(_keys, totalSize, _cb).Forget();
            } else {
                ShowBar(false);
                _cb?.Invoke(true);
            }
        }
        async UniTask DownloadingAddressableAsync(List<string> _keys, long _totalSize, Action<bool> _cb) {
            var curDownloading = Addressables.DownloadDependenciesAsync(_keys, Addressables.MergeMode.Union);
            bool downloading = true;
            while (downloading) {
                float curDownloadPercent = curDownloading.GetDownloadStatus().Percent;
                long curDownloadSize = (long)(curDownloadPercent * _totalSize);

                ProgressImg.fillAmount = curDownloadPercent;
                Txt_Progress.text = string.Format(JsonString.GetUIString("AssetUpdating"), MyMath.BytesToMB(curDownloadSize).ToString("N2"), MyMath.BytesToMB(_totalSize).ToString("N2"));
                if (curDownloading.IsDone) {
                    Addressables.Release(curDownloading);
                    downloading = false;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
            }
            ShowBar(false);
            _cb?.Invoke(true);
        }
    }
}
