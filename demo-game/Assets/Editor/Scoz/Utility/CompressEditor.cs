using System.IO;
using UnityEditor;
using UnityEngine;

namespace Scoz.Func {
    public class CompressEditor : MonoBehaviour {
        private const int PRIORITY_CONVERT_TO_SPRITE = 1011;
        private const int PRIORITY_RESIZE = 1016;
        private const int PRIORITY_CONVERT_TO_ASTC6X6 = 1022;
        private const int PRIORITY_CONVERT_TO_CRUNCHED_DXT5 = 1033;


        #region Convert to Sprite
        [MenuItem("Assets/Scoz/SpriteFormat/圖片格式設定為Sprite", priority = PRIORITY_CONVERT_TO_SPRITE)]
        private static void SetTextureTypeToSprite() {
            Object[] selectTextureS = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
            Object selectObj = Selection.activeObject;
            Object[] selectObjS = Selection.objects;

            Selection.activeObject = null;
            Selection.objects = new Object[0];
            foreach (var obj in selectTextureS) {
                if (obj == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (EditorUtility.DisplayCancelableProgressBar("Set Texture to Sprite... ", path, 0f)) {
                    Selection.activeObject = selectObj;
                    Selection.objects = selectObjS;
                    EditorUtility.ClearProgressBar();
                    return;
                }

                bool bChange = false;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                importer.spriteImportMode = SpriteImportMode.Single;
                bChange |= SetTextureToSprite(importer);

                if (bChange) {
                    importer.SaveAndReimport();
                }
            }
            Selection.activeObject = selectObj;
            Selection.objects = selectObjS;
            EditorUtility.ClearProgressBar();

            bool SetTextureToSprite(TextureImporter _importer) {
                if (_importer == null)
                    return false;
                if (_importer.textureType != TextureImporterType.Sprite) {
                    _importer.textureType = TextureImporterType.Sprite;
                    return true;
                } else
                    return false;
                /*
                var setting = _importer.GetPlatformTextureSettings("Standalone");
                if (setting.format != TextureImporterFormat.DXT5Crunched ||
                    setting.compressionQuality != 100)
                {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.DXT5Crunched;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = MaxTextureSize;
                    _importer.filterMode = FilterMode.Bilinear;
                    _importer.npotScale = TextureImporterNPOTScale.None;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    return true;
                }
                return false;
                */
            }
        }

        #endregion

        #region Set size to multiple of 4
        [MenuItem("Assets/Scoz/SpriteFormat/將png圖片大小改成4的倍數(無法復原)", priority = PRIORITY_RESIZE)]
        public static void AutoPadToMultipleOf4AndCompress() {
            Object[] selectedTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            int modifyCount = 0;
            for (int i = 0; i < selectedTextures.Length; i++) {
                var tex = selectedTextures[i] as Texture2D;
                if (tex == null) continue;

                string assetPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(assetPath)) continue;

                // 讀入原圖檔 (texture importer 要開Read/Write Enabled 才能GetPixels)
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool isReadable = importer.isReadable;
                if (!isReadable) {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }

                // 重新載入 (確保讀取 isReadable 的資源)
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

                // 檢查寬高是否為4的倍數
                int width = tex.width;
                int height = tex.height;
                int newWidth = (width + 3) / 4 * 4;  // 無條件進位到下個4的倍數
                int newHeight = (height + 3) / 4 * 4;

                // 如果已經是4倍數, 就不做處理
                if (width == newWidth && height == newHeight) {
                    //Debug.Log($"{tex.name} 已經是4的倍數: {width}x{height}");
                } else {
                    // 新建一個空的Texture2D
                    Texture2D paddedTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
                    paddedTex.SetPixels32(new Color32[newWidth * newHeight]); // 先把全部清成透明

                    // 把原圖的像素貼上
                    Color[] oldPixels = tex.GetPixels(0, 0, width, height);
                    paddedTex.SetPixels(0, 0, width, height, oldPixels);
                    paddedTex.Apply();

                    // 重新編碼 PNG (或你想要的格式)
                    byte[] pngData = paddedTex.EncodeToPNG();

                    // 寫回原檔(或你要的其它目標路徑)
                    File.WriteAllBytes(assetPath, pngData);

                    // 清除暫存
                    Object.DestroyImmediate(paddedTex);

                    modifyCount++;
                    Debug.Log($"已將圖片 {tex.name} 補到 4 的倍數: {width}x{height} -> {newWidth}x{newHeight}");
                }

                // 恢復原先可讀狀態
                if (!isReadable) {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log($"有{modifyCount}張圖片自動補齊到 4 的倍數");
        }

        #endregion

        #region Convert to RGBA ASTC 6X6
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成ASTC 6X6 (4096)", priority = PRIORITY_CONVERT_TO_ASTC6X6, secondaryPriority = 0f)]
        private static void CompressTextureASTC_4096() {
            CompressTextureASTC(4096);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成ASTC 6X6 (2048)", priority = PRIORITY_CONVERT_TO_ASTC6X6, secondaryPriority = 1f)]
        private static void CompressTextureASTC_2048() {
            CompressTextureASTC(2048);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成ASTC 6X6 (1024)", priority = PRIORITY_CONVERT_TO_ASTC6X6, secondaryPriority = 2f)]
        private static void CompressTextureASTC_1024() {
            CompressTextureASTC(1024);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成ASTC 6X6 (512)", priority = PRIORITY_CONVERT_TO_ASTC6X6, secondaryPriority = 3f)]
        private static void CompressTextureASTC_512() {
            CompressTextureASTC(512);
        }
        private static void CompressTextureASTC(int _maxTextureSize) {
            Object[] selectTextureS = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
            Object selectObj = Selection.activeObject;
            Object[] selectObjS = Selection.objects;

            Selection.activeObject = null;
            Selection.objects = new Object[0];
            foreach (var obj in selectTextureS) {
                if (obj == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (EditorUtility.DisplayCancelableProgressBar("Compressing (ASTC 6X6)... ", path, 0f)) {
                    Selection.activeObject = selectObj;
                    Selection.objects = selectObjS;
                    EditorUtility.ClearProgressBar();
                    return;
                }

                bool bChange = false;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                bChange |= SetASTC66(importer, _maxTextureSize);

                if (bChange) {
                    importer.SaveAndReimport();
                }
            }

            Selection.activeObject = selectObj;
            Selection.objects = selectObjS;
            EditorUtility.ClearProgressBar();

            bool SetASTC66(TextureImporter _importer, int _maxTextureSize) {
                if (_importer == null)
                    return false;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                bool result = false;
                var setting = _importer.GetPlatformTextureSettings("Android");
                if (setting.format != TextureImporterFormat.ASTC_6x6 || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.ASTC_6x6;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;

                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                setting = _importer.GetPlatformTextureSettings("iOS");
                if (setting.format != TextureImporterFormat.ASTC_6x6 || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.ASTC_6x6;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                setting = _importer.GetPlatformTextureSettings("WebGL");
                if (setting.format != TextureImporterFormat.ASTC_6x6 || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.ASTC_6x6;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }

                return result;
            }
        }
        #endregion

        #region Convert to RGBA Crunched DXT5|BC3
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (4096)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 0f)]
        private static void CompressTextureCrunchedDXT5_4096() {
            CompressTextureCrunchedDXT5(4096);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (2048)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 1f)]
        private static void CompressTextureCrunchedDXT5_2048() {
            CompressTextureCrunchedDXT5(2048);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (1024)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 2f)]
        private static void CompressTextureCrunchedDXT5_1024() {
            CompressTextureCrunchedDXT5(1024);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (512)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 3f)]
        private static void CompressTextureCrunchedDXT5_512() {
            CompressTextureCrunchedDXT5(512);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (256)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 4f)]
        private static void CompressTextureCrunchedDXT5_256() {
            CompressTextureCrunchedDXT5(256);
        }
        [MenuItem("Assets/Scoz/SpriteFormat/圖檔壓縮成Crunched DXT5 6X6 (128)", priority = PRIORITY_CONVERT_TO_CRUNCHED_DXT5, secondaryPriority = 5f)]
        private static void CompressTextureCrunchedDXT5_128() {
            CompressTextureCrunchedDXT5(128);
        }
        private static void CompressTextureCrunchedDXT5(int _maxTextureSize) {
            Object[] selectTextureS = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);
            Object selectObj = Selection.activeObject;
            Object[] selectObjS = Selection.objects;

            Selection.activeObject = null;
            Selection.objects = new Object[0];
            foreach (var obj in selectTextureS) {
                if (obj == null)
                    continue;
                var path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
                if (EditorUtility.DisplayCancelableProgressBar("Compressing (Crunched DXT5)... ", path, 0f)) {
                    Selection.activeObject = selectObj;
                    Selection.objects = selectObjS;
                    EditorUtility.ClearProgressBar();
                    return;
                }

                bool bChange = false;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                bChange |= SetTextureRGBACrunchedDXT5(importer, _maxTextureSize);

                if (bChange) {
                    importer.SaveAndReimport();
                }
            }

            Selection.activeObject = selectObj;
            Selection.objects = selectObjS;
            EditorUtility.ClearProgressBar();

            bool SetTextureRGBACrunchedDXT5(TextureImporter _importer, int _maxTextureSize) {
                if (_importer == null)
                    return false;
                _importer.filterMode = FilterMode.Bilinear;
                _importer.npotScale = TextureImporterNPOTScale.None;
                _importer.spriteImportMode = SpriteImportMode.Single;
                bool result = false;
                var setting = _importer.GetPlatformTextureSettings("Standalone");
                if (setting.format != TextureImporterFormat.DXT5Crunched || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.DXT5Crunched;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                setting = _importer.GetPlatformTextureSettings("Android");
                if (setting.format != TextureImporterFormat.DXT5Crunched || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.DXT5Crunched;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                setting = _importer.GetPlatformTextureSettings("iOS");
                if (setting.format != TextureImporterFormat.ASTC_4x4 || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.ASTC_4x4;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                setting = _importer.GetPlatformTextureSettings("WebGL");
                if (setting.format != TextureImporterFormat.DXT5Crunched || setting.compressionQuality != 100 || setting.maxTextureSize != _maxTextureSize) {
                    setting.crunchedCompression = true;
                    setting.format = TextureImporterFormat.DXT5Crunched;
                    setting.compressionQuality = 100;
                    setting.maxTextureSize = _maxTextureSize;
                    setting.overridden = true;
                    _importer.SetPlatformTextureSettings(setting);
                    result |= true;
                }
                return result;
            }
        }
        #endregion
    }
}