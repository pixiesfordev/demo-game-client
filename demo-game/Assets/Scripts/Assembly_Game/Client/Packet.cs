#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Packet {
    [System.Serializable]
    public class AuthRequest {
        public string token;          // 玩家 Token
    }

    [System.Serializable]
    public class AuthResponse {
        public string? error_code;    // 錯誤碼
        public bool is_auth;          // 是否驗證成功
    }

    [System.Serializable]
    public class StateRequest {
        // 空實作
    }

    [System.Serializable]
    public class StateResponse {
        public string? error_code;    // 錯誤碼
        public GameState? game_state;  // 遊戲狀態
        public double player_gold;     // 玩家金幣(點數)
        public int remaining_funplay_count; // 該玩家當日剩餘 FunPlay 次數
    }

    [System.Serializable]
    public class GameState {
        public double bet; // 下注額
        public double curr_odds; // 當前倍率
        public double curr_reward; // 當前獎勵
        public string difficulty;  // 遊戲難度 ("easy" | "medium" | "hard" | "expert" | "master")
        public string game_id;                   // 遊戲 ID
        public List<int> level_auto_choices; // 自動遊戲的選擇 
        public List<int> level_choices; // 玩家的選擇
        public List<List<bool>> level_results; // 遊戲結果的 Blocks
        public string order_id;                   // 母注單 ID
        public int random_count; // 隨機選擇的次數
        public int reached_level; // 敲磚次數
    }

    [System.Serializable]
    public class StartRequest {
        public double bet; // 下注額
        public string difficulty; // 遊戲難度 ("easy" | "medium" | "hard" | "expert" | "master")
        public string token; // 玩家 Token
    }

    [System.Serializable]
    public class StartResponse {
        public string? error_code;    // 錯誤碼
        public double cost;            // 遊玩花費
        public GameState? game_state;  // 遊戲狀態
        public double player_gold;     // 玩家金幣
    }



    [System.Serializable]
    public class HistoryRequest {
        public uint n { get; set; }   // 獲取最多 N 筆遊戲紀錄
        public string token; // 玩家 Token
    }

    [System.Serializable]
    public class HistoryResponse {
        public string? error_code;    // 錯誤碼
        public GameLog? history_best { get; set; }                           // 遊戲最佳紀錄
        public List<GameLog> game_logs;  // 遊戲紀錄
    }

    [System.Serializable]
    public class GameLog {
        public DateTime ended_at { get; set; }                           // 遊戲結束時間
        public string game_id;               // 遊戲ID
        public List<int> level_auto_choices; // 自動遊戲的選擇 
        public List<int> level_choices; // 玩家的選擇
        public List<List<bool>> level_results; // 遊戲結果的 Blocks
        public double odds; // 獎勵倍率
        public string order_id; // 母注單 ID (訂單編號)
        public int random_count; // 隨機選擇的次數 (隨機敲磚次數)
        public int reached_level; // 敲專次數 (敲磚層數)
        public double reward; // 獎勵
        public DateTime started_at { get; set; }                           // 遊戲開始時間
    }


    [System.Serializable]
    public class ChooseRequest {
        public int chosen_index; // 選擇的格子 Index
        public string game_id;                // 遊戲ID
        public bool is_random; // 是否為隨機選擇
        public string token; // 玩家 Token
    }
    [System.Serializable]
    public class ChooseResponse {
        public GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
        public GameState? game_state;  // 遊戲狀態
        public bool is_game_timeout_settled; // 遊戲是否已逾時結算
        public double player_gold; // 玩家金幣
        public bool reached_max_level; // 是否抵達最大層數
    }
    [System.Serializable]
    public class GameResult {
        public DateTime ended_at { get; set; }                           // 遊戲結束時間
        public string game_id;               // 遊戲ID
        public List<int> level_auto_choices; // 自動遊戲的選擇 
        public List<int> level_choices; // 玩家的選擇
        public List<List<bool>> level_results; // 遊戲結果的 Blocks
        public double odds; // 獎勵倍率
        public string order_id; // 母注單 ID (訂單編號)
        public int random_count; // 隨機選擇的次數 (隨機敲磚次數)
        public int reached_level; // 敲專次數 (敲磚層數)
        public double reward; // 獎勵
        public DateTime started_at { get; set; }                           // 遊戲開始時間
    }


    [System.Serializable]
    public class AutoRequest {
        public double bet; // 下注額
        public List<int> chosen_idxs; // 選擇的格子 Indexes
        public string difficulty; // 遊戲難度 ("easy" | "medium" | "hard" | "expert" | "master")
        public string token; // 玩家 Token
    }
    [System.Serializable]
    public class AutoResponse {
        public GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
        public GameState? game_state;  // 遊戲狀態
        public double player_gold; // 玩家金幣
    }


    [System.Serializable]
    public class CashoutRequest {
        public double bet; // 下注額
        public List<int> chosen_idxs; // 選擇的格子 Indexes
        public string difficulty; // 遊戲難度 ("easy" | "medium" | "hard" | "expert" | "master")
        public string token; // 玩家 Token
    }
    [System.Serializable]
    public class CashoutResponse {
        public GameResult? game_result;  // 遊戲結果 (輸掉 or 抵達最大層數)
        public bool is_game_timeout_settled; // 遊戲是否已逾時結算
        public double player_gold; // 玩家金幣
    }


    [System.Serializable]
    public class PingRequest {
        // 空實作
    }

    [System.Serializable]
    public class PingResponse {
        // 空實作
    }


}
