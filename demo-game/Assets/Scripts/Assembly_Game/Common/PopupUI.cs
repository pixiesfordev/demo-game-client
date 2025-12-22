using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using tower.main;
using DG.Tweening;

namespace Scoz.Func {
    public partial class PopupUI : MonoBehaviour {

        public static PopupUI Instance { get; private set; }
        public Canvas MyCanvas { get; private set; }
        static Action onPopupAc;


        //[HeaderAttribute("==============基本設定==============")]

        public void Init() {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MyCanvas = GetComponent<Canvas>();
            MyCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            MyCanvas.sortingLayerName = "UI";
            InitLoading();
            InitClickCancel();
            InitConfirmCancel();
        }
        void Start() {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }
        void OnLevelFinishedLoading(Scene _scene, LoadSceneMode _mode) {
            MyCanvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            MyCanvas.sortingLayerName = "UI";
        }

        public static void RegisterOnPopupAction(Action _ac) {
            onPopupAc += _ac;
        }
        public static void UnRegisterOnPopupAction(Action _ac) {
            onPopupAc -= _ac;
        }

        #region 彈窗通用動畫設定

        [SerializeField] float popupBoxAni_Duration = 0.15f;   // 動畫時間
        [SerializeField] float popupBoxAni_StartScale = 0.9f;  // 起始大小
        [SerializeField] float popupBoxAni_OverScale = 1.1f;   // 放大大小
        CancellationTokenSource confirmCancelAnimCts;

        /// <summary>
        /// 通用彈窗動畫
        /// </summary>
        void playPopupBoxAnim(RectTransform _rt) {

            // 停掉之前動畫
            _rt.DOKill();

            _rt.localScale = Vector3.one * popupBoxAni_StartScale;
            _rt.DOScale(popupBoxAni_OverScale, popupBoxAni_Duration * 0.5f)
             .SetUpdate(true)
             .OnComplete(() => {
                 _rt.DOScale(1f, popupBoxAni_Duration * 0.5f)
                  .SetUpdate(true);
             });
        }

        #endregion

        #region 讀取彈窗

        [HeaderAttribute("==============讀取彈窗==============")]
        [SerializeField] GameObject LoadingGo = null;
        [SerializeField] TextMeshProUGUI LoadingText = null;
        static CancellationTokenSource loadingCts;
        static string loadingTimeOutStr = "";

        void InitLoading() {
            LoadingGo.SetActive(false);
        }
        /// <summary>
        /// 顯示Loading介面, _maxLoadingTime不傳入預設是看GameSetting表
        /// </summary>
        public static void ShowLoading(string _text, float _maxLoadingTime = float.MaxValue, string _loadingTimeOutStr = "") {
            if (!Instance) return;
            // 顯示 UI
            Instance.LoadingGo.SetActive(true);
            Instance.LoadingText.text = _text;
            loadingTimeOutStr = _loadingTimeOutStr;

            // 取消上一個排程
            loadingCts?.Cancel();
            loadingCts = null;

            // 排程超時隱藏
            if (_maxLoadingTime > 0 && _maxLoadingTime < float.MaxValue) {
                loadingCts = new CancellationTokenSource();
                var token = loadingCts.Token;

                UniTask
                  .Delay(TimeSpan.FromSeconds(_maxLoadingTime), cancellationToken: token)
                  .ContinueWith(() => {
                      HideLoading();
                      if (!string.IsNullOrEmpty(loadingTimeOutStr)) {
                          ShowClickCancel(JsonString.GetUIString("Popup_SysInfo"), loadingTimeOutStr, null);
                      }
                  }).SuppressCancellationThrow().Forget();
            }
        }
        public static void HideLoading() {
            if (!Instance) return;
            // 取消未完成的排程
            loadingCts?.Cancel();
            loadingCts = null;
            Instance.LoadingGo.SetActive(false);
        }

        #endregion

        #region 單按鈕彈窗

        [HeaderAttribute("==============點擊關閉彈窗==============")]
        [SerializeField] GameObject ClickCancelGo = null;
        [SerializeField] RectTransform Trans_ClickCancel;
        [SerializeField] TextMeshProUGUI ClickCancel_Title = null;
        [SerializeField] TextMeshProUGUI ClickCancel_Content = null;
        [SerializeField] TextMeshProUGUI ClickCancel_CancelBtnText = null;
        Action ClickCancelAction = null;

        void InitClickCancel() {
            ClickCancelGo.SetActive(false);
            ClickCancelGo.transform.localScale = Vector3.one;
        }

        public static void ShowClickCancel(string _title, string _content, Action _clickCancelAction, bool _triggerOnPopupAc = true) {
            if (!Instance) return;
            if (_triggerOnPopupAc) onPopupAc?.Invoke();
            Instance.ClickCancelGo.SetActive(true);
            Instance.ClickCancel_Title.text = _title;
            Instance.ClickCancel_Content.text = _content;
            Instance.ClickCancelAction = _clickCancelAction;
            Instance.ClickCancel_CancelBtnText.text = JsonString.GetUIString("Popup_Confirm");
            Instance.playPopupBoxAnim(Instance.Trans_ClickCancel); // 播放彈出動畫
        }
        public void OnClickCancelClick() {
            if (!Instance) return;
            AudioPlayer.PlayAudioByPath(MyAudioType.Sound, "btn_soft");
            Instance.ClickCancelGo.SetActive(false);
            ClickCancelAction?.Invoke();
        }

        #endregion

        #region 雙按鈕彈窗(確認&取消)

        [HeaderAttribute("==============確認/取消彈窗==============")]
        [SerializeField] GameObject ConfirmCancelGo = null;
        [SerializeField] RectTransform Trans_ConfirmCancel;
        [SerializeField] TextMeshProUGUI ConfirmCancel_Title = null;
        [SerializeField] TextMeshProUGUI ConfirmCancel_Content = null;
        [SerializeField] TextMeshProUGUI ConfirmCancel_ConfirmBtnText = null;
        [SerializeField] TextMeshProUGUI ConfirmCancel_CancelBtnText = null;
        Action ConfirmCancelAction_Click = null;
        Action ConfirmCancelAction_Cancel = null;

        void InitConfirmCancel() {
            ConfirmCancelGo.SetActive(false);
            ConfirmCancelGo.transform.localScale = Vector3.one;
        }
        public static void ShowConfirmCancel(string _title, string _content, Action _confirmAction, Action _cancelAction, bool _triggerOnPopupAc = true) {
            if (!Instance) return;
            if (_triggerOnPopupAc) onPopupAc?.Invoke();
            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancel_Title.text = _title;
            Instance.ConfirmCancel_Content.text = _content;
            Instance.ConfirmCancelAction_Click = _confirmAction;
            Instance.ConfirmCancelAction_Cancel = _cancelAction;
            Instance.ConfirmCancel_ConfirmBtnText.text = JsonString.GetUIString("Popup_Confirm");
            Instance.ConfirmCancel_CancelBtnText.text = JsonString.GetUIString("Popup_Cancel");
            Instance.playPopupBoxAnim(Instance.Trans_ConfirmCancel); // 播放彈出動畫
        }

        public void OnConfirmCancel_ConfirmClick() {
            if (!Instance) return;
            AudioPlayer.PlayAudioByPath(MyAudioType.Sound, "btn_soft");
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Click?.Invoke();
        }

        public void OnConfirmCancel_CancelClick() {
            if (!Instance) return;
            AudioPlayer.PlayAudioByPath(MyAudioType.Sound, "btn_soft");
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Cancel?.Invoke();
        }

        #endregion

    }
}
