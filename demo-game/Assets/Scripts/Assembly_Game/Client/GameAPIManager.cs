using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Scoz.Func;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Scoz.Func.Poster;

namespace tower.main {
    public class GameAPIManager {
        static string domain;
        static string queryUrl;

#if Release
        static string domain_main;
        static string domain_backup;
#endif
        static UniTask initTask = UniTask.CompletedTask;

        public static void Init() {
            if (!MyEnum.TryParseEnum($"Domain_{GameManager.CurVersion}_Game", out GameSetting settingKey_domain)) return;
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
            queryUrl = GetQueryUrl(domain);
            initTask = InitDomainByHealthCheck().Preserve();
#else
            queryUrl = GetQueryUrl(domain);
            initTask = UniTask.CompletedTask;
#endif

            WriteLog.LogColor($"game domain: {domain}  query url: {queryUrl}", WriteLog.LogType.ServerAPI_Info);
        }

        static string GetQueryUrl(string _domain) {
            string url = $"{_domain}/{GameManager.PROJECT_NAME}";
#if Dev || UNITY_EDITOR
            url += "-test";
#endif
            return url;
        }

        static UniTask WaitInit() {
            return initTask;
        }

#if Release
        static async UniTask InitDomainByHealthCheck() {
            bool canConnectMain = await CanConnect(domain_main);
            if (canConnectMain) {
                domain = domain_main;
                queryUrl = GetQueryUrl(domain);
                Debug.Log("使用 gameAPI domain_main");
                //WriteLog.LogColor($"game domain online: {domain}  query url: {queryUrl}", WriteLog.LogType.ServerAPI_Info);
                return;
            }

            domain = domain_backup;
            queryUrl = GetQueryUrl(domain);
            Debug.Log("使用 gameAPI domain_backup");
            //WriteLog.LogColor($"game domain switch to backup: {domain}  query url: {queryUrl}", WriteLog.LogType.ServerAPI_Info);
        }

        static async UniTask<bool> CanConnect(string _domain) {
            var _queryUrl = GetQueryUrl(_domain);
            string url = $"{_queryUrl}/health";
            var sendData = new { };
            var qResp = await Query(QueryType.Get, url, "", sendData);
            return qResp != null && qResp.ErrorCode == null;
        }
#endif

        public static async UniTask InitPing() {
            await WaitInit();
            await UniTask.WaitForSeconds(5); // 5秒後開Ping
            List<double> pings = new List<double>();
            for (int i = 0; i < 3; i++) {
                await UniTask.WaitForSeconds(1); // 1秒Ping一次
                var miliSecs = await Ping();
                pings.Add(miliSecs);
            }
            double avgPing = pings.Average(); // 計算平均
            WriteLog.LogColor($"平均 Ping: {avgPing:F2} ms", WriteLog.LogType.ServerAPI_Resp);
        }


