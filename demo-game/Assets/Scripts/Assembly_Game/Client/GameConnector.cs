using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Packet;
using Scoz.Func;
using System;
using Cysharp.Threading.Tasks;
using tower.main;


public class GameConnetor : MonoBehaviour {
    public static GameConnetor Instance { get; private set; }
    public string Token { get; private set; }
    bool onlineStateCheckStarted = false;
    bool refreshLoopStarted = false;
    const float REFRESH_ONLINE_SECS = 10;
    const float REFRESH_TOKENE_MINS = 5;

    // 初始連線跟刷新Token
    Action onFinishStartConnect = null;
    public bool StartConnected { get; private set; }


    //// ping延遲紀錄
    //readonly Queue<long> pingRecords = new Queue<long>();
    ///// <summary>
    ///// 目前的平均延遲(毫秒)，若筆數不足則回傳 null
    ///// </summary>
    //public double PingLatency => pingRecords.Count == 0 ? 9999 : pingRecords.Average();

    public void Init() {
        Instance = this;

        ServiceAPIManager.Init();
        GameAPIManager.Init();
    }


    public async UniTask<(bool, string)> Connect() {
        WriteLog.LogColor("開始連線", WriteLog.LogType.Connection);
        // Token檢查
        bool gotToken = await TokenCheck();
        if (!gotToken) {
            WriteLog.LogError("取得Token失敗");
            await UniTask.SwitchToMainThread();
            LeaveGame(JsonString.GetUIString("AuthFail_Content"));
            return (false, JsonString.GetUIString("AuthFail_Content"));
        }

        // 初始化刷新Token
        var initToken = Token;
        var (refreshResult, errorStr) = await RefreshToken();
        if (refreshResult != RefreshTokenResult.OK) {
            await UniTask.SwitchToMainThread();
            WriteLog.LogError($"Token刷新錯誤");
            LeaveGame($"{JsonString.GetUIString("AuthFail_Content")} error_code: {errorStr}");
            return (false, $"error_code: {errorStr}");
        }
        WriteLog.LogColor($"初始Token: {initToken}  刷新後Token: {Token}", WriteLog.LogType.Connection);
        WriteLog.LogColor("連線成功", WriteLog.LogType.Connection);
        onFinishStartConnect?.Invoke();
        StartConnected = true;
        return (true, "");
    }
    public void RegisterOnFinishConnectAC(Action _ac) {
        onFinishStartConnect = _ac;
    }

    private void OnDestroy() {
        closeRefreshLoop();
    }

    public void StartRefreshLoop() {
        startRefreshTokenLoop().Forget(); // 每X分鐘定時刷新Token
        startRefreshOnlineStateLoop().Forget(); // 每X秒刷新上線狀態
    }

    void closeRefreshLoop() {
        onlineStateCheckStarted = false;
        refreshLoopStarted = false;
    }


    bool isDevTest() {
#if Dev
        //string url = Application.absoluteURL;
        //return url.Contains("minigames-devtest");
        return true;
#else
        return false;
#endif
    }


