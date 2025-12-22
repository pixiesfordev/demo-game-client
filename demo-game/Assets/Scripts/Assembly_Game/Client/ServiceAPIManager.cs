using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Scoz.Func;
using UnityEngine;
using static Scoz.Func.Poster;

namespace tower.main {
    public class ServiceAPIManager {
        static string domain;
#if Release
        static string domain_main;
        static string domain_backup;
#endif
        static UniTask initTask = UniTask.CompletedTask;


        public static void Init() {
            Init("");
        }

        public static void Init(string _token) {
            if (!MyEnum.TryParseEnum($"Domain_{GameManager.CurVersion}_Service", out GameSetting settingKey_domain)) return;
            var baseDomain = JsonGameSetting.GetStr(settingKey_domain);
            domain = baseDomain;
#if Release
            if (!MyEnum.TryParseEnum("Domain_Release_Main_Suffix", out GameSetting settingKey_domain_main_suffix)) return;
            if (!MyEnum.TryParseEnum("Domain_Release_Backup_Suffix", out GameSetting settingKey_domain_backup_suffix)) return;
            var domain_main_suffix = JsonGameSetting.GetStr(settingKey_domain_main_suffix);
            var domain_backup_suffix = JsonGameSetting.GetStr(settingKey_domain_backup_suffix);

            domain_main = baseDomain + domain_main_suffix;
            domain_backup = baseDomain + domain_backup_suffix;

            domain = domain_main;
            initTask = InitDomainByOnlineCheck(_token).Preserve();
#else
            initTask = UniTask.CompletedTask;
#endif
            WriteLog.LogColor($"service domain: {domain}", WriteLog.LogType.ServerAPI_Info);
        }

#if Release
        static async UniTask InitDomainByOnlineCheck(string _token) {
            bool canConnectMain = await CanConnect(domain_main, _token);
            if (canConnectMain) {
                domain = domain_main;
                Debug.Log("使用serviceAPI domain_main");
                //WriteLog.LogColor($"service domain online: {domain}", WriteLog.LogType.ServerAPI_Info);
                return;
            }

            domain = domain_backup;
            Debug.Log("使用serviceAPI domain_backup");
            //WriteLog.LogColor($"service domain switch to backup: {domain}", WriteLog.LogType.ServerAPI_Info);
        }

        static async UniTask<bool> CanConnect(string _domain, string _token) {
            string url = $"{_domain}/auth/player/online";
            var sendData = new { };
            var qResp = await Query(QueryType.Post, url, _token, sendData);
            return qResp != null;
        }
#endif

        static UniTask WaitInit() {
            return initTask;
        }


        public static async UniTask<(string, bool)> CreatePlayer() {
            await WaitInit();
            if (string.IsNullOrEmpty(domain)) return (null, false);

            string url = $"{domain}/auth/player/create";
            var sendData = new {
                game = "waifu-tower",
                gold = 5000,
            };
            var qResp = await Query(QueryType.Post, url, "", sendData);
            if (qResp == null) return (null, false);
            var decode = Decode<Signup_Res>(qResp.Content);
            if (qResp.Result == false || decode == null || decode.Token == null) {
                return (null, false);
            }
            return (decode.Token, qResp.Result);
        }
        public class Signup_Res {
            [JsonProperty("token")]
            public string Token;
        }


        public static async UniTask<(RefreshToken_Res, string)> RefreshToken(string _token) {
            await WaitInit();
            if (string.IsNullOrEmpty(domain)) return (null, "domain is null");

            string url = $"{domain}/auth/player/token/refresh";
            var sendData = new {
            };
            var qResp = await Query(QueryType.Post, url, _token, sendData);
            if (qResp == null) return (null, "refresh resp is null");
            var decode = Decode<RefreshToken_Res>(qResp.Content);
            if (decode == null) return (null, "refresh resp decode fail");
            return (decode, qResp.ErrorCode);
        }
        public class RefreshToken_Res {
            [JsonProperty("expired_at")]
            public string ExpiredAt;
            [JsonProperty("is_refresh_expired")]
            public bool Is_refresh_expired;
            [JsonProperty("player_token")]
            public string PlayerToken;
            [JsonProperty("refresh_expired_at")]
            public string RefreshExpiredAt;
        }


        public static async UniTask<(bool, string)> OnlineCheck(string _token) {
            await WaitInit();
            if (string.IsNullOrEmpty(domain)) return (false, "domain is null");

            string url = $"{domain}/auth/player/online";
            var sendData = new { };
            var qResp = await Query(QueryType.Post, url, _token, sendData);
            if (qResp == null) return (false, "online resp is null");
            return (qResp.Result, qResp.ErrorCode);
        }


        public static void Offline(string _token) {
            OfflineAsync(_token).Forget();
        }

        static async UniTask OfflineAsync(string _token) {
            await WaitInit();
            if (string.IsNullOrEmpty(domain)) return;

            string url = $"{domain}/auth/player/offline";
            var sendData = new {
            };
            await Query(QueryType.Post, url, _token, sendData);
        }


        public static async UniTask FunPlay(string _token) {
            await WaitInit();
            if (string.IsNullOrEmpty(domain)) return;

            string url = $"{domain}/auth/player/funplay";
            var sendData = new {
            };
            await Query(QueryType.Post, url, _token, sendData);
        }
    }
}
