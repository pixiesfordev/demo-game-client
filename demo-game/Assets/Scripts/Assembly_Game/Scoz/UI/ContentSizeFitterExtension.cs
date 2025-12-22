using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;

namespace Scoz.Func {

    public static class ContentSizeFitterExtension {
        /// <summary>
        /// 刷新ContentSizeFitter，要跑這個圖片隨文字延展才會更新
        /// </summary>
        public static void Update(this ContentSizeFitter _contentSizeFitter, Action _ac = null) {
            if (_contentSizeFitter == null) return;
            _contentSizeFitter.enabled = false;
            UniTask.Delay(TimeSpan.FromSeconds(0.05f)).ContinueWith(() => {
                if (_contentSizeFitter != null) _contentSizeFitter.enabled = true;
                _ac?.Invoke();
            }).Forget();
        }
        /// <summary>
        /// 刷新ContentSizeFitter，要跑這個圖片隨文字延展才會更新
        /// </summary>
        public static void Update(this ContentSizeFitter[] _contentSizeFitters) {
            if (_contentSizeFitters == null) return;
            for (int i = 0; i < _contentSizeFitters.Length; i++) {
                if (_contentSizeFitters[i] != null) {
                    _contentSizeFitters[i].enabled = false;
                    int index = i;
                    UniTask.Delay(TimeSpan.FromSeconds(0.01f)).ContinueWith(() => {
                        _contentSizeFitters[index].enabled = true;
                    }).Forget();
                }
            }
        }
    }
}