        public class Resp_State {
            [JsonProperty("game_state")]
            public Resp_GameState game_state; // 遊戲狀態
            [JsonProperty("player_gold")]
            public double player_gold; // 玩家金幣(點數)
            [JsonProperty("remaining_funplay_count")]
            public int remaining_funplay_count; // 該玩家當日剩餘 FunPlay 次數
        }
        public class Resp_Start {
            [JsonProperty("cost")]
            public double cost; // 玩家花費
            [JsonProperty("game_state")]
            public Resp_GameState game_state; // 遊戲狀態
            [JsonProperty("player_gold")]
            public double player_gold; // 玩家金幣(點數)
        }
        public class Resp_Choose {
            [JsonProperty("game_result")]
            public Resp_GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
            [JsonProperty("game_state")]
            public Resp_GameState? game_state;  // 遊戲狀態
            [JsonProperty("is_game_timeout_settled")]
            public bool is_game_timeout_settled; // 遊戲是否已逾時結算
            [JsonProperty("player_gold")]
            public double? player_gold; // 玩家金幣
            [JsonProperty("reached_max_level")]
            public bool reached_max_level; // 是否抵達最大層數
        }
        public class Resp_GameResult {
            [JsonProperty("ended_at")]
            public DateTime ended_at { get; set; } // 遊戲結束時間
            [JsonProperty("game_id")]
            public string game_id; // 遊戲ID
            [JsonProperty("level_auto_choices")]
            public List<int> level_auto_choices; // 自動遊戲選擇的 Blocks Indexes 
            [JsonProperty("level_choices")]
            public List<int> level_choices; // 玩家選擇的 Blocks Indexes
            [JsonProperty("level_results")]
            public List<List<bool>> level_results; // 遊戲結果的 Blocks
            [JsonProperty("odds")]
            public double odds; // 獎勵倍率
            [JsonProperty("order_id")]
            public string order_id; // 母注單 ID (訂單編號)
            [JsonProperty("random_count")]
            public int random_count; // 隨機選擇的次數 (隨機敲磚次數)
            [JsonProperty("reached_level")]
            public int reached_level; // 敲專次數 (敲磚層數)
            [JsonProperty("reward")]
            public double reward; // 獎勵
            [JsonProperty("started_at")]
            public DateTime started_at { get; set; } // 遊戲開始時間
        }
        public class Resp_GameState {
            [JsonProperty("bet")]
            public double bet; // 下注額
            [JsonProperty("curr_odds")]
            public double curr_odds; // 當前倍率
            [JsonProperty("curr_reward")]
            public double curr_reward; // 當前獎勵
            [JsonProperty("difficulty")]
            public string difficulty; // 遊戲難度 ("easy" | "medium" | "hard" | "expert" | "master")
            [JsonProperty("game_id")]
            public string game_id; // 遊戲 ID
            [JsonProperty("level_auto_choices")]
            public List<int> level_auto_choices; // 自動遊戲選擇的 Blocks Indexes 
            [JsonProperty("level_choices")]
            public List<int> level_choices; // 玩家選擇的 Blocks Indexes
            [JsonProperty("level_results")]
            public List<List<bool>> level_results; // 遊戲結果的 Blocks
            [JsonProperty("order_id")]
            public string order_id;                   // 母注單 ID
            [JsonProperty("random_count")]
            public int random_count; // 隨機選擇的次數
            [JsonProperty("reached_level")]
            public int reached_level; // 敲磚次數

            public Resp_GameState() { }
            public Resp_GameState(Resp_GameResult _result, double _bet, string _difficulty) {
                bet = _bet;
                curr_odds = _result.odds;
                curr_reward = _result.reward;
                difficulty = _difficulty;
                game_id = _result.game_id;
                level_auto_choices = _result.level_auto_choices;
                level_choices = _result.level_choices;
                level_results = _result.level_results;
                order_id = _result.order_id;
                random_count = _result.random_count;
                reached_level = _result.reached_level;
            }

        }
        public class Resp_Auto {
            [JsonProperty("game_result")]
            public Resp_GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
            [JsonProperty("game_state")]
            public Resp_GameState? game_state;  // 遊戲狀態
            [JsonProperty("player_gold")]
            public double player_gold; // 玩家金幣
        }
        public class Resp_Cashout {
            [JsonProperty("game_result")]
            public Resp_GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
            [JsonProperty("is_game_timeout_settled")]
            public bool is_game_timeout_settled; // 遊戲是否已逾時結算
            [JsonProperty("player_gold")]
            public double player_gold; // 玩家金幣
        }

        public class Resp_History {
            [JsonProperty("game_logs")]
            public List<Resp_GameLog> game_logs; // 遊戲紀錄
        }