    public async UniTask<bool> TokenCheck() {
        if (string.IsNullOrEmpty(GamePlayer.Instance.GameToken)) {
            string token = null;

            if (isDevTest()) {
                WriteLog.LogColor("尚未取得 GameToken，向 Server 要一個新 Token", WriteLog.LogType.ServerAPI_Info);
                token = URLParamReader.GetStr("token");
                //token = "eyJhbGciOiJFZERTQSIsInR5cCI6IkpXVCJ9.eyJjdXJyZW5jeSI6IktWTkQiLCJleHBpcmVkIjoxNzU3MzU4MTg0LCJnYW1lIjoiY2FyZC1zd2FwIiwiaWQiOiI2OGJmMjNiN2YzNmJjM2NjYmJiMTY1NjAiLCJsYW5ndWFnZSI6InZuIiwibm90ZSI6LTEsInBsYXRmb3JtIjoidGVzdCIsInJlZnJlc2hfZXhwaXJlZCI6MTc1NzM1ODc4NCwidW5pcXVlIjoiNGFiMjM0NmMtMWI0NTg1YTA2MzZiNjAwMCJ9.CCo6_W8HRlUUlpRTs6zXBJiuSYEfjHXz9CPDYaEF7HhGNysmgqrFbzWoNpYCAz-IuNSE1EHyC1JkCYEuvPFiAg";
                if (string.IsNullOrEmpty(token)) {
                    WriteLog.LogColor("Token取得失敗 測試環境直接跟server要一個新Token", WriteLog.LogType.ServerAPI_Info);
                    await getAndSetNewToken();
                } else {
                    setToken(token);
                }
                return true;
            } else {
                await UniTask.SwitchToMainThread();
                token = URLParamReader.GetStr("token");
                if (string.IsNullOrEmpty(token)) {
                    Debug.LogError("初始Token取得失敗"); // Release取不到Token也要輸出Log
                    return false;
                }
                setToken(token);
                return true;
            }

#if Dev && UNITY_EDITOR
            WriteLog.LogColor("尚未取得GameToken，向Server要一個新Token", WriteLog.LogType.ServerAPI_Info);
            await getAndSetNewToken();
            return true;
#elif Dev && !UNITY_EDITOR
        WriteLog.LogColor("尚未取得GameToken，向Server要一個新Token", WriteLog.LogType.ServerAPI_Info);
        await getAndSetNewToken();
        return true;
#else
            await UniTask.SwitchToMainThread();
            token = URLParamReader.GetStr("token");
            if (string.IsNullOrEmpty(token)) {
                WriteLog.LogError("Auth失敗 Token取得失敗");
                return false;
            }
            setToken(token);
            return true;
#endif

        } else {
            setToken(GamePlayer.Instance.GameToken);
            return true;
        }
    }

    public enum RefreshTokenResult { OK, FAIL };
    DateTime lastRefreshTokenTime;
    public async UniTask<(RefreshTokenResult, string)> RefreshToken() {
        await UniTask.SwitchToMainThread();

        var beforeToken = Token;
        var (resp, error) = await ServiceAPIManager.RefreshToken(Token);
        if (!string.IsNullOrEmpty(error)) {
            WriteLog.LogError($"Token刷新失敗 error_code: {error}");
#if UNITY_EDITOR
            await getAndSetNewToken();
            return (RefreshTokenResult.OK, "");
#endif
            return (RefreshTokenResult.FAIL, error);
        } else {
            lastRefreshTokenTime = DateTime.Now;
            if (beforeToken == resp.PlayerToken) WriteLog.LogWarning($"刷新Token異常 刷新後Token沒變: ${resp.PlayerToken}");
            setToken(resp.PlayerToken);
            return (RefreshTokenResult.OK, "");
        }
    }

    async UniTask getAndSetNewToken() {
        var (token, result) = await ServiceAPIManager.CreatePlayer();
        if (result == false || token == null) {
            await UniTask.SwitchToMainThread();
            WriteLog.LogError("APIManager.CreatePlayer失敗");
            PopupUI.ShowClickCancel(JsonString.GetUIString("AuthFail_Title"), JsonString.GetUIString("AuthFail_Content"), () => {
                UnityAssemblyCaller.Invoke("JSFuncWrapper", "SendAction", true, "game_leave");
            });
            return;
        }
        WriteLog.Log($"取得測試用新Token: {token}");
        if (token == null) return;
        setToken(token);
    }

    void setToken(string _token) {
        if (string.IsNullOrEmpty(_token)) {
            WriteLog.LogError($"傳入錯誤的參數 _toekn: {_token} ");
            return;
        }
        Token = _token;
        GamePlayer.Instance.SetGameToken(Token);
#if UNITY_EDITOR
        GamePlayer.Instance.SaveSettingToLoco();
#endif
        WriteLog.Log($"設定新Token: {Token}");
    }

