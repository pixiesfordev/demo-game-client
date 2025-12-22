using Scoz.Func;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks; // ★ 引用 UniTask

namespace tower.main {
    public class BlurImgMaker : MonoBehaviour {

        [SerializeField] Material BlurHMat;
        [SerializeField] Material BlurVMat;

        static BlurImgMaker instance;
        static RenderTexture rt;
        static Texture2D capturedTexture;

        void Start() {
            instance = this;
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            rt.Create();
            capturedTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        }


        /// <summary>
        /// 取得攝影機畫面模糊截圖(同步版本)
        /// </summary>
        public static Texture CaptureCamViewToBlurTexture(Camera _cam, float _resolutionDivide = 1) {
            WriteLog.Log("CaptureCamViewToBlurTexture");
            if (_resolutionDivide > 1 || _resolutionDivide <= 0) {
                WriteLog.LogError($"CaptureCamViewToBlurTexture傳入錯誤的_resolutionDivide: {_resolutionDivide}");
                return null;
            }
            var mainCam = _cam;

            // 把 Camera 的畫面渲染到 rt
            mainCam.targetTexture = rt;
            mainCam.Render();
            mainCam.targetTexture = null;

            // 將 RenderTexture 內容讀取到 capturedTexture
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            capturedTexture.Apply();
            RenderTexture.active = prevActive;

            if (_resolutionDivide == 1f) { // 原尺寸就不用縮放畫面再模糊
                RenderTexture tempRT1 = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);
                RenderTexture tempRT2 = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);

                // 水平模糊
                Graphics.Blit(capturedTexture, tempRT1, instance.BlurHMat);
                // 垂直模糊
                Graphics.Blit(tempRT1, tempRT2, instance.BlurVMat);

                // 從 tempRT2 讀取到 capturedTexture
                prevActive = RenderTexture.active;
                RenderTexture.active = tempRT2;
                capturedTexture.Reinitialize(rt.width, rt.height);
                capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                capturedTexture.Apply();
                RenderTexture.active = prevActive;

                RenderTexture.ReleaseTemporary(tempRT1);
                RenderTexture.ReleaseTemporary(tempRT2);

                return capturedTexture;
            } else {
                // 若 _resolutionDivide < 1f，則先縮小再模糊，最後再放大
                int blurWidth = (int)(rt.width / _resolutionDivide);
                int blurHeight = (int)(rt.height / _resolutionDivide);
                // 先縮小
                RenderTexture tempHalfRT = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);
                Graphics.Blit(capturedTexture, tempHalfRT);

                RenderTexture tempRT1 = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);
                RenderTexture tempRT2 = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);

                // 水平模糊
                Graphics.Blit(tempHalfRT, tempRT1, instance.BlurHMat);
                // 垂直模糊
                Graphics.Blit(tempRT1, tempRT2, instance.BlurVMat);

                // 再把模糊後的結果放大回原尺寸
                RenderTexture finalRT = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);
                Graphics.Blit(tempRT2, finalRT);

                // 最終讀取到 capturedTexture
                prevActive = RenderTexture.active;
                RenderTexture.active = finalRT;
                capturedTexture.Reinitialize(rt.width, rt.height);
                capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                capturedTexture.Apply();
                RenderTexture.active = prevActive;

                // 釋放暫存的 RT
                RenderTexture.ReleaseTemporary(tempHalfRT);
                RenderTexture.ReleaseTemporary(tempRT1);
                RenderTexture.ReleaseTemporary(tempRT2);
                RenderTexture.ReleaseTemporary(finalRT);

                return capturedTexture;
            }
        }


        /// <summary>
        /// 取得攝影機畫面模糊截圖(非同步版本)
        /// </summary>
        public static async UniTask<Texture2D> CaptureCamViewToBlurTextureAsync(Camera _cam, float _resolutionDivide = 1) {
            WriteLog.Log("CaptureCamViewToBlurTexture - Async");
            if (_resolutionDivide > 1 || _resolutionDivide <= 0) {
                WriteLog.LogError($"CaptureCamViewToBlurTexture傳入錯誤的_resolutionDivide: {_resolutionDivide}");
                return null;
            }
            var mainCam = _cam;

            // 把 Camera 的畫面渲染到 rt
            mainCam.targetTexture = rt;
            mainCam.Render();
            mainCam.targetTexture = null;

            // 下一幀再繼續
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            // 將 RenderTexture 內容讀取到 capturedTexture
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            capturedTexture.Apply();
            RenderTexture.active = prevActive;

            // 下一幀再繼續
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

            if (_resolutionDivide == 1f) {
                RenderTexture tempRT1 = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);
                RenderTexture tempRT2 = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);

                // 水平模糊
                Graphics.Blit(capturedTexture, tempRT1, instance.BlurHMat);
                // 下一幀再繼續
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // 垂直模糊
                Graphics.Blit(tempRT1, tempRT2, instance.BlurVMat);
                // 下一幀再繼續
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // 從 tempRT2 讀取到 capturedTexture
                prevActive = RenderTexture.active;
                RenderTexture.active = tempRT2;
                capturedTexture.Reinitialize(rt.width, rt.height);
                capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                capturedTexture.Apply();
                RenderTexture.active = prevActive;

                RenderTexture.ReleaseTemporary(tempRT1);
                RenderTexture.ReleaseTemporary(tempRT2);

                return capturedTexture;
            } else {
                // 若 _resolutionDivide < 1f，則先縮小再模糊，最後再放大
                int blurWidth = (int)(rt.width / _resolutionDivide);
                int blurHeight = (int)(rt.height / _resolutionDivide);

                // 先縮小
                RenderTexture tempHalfRT = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);
                Graphics.Blit(capturedTexture, tempHalfRT);

                // 下一幀再繼續
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                RenderTexture tempRT1 = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);
                RenderTexture tempRT2 = RenderTexture.GetTemporary(blurWidth, blurHeight, 0, rt.format);

                // 水平模糊
                Graphics.Blit(tempHalfRT, tempRT1, instance.BlurHMat);
                // 下一幀再繼續
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // 垂直模糊
                Graphics.Blit(tempRT1, tempRT2, instance.BlurVMat);
                // ★ 再度讓出
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                // 再把模糊後的結果放大回原尺寸
                RenderTexture finalRT = RenderTexture.GetTemporary(rt.width, rt.height, 0, rt.format);
                Graphics.Blit(tempRT2, finalRT);

                prevActive = RenderTexture.active;
                RenderTexture.active = finalRT;
                capturedTexture.Reinitialize(rt.width, rt.height);
                capturedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                capturedTexture.Apply();
                RenderTexture.active = prevActive;

                // 釋放暫存的 RT
                RenderTexture.ReleaseTemporary(tempHalfRT);
                RenderTexture.ReleaseTemporary(tempRT1);
                RenderTexture.ReleaseTemporary(tempRT2);
                RenderTexture.ReleaseTemporary(finalRT);

                return capturedTexture;
            }
        }
    }
}