        public class Resp_GameLog {
            [JsonProperty("bet")]
            public double bet; // 下注額
            [JsonProperty("ended_at")]
            public DateTime ended_at { get; set; } // 遊戲結束時間
            [JsonProperty("game_id")]
            public string game_id; // 遊戲ID
            [JsonProperty("level_auto_choices")]
            public List<int> level_auto_choices; // 自動遊戲選擇的 Blocks Indexes 
            [JsonProperty("level_choices")]
            public List<int> level_choices; // 玩家選擇的 Blocks Indexes
            [JsonProperty("level_results")]
            public List<List<bool>> level_results; // 遊戲結果的 Blocks
            [JsonProperty("odds")]
            public double odds; // 獎勵倍率
            [JsonProperty("order_id")]
            public string order_id; // 母注單 ID (訂單編號)
            [JsonProperty("random_count")]
            public int random_count; // 隨機選擇的次數 (隨機敲磚次數)
            [JsonProperty("reached_level")]
            public int reached_level; // 敲專次數 (敲磚層數)
            [JsonProperty("reward")]
            public double reward; // 獎勵
            [JsonProperty("started_at")]
            public DateTime started_at { get; set; } // 遊戲開始時間
        }
        /// <summary>
        /// Ping
        /// </summary>
        public static async UniTask<double> Ping() {
            await WaitInit();
            string url = $"{queryUrl}/health";
            var sendData = new {
            };
            var before = DateTime.Now;
            await Query(QueryType.Get, url, "", sendData);
            double miliSecs = (DateTime.Now - before).TotalMilliseconds;
            //WriteLog.LogColor($"Ping 封包花費 {miliSecs} 毫秒", WriteLog.LogType.ServerAPI);
            return miliSecs;
        }
        static async UniTask<(TResp result, string error)> postAndDecode<TResp>(string request, object sendData) where TResp : class {
            await WaitInit();
            if (string.IsNullOrEmpty(queryUrl)) return (default, "queryUrl is null");

            string url = $"{queryUrl}/{request}";
            var before = DateTime.Now;
            var qResp = await Query(QueryType.Post, url, "", sendData);
            WriteLog.LogColor($"{request} 請求花費 {(DateTime.Now - before).TotalMilliseconds} 毫秒", WriteLog.LogType.ServerAPI_Query);
            if (qResp == null) return (default, $"Request({request}) resp is null");
            if (qResp.ErrorCode != null) return (default, qResp.ErrorCode);
            if (qResp.Result == false) return (default, $"Request({request}) failed");
            try {
                var decode = Decode<TResp>(qResp.Content);
                if (decode == null) return (default, $"Request({request}) decode error");
                return (decode, null);
            } catch (Exception ex) {
                return (default, $"Request({request}) decode exception: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得遊戲狀態
        /// </summary>
        public static async UniTask<(Resp_State, string)> State() {
            return await postAndDecode<Resp_State>("state", new { token = GameConnetor.Instance.Token });
        }
        /// <summary>
        /// 開始遊戲
        /// </summary>
        public static async UniTask<(Resp_Start, string)> Start(double _bet, string _difficulty) {
            return await postAndDecode<Resp_Start>("start", new { bet = _bet, difficulty = _difficulty, token = GameConnetor.Instance.Token });
        }
        /// <summary>
        /// 取得下注紀錄
        /// </summary>
        public static async UniTask<(Resp_History, string)> History(int _n) {
            return await postAndDecode<Resp_History>("history", new { n = _n, token = GameConnetor.Instance.Token });
        }
        /// <summary>
        /// 選擇磚塊
        /// </summary>
        public static async UniTask<(Resp_Choose, string)> Choose(int _chosen_index, string _game_id, bool _is_random) {
            return await postAndDecode<Resp_Choose>("choose", new { chosen_index = _chosen_index, game_id = _game_id, is_random = _is_random, token = GameConnetor.Instance.Token });
        }
        /// <summary>
        /// 收手
        /// </summary>
        public static async UniTask<(Resp_Cashout, string)> Cashout(string _game_id) {
            return await postAndDecode<Resp_Cashout>("cashout", new { game_id = _game_id, token = GameConnetor.Instance.Token });
        }
        /// <summary>
        /// 自動下注
        /// </summary>
        public static async UniTask<(Resp_Auto, string)> Auto(double _bet, List<int> _chosen_idxs, string _difficulty) {
            return await postAndDecode<Resp_Auto>("auto", new { bet = _bet, chosen_idxs = _chosen_idxs, difficulty = _difficulty, token = GameConnetor.Instance.Token });
        }
    }
}