    /// <summary>
    /// 每 X 分鐘定時刷新Token
    /// </summary>
    async UniTaskVoid startRefreshTokenLoop() {
        if (!StartConnected) return;
        if (refreshLoopStarted) return;
        if (string.IsNullOrEmpty(Token)) {
            WriteLog.LogError("startRefreshTokenLoop error, Token IsNullOrEmpty");
            return;
        }
        refreshLoopStarted = true;

        while (refreshLoopStarted) {
            await UniTask.Delay(TimeSpan.FromMinutes(REFRESH_TOKENE_MINS));
            var (result, errorStr) = await RefreshToken();
            if (result != RefreshTokenResult.OK) {
                await UniTask.SwitchToMainThread();
                HandleDisconnect($"{JsonString.GetUIString("AuthFail_Content")}");
                refreshLoopStarted = false;
                break;
            }
        }
    }
    /// <summary>
    /// 上線狀態刷新
    /// </summary>
    async UniTaskVoid startRefreshOnlineStateLoop() {
        if (onlineStateCheckStarted) return;
        if (string.IsNullOrEmpty(Token)) {
            WriteLog.LogError("startRefreshOnlineStateLoop error, Token IsNullOrEmpty");
            return;
        }
        onlineStateCheckStarted = true;
        // 每10秒刷新一次上線狀態
        while (onlineStateCheckStarted) {
            try {
                // 10秒內沒回傳算失敗
                await UniTask.SwitchToMainThread();
                var (success, errorCode) = await ServiceAPIManager.OnlineCheck(Token).Timeout(TimeSpan.FromSeconds(REFRESH_ONLINE_SECS));
                if (!string.IsNullOrEmpty(errorCode)) {
                    WriteLog.LogError($"ErrorCode: {errorCode}");
                    await UniTask.SwitchToMainThread();
                    switch (errorCode) {
                        case "a-0-2-5":
                            LeaveGame($"{JsonString.GetUIString("AuthFail_Content_MultipleLogin")}  error_code: {errorCode}");
                            break;
                        default:
                            LeaveGame($"{JsonString.GetUIString("AuthFail_Content")}  error_code: {errorCode}");
                            break;
                    }
                    break;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(15));
            } catch (TimeoutException) {
                await UniTask.SwitchToMainThread();
                HandleDisconnect($"{JsonString.GetUIString("AuthFail_Content")}");
                break;
            }
        }
        onlineStateCheckStarted = false;
    }

    public void HandleDisconnect(string _errorStr) {
        closeRefreshLoop();
        PopupUI.ShowClickCancel(JsonString.GetUIString("Disconnect_Title"),
            _errorStr,
            async () => {
                await UniTask.Delay(TimeSpan.FromSeconds(0.3));

                RefreshTokenResult tokenResult = RefreshTokenResult.FAIL;
                string errorStr = "";
                try {
                    // 如果 28 秒內沒完成 直接拋出 TimeoutException 設比Poster的逾時時間還短
                    (tokenResult, errorStr) = await GameConnetor.Instance.RefreshToken()
                        .Timeout(TimeSpan.FromSeconds(28));

                } catch (TimeoutException) {
                    WriteLog.LogError("RefreshToken 逾時");
                    tokenResult = RefreshTokenResult.FAIL;
                }

                if (tokenResult != RefreshTokenResult.OK) {
                    WriteLog.LogError(" RefreshToken 失敗");
                    LeaveGame($"{JsonString.GetUIString("AuthFail_Content")} error_code: {errorStr}");
                    return;
                }

                // 刷新成功
                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                // 觸發重連遊戲
                bool success = false;
                try {
                    success = await MainSceneUI.GetInstance<MainSceneUI>().GetGameState().Timeout(TimeSpan.FromSeconds(28)); // 設比Poster的逾時時間還短
                } catch (TimeoutException) {
                    WriteLog.LogError("GetGameState 逾時");
                    success = false;
                } finally {
                    PopupUI.HideLoading();
                }

                if (!success) {
                    // 重連逾時或失敗
                    await UniTask.SwitchToMainThread();
                    LeaveGame(JsonString.GetUIString("Disconnect_Content_ReconnectFail"));
                    return;
                }
                WriteLog.Log("斷線重連成功");
            }
        );
    }

    public void LeaveGame(string _str) {
        WriteLog.Log($"離開遊戲");
        closeRefreshLoop();
        ServiceAPIManager.Offline(GamePlayer.Instance.GameToken);
        PopupUI.ShowClickCancel(JsonString.GetUIString("Popup_SysInfo"), _str, () => {
            UnityAssemblyCaller.Invoke("JSFuncWrapper", "SendAction", true, "game_leave");
        });
    }

}
