using tower.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;


namespace Scoz.Func {
    public class PopupUI_Local : MonoBehaviour {
        public static bool IsInit { get; private set; }
        private static PopupUI_Local Instance;

        //[HeaderAttribute("==============基本設定==============")]

        public static PopupUI_Local CreateNewInstance() {
            if (Instance != null) {
                //WriteLog.Log("GameDictionary之前已經被建立了");
            } else {
                var prefab = Resources.Load<GameObject>("Prefabs/Common/PopupUI_Local");
                var go = Instantiate(prefab);

                go.name = "PopupUI_Local";
                Instance = go.GetComponent<PopupUI_Local>();
                Instance.Init();
            }

            return Instance;
        }

        private void Start() {
        }

        private void OnDestroy() {
        }

        public void Init() {
            Instance = this;
            InitLoading();
            InitClickCamcel();
            InitConfirmCancel();

            WriteLog_UnityAssembly.Log("初始化PopupUI_Local");
        }


        [HeaderAttribute("==============讀取彈窗==============")]
        [SerializeField]
        private GameObject LoadingGo = null;

        [SerializeField] private Text LoadingText = null;
        private static float LoadingMaxTime = 30f;
        private static CancellationTokenSource loadingCts;

        private void InitLoading() {
            LoadingGo.SetActive(false);
        }

        public static void ShowLoading(string _text, float _maxLoadingTime = 0) {
            if (!Instance) return;

            if (_maxLoadingTime <= 0f) _maxLoadingTime = LoadingMaxTime;

            Instance.LoadingGo.SetActive(true);
            Instance.LoadingText.text = _text;

            // 取消上一個排程
            loadingCts?.Cancel();
            loadingCts = null;

            // 排程超時隱藏
            if (_maxLoadingTime > 0f) {
                loadingCts = new CancellationTokenSource();
                var token = loadingCts.Token;

                UniTask.Delay(TimeSpan.FromSeconds(_maxLoadingTime), cancellationToken: token).ContinueWith(HideLoading)
                    .SuppressCancellationThrow().Forget();
            }
        }

        public static void HideLoading() {
            if (!Instance) return;
            // 取消未完成的排程
            loadingCts?.Cancel();
            loadingCts = null;
            Instance.LoadingGo.SetActive(false);
        }

        [HeaderAttribute("==============點擊關閉彈窗==============")]
        [SerializeField]
        private GameObject ClickCancelGo = null;

        [SerializeField] private Text ClickCancelTitle = null;
        [SerializeField] private Text ClickCancelContent = null;
        [SerializeField] private Text ConfrimText = null;
        [SerializeField] private Action ClickCancelAction = null;
        private Action<object> ClickCancelActionWithParam = null;
        private object ClickCancelParam;

        private void InitClickCamcel() {
            ClickCancelGo.SetActive(false);
        }

        public static void ShowClickCancel(string _title, string _content, string _confirmText,
            Action _clickCancelAction) {
            if (!Instance) return;
            Instance.ClickCancelGo.SetActive(true);
            Instance.ClickCancelTitle.text = _title;
            Instance.ClickCancelContent.text = _content;
            if (_confirmText != "")
                Instance.ConfrimText.text = _confirmText;
            else
                Instance.ConfrimText.text = StringJsonData_UnityAssembly.GetUIString("Confirm");
            Instance.ClickCancelAction = _clickCancelAction;
        }

        public static void ShowClickCancel(string _title, string _content, string _confirmText,
            Action<object> _clickCancelAction, object _param) {
            if (!Instance) return;
            Instance.ClickCancelGo.SetActive(true);
            Instance.ClickCancelTitle.text = _title;
            Instance.ClickCancelContent.text = _content;
            if (_confirmText != "")
                Instance.ConfrimText.text = _confirmText;
            else
                Instance.ConfrimText.text = StringJsonData_UnityAssembly.GetUIString("Confirm");
            Instance.ClickCancelActionWithParam = _clickCancelAction;
            Instance.ClickCancelParam = _param;
        }

        public void OnClickCancelClick() {
            if (!Instance) return;
            Instance.ClickCancelGo.SetActive(false);
            ClickCancelAction?.Invoke();
            ClickCancelActionWithParam?.Invoke(ClickCancelParam);
        }


        [HeaderAttribute("==============確認/取消彈窗==============")]
        [SerializeField]
        private GameObject ConfirmCancelGo = null;

        [SerializeField] private Text ConfirmCancelTitle = null;
        [SerializeField] private Text ConfirmCancelContent = null;
        [SerializeField] private Text ConfirmCancel_ConfirmBtnText = null;
        [SerializeField] private Text ConfirmCancel_CancelBtnText = null;
        [SerializeField] private Button ConfirmCancel_ConfirmBtn;
        private Action ConfirmCancelAction_Click = null;
        private Action ConfirmCancelAction_Cancel = null;
        private Action<object> ConfirmCancelAction_Click_WithParam = null;
        private Action<object> ConfirmCancelAction_Cancel_WithParam = null;
        private object ConfirmCancel_ConfirmParam;
        private object ConfirmCancel_CancelParam;
        private int ConfirmCanClickCoundownSecs;

        private void InitConfirmCancel() {
            ConfirmCancelGo.SetActive(false);
        }

        /// <summary>
        /// 顯示確認取消視窗
        /// </summary>
        public static void ShowConfirmCancel(string _title, string _content, Action _confirmAction,
            Action _cancelAction) {
            if (!Instance) return;
            Instance.ConfirmCancelGo.SetActive(true);
            Instance.ConfirmCancelTitle.text = _title;
            Instance.ConfirmCancelContent.text = _content;
            Instance.ConfirmCancelAction_Click = _confirmAction;
            Instance.ConfirmCancelAction_Cancel = _cancelAction;
            Instance.ConfirmCancelAction_Click_WithParam = null;
            Instance.ConfirmCancelAction_Cancel_WithParam = null;
            Instance.ConfirmCancel_ConfirmBtnText.text = StringJsonData_UnityAssembly.GetUIString("Confirm");
            Instance.ConfirmCancel_CancelBtnText.text = StringJsonData_UnityAssembly.GetUIString("Cancel");
            Instance.ConfirmCancel_ConfirmBtn.interactable = true;
        }


        public void OnConfirmCancel_ConfirmClick() {
            if (!Instance) return;
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Click?.Invoke();
            ConfirmCancelAction_Click_WithParam?.Invoke(ConfirmCancel_ConfirmParam);
        }

        public void OnConfirmCancel_CancelClick() {
            if (!Instance) return;
            Instance.ConfirmCancelGo.SetActive(false);
            ConfirmCancelAction_Cancel?.Invoke();
            ConfirmCancelAction_Cancel_WithParam?.Invoke(ConfirmCancel_CancelParam);
        }
    }
}