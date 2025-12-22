using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;

namespace Scoz.Func {
    public class LoadingProgress {
        private Dictionary<string, bool> progress;
        private Action finishCB;
        private float cbTime;
        private int pendingCount;

        /// <summary>
        /// 進度完成時會設為true，且不能再新增Loading項目
        /// </summary>
        public bool IsFinished { get; private set; } = false;

        /// <summary>
        /// 傳入完成進度時的callBack與完成進度時callBack前的等待時間
        /// </summary>
        public LoadingProgress(Action _cb, float _cbTime = 0) {
            progress = new Dictionary<string, bool>();
            finishCB = _cb;
            cbTime = _cbTime;
            IsFinished = false;
            pendingCount = 0;
        }

        /// <summary>
        /// 重置所有進度與狀態
        /// </summary>
        public void ResetProgress() {
            IsFinished = false;
            progress.Clear();
            pendingCount = 0;
        }

        /// <summary>
        /// 新增要讀取的Key
        /// </summary>
        public void AddLoadingProgress(params string[] _loadingKeys) {
            if (IsFinished) {
                WriteLog.LogError("LoadingProgress 已經完成 無法再新增Loading項目");
                return;
            }

            if (_loadingKeys == null) return;
            for (var i = 0; i < _loadingKeys.Length; i++) {
                var key = _loadingKeys[i];
                if (string.IsNullOrEmpty(key)) {
                    WriteLog.LogErrorFormat("要加入的Key為空: {0}", key);
                    continue;
                }

                if (progress.ContainsKey(key)) {
                    WriteLog.LogError("嘗試新增重複的LoadingKey:" + key);
                    continue;
                }

                progress.Add(key, false);
                pendingCount++;
            }
        }

        /// <summary>
        /// 完成讀取進度
        /// </summary>
        public void FinishProgress(string _loadingKey) {
            if (IsFinished) return;
            if (!progress.TryGetValue(_loadingKey, out var isDone) || isDone) return;

            progress[_loadingKey] = true;
            pendingCount--;

            if (CheckIfProgressIsFinished()) {
                IsFinished = true;
                HandleFinishCallback().Forget();
            }
        }

        /// <summary>
        /// 檢查是否所有進度皆已完成
        /// </summary>
        private bool CheckIfProgressIsFinished() {
            return pendingCount <= 0;
        }

        /// <summary>
        /// 處理完成時的回調與延遲
        /// </summary>
        private async UniTaskVoid HandleFinishCallback() {
            if (cbTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(cbTime));
            finishCB?.Invoke();
        }

        /// <summary>
        /// 獲取尚未完成的Key清單
        /// </summary>
        public List<string> GetNotFinishedKeys() {
            var keys = new List<string>(pendingCount);
            foreach (var pair in progress)
                if (!pair.Value)
                    keys.Add(pair.Key);

            return keys;
        }
    }
}