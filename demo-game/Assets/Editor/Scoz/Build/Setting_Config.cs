using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using Scoz.Func;
using UnityEngine;

namespace Scoz.Editor {
    public static class Setting_Config {
        public const string PROJECT_SERVER_NAME = "waifu_tower";
        public const string PROJECT_DEPLOYMENT_NAME = "waifu-tower";
        public const string Bundle_NAME = "waifu_tower_bundle";
        public const string PACKAGE_NAME = "waifutower";
        public const string COMPANY_NAME = "aura";
        public const string ADDRESABLE_BIN_PATH = "Assets/AddressableAssetsData/{0}/{1}/{2}/"; // 平台/版本/遊戲版本 例如 WebGL/Dev/1.1
        public const string PATH_MAIN_BUILD_ORIGIN = "Builds/Build/"; // 主程式原本路徑 案根目錄/Builds/Build
        public const string PATH_MAIN_BUILD_TARGET = "Builds/{0}/{1}/{2}/"; // 主程式複製到目標路徑 例如 專案根目錄/Builds/Dev/webgl/1.1
        public const string BUNDLE_PATH = "ServerData/{0}/{1}/{2}/"; // Bundle路徑 ServerData/Dev/webgl/1.1
        public static List<string> ServerJsons = new List<string>() { "" };

        public static EnvVersion GetEnvVersion {
            get {
#if Dev
                return EnvVersion.Dev;
#elif Test
                return EnvVersion.Test;
#elif Release
                return EnvVersion.Release;
#else
                return EnvVersion.Dev;
#endif
            }
        }

        public static Dictionary<EnvVersion, string> GOOGLE_PROJECTS = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, "csc5023-games-dev"},
            { EnvVersion.Test, "csc5023-minigames-test"},
            { EnvVersion.Release, "csc5023-minigames-release"},
        };
        public static Dictionary<EnvVersion, string> GCS_WEBGL_PATHS_DEVTEST = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, $"minigames-devtest/{PROJECT_SERVER_NAME}"},
            { EnvVersion.Test, $"minigames-devtest/{PROJECT_SERVER_NAME}"},
            { EnvVersion.Release, $"minigames-devtest/{PROJECT_SERVER_NAME}"},
        };
        public static Dictionary<EnvVersion, string> GCS_WEBGL_PATHS = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, $"minigames-static-public-dev/{PROJECT_SERVER_NAME}/webgl"},
            { EnvVersion.Test, $"cdn-games-test-mini-s.88play.online/{PROJECT_SERVER_NAME}/webgl"},
            { EnvVersion.Release, $"cdn-games-mini-s.epicminigame.net/{PROJECT_SERVER_NAME}/webgl"},
        };
        public static Dictionary<EnvVersion, string> GCS_BUNDLE_PATHS = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, $"minigames-public-dev/{Bundle_NAME}/webgl"},
            { EnvVersion.Test, $"cdn-games-test-mini-d.88play.online/{Bundle_NAME}/webgl"},
            { EnvVersion.Release, $"cdn-games-mini-d.epicminigame.net/{Bundle_NAME}/webgl"},
        };
        public static Dictionary<EnvVersion, string> GCS_JSON_PATHS = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, $"minigames-private-dev/gamejsons/{PROJECT_SERVER_NAME}"},
            { EnvVersion.Test, $"minigames-private-test/gamejsons/{PROJECT_SERVER_NAME}"},
            { EnvVersion.Release, $"minigames-private-release/gamejsons/{PROJECT_SERVER_NAME}"},
        };

        public static Dictionary<EnvVersion, string> ADDRESABALE_PROFILES = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, "GoogleCloud-Dev"},
            { EnvVersion.Test, "GoogleCloud-Test"},
            { EnvVersion.Release, "GoogleCloud-Release"},
        };
        public static Dictionary<EnvVersion, string> KEYSTORE_ALIAS = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, "123456"},
            { EnvVersion.Test, "123456"},
            { EnvVersion.Release, "123456"},
        };

        public static Dictionary<EnvVersion, string> PACKAGE_NAMES = new Dictionary<EnvVersion, string>() {
            { EnvVersion.Dev, $"com.{COMPANY_NAME}.{PACKAGE_NAME}.dev"},
            { EnvVersion.Test, $"com.{COMPANY_NAME}.{PACKAGE_NAME}.test"},
            { EnvVersion.Release, $"com.{COMPANY_NAME}.{PACKAGE_NAME}"},
        };
    }
}