using System;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

namespace Scoz.Func {
    public class Downloader : MonoBehaviour {
        public static Downloader Instance;
        private static DateTime LastCallDownloadTime;
        private static readonly double MinInterval = 0.2f;

        private void Awake() {
            Instance = this;
        }

        /// <summary>
        /// 透過 URL 取得 Sprite 資源
        /// </summary>
        public static void GetSpriteFromUrl(string _url, Action<Sprite> _cb) {
            if (Instance == null) {
                WriteLog.LogError("Downloader 尚未初始化");
                return;
            }

            Instance.DownloadAsync(_url, _cb).Forget();
        }

        /// <summary>
        /// 執行非同步下載並轉換為 Sprite
        /// </summary>
        public async UniTaskVoid DownloadAsync(string _url, Action<Sprite> _cb) {
            float waitTime = 0;
            var now = DateTime.Now;
            var diffSeconds = (now - LastCallDownloadTime).TotalSeconds;

            if (diffSeconds < MinInterval) waitTime = (float)(MinInterval - diffSeconds);

            LastCallDownloadTime = now.AddSeconds(waitTime);

            if (waitTime > 0) await UniTask.Delay(TimeSpan.FromSeconds(waitTime));

            using (var request = UnityWebRequestTexture.GetTexture(_url)) {
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success) {
                    WriteLog.LogError($"下載錯誤: {request.error} URL: {_url}");
                    _cb?.Invoke(null);
                    return;
                }

                var texture = DownloadHandlerTexture.GetContent(request);
                if (texture != null) {
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    _cb?.Invoke(sprite);
                } else {
                    WriteLog.LogError("無法從下載內容中獲取 Texture");
                    _cb?.Invoke(null);
                }
            }
        }
    }
}