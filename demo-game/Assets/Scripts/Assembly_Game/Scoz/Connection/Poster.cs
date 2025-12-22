using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scoz.Func {
    public class Poster : MonoBehaviour {

        const int TIMEOUT_SECS = 30; // 逾時時間
        const string TIMEOUT_ERROR = "timeout";

        [Serializable]
        public class ErrorResponse {
            public string error_code;
        }
        public enum QueryType {
            Get,
            Post,
        }
        public class QueryResp {
            public string Content = null;
            public string ErrorCode = null;
            public bool Result = false;
        }

        public static async UniTask<QueryResp> Query(QueryType _type, string _url, string _token, object _data) {
            QueryResp qResp = new QueryResp();
            if (string.IsNullOrEmpty(_url)) {
                WriteLog.LogError($"{_type} uri 為 null");
                return qResp;
            }
            if (_type == QueryType.Post && _data == null) {
                WriteLog.LogError($"{_type} _data 為 null");
                return qResp;
            }
            string jsonBody = JsonConvert.SerializeObject(_data);
            if (_type == QueryType.Get) {
                qResp = await Get(_url, _token);
            } else {
                qResp = await Post(_url, _token, jsonBody);
            }
            return qResp;
        }
        public static T Decode<T>(string _res) where T : class {
            if (string.IsNullOrEmpty(_res)) {
                WriteLog.LogError("_res 為空");
                return null;
            }
            var resp = JsonConvert.DeserializeObject<T>(_res);
            if (resp == null) {
                return null;
            }
            return resp;
        }
        static string tryParseErrorCode(string json) {
            if (string.IsNullOrEmpty(json)) return null;
            try {
                var jo = JObject.Parse(json);
                var code = jo["error_code"]?.ToString();
                return string.IsNullOrEmpty(code) ? null : code;
            } catch {
                return null;
            }
        }

        static string extractJsonFromText(string text) {
            if (string.IsNullOrEmpty(text)) return null;
            int start = text.IndexOf('{');
            int end = text.LastIndexOf('}');
            if (start >= 0 && end > start) {
                return text.Substring(start, end - start + 1);
            }
            return null;
        }

        /// <summary>
        /// 執行 HTTP POST，回傳 (responseContent, true) 或 (null, false)
        /// </summary>
        public static async UniTask<QueryResp> Post(string _url, string _token, string _bodyJson) {
            WriteLog.LogColor($"Post URL: {_url} SendData: {_bodyJson}", WriteLog.LogType.ServerAPI_Query);
            var qResp = new QueryResp();
            try {
                using (var request = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST)) {
                    if (!string.IsNullOrEmpty(_token))
                        request.SetRequestHeader("Authorization", $"Bearer {_token}");

                    request.SetRequestHeader("Content-Type", "application/json");
                    request.SetRequestHeader("Accept", "application/json");

                    //request.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                    //request.SetRequestHeader("Pragma", "no-cache");
                    //request.SetRequestHeader("If-Modified-Since", DateTime.UtcNow.ToString("R"));

                    request.timeout = TIMEOUT_SECS;

                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_bodyJson));
                    request.downloadHandler = new DownloadHandlerBuffer();

                    await request.SendWebRequest();

                    string responseText = request.downloadHandler?.text ?? string.Empty;
                    long httpCode = request.responseCode;

                    // 成功
                    if (request.result == UnityWebRequest.Result.Success) {
                        WriteLog.LogColor($"Url: {_url} Response Code: {httpCode}  Content: {responseText}", WriteLog.LogType.ServerAPI_Resp);
                        qResp.Content = responseText;
                        qResp.ErrorCode = null;
                        qResp.Result = true;
                        return qResp;
                    }

                    // 失敗：是否 timeout
                    string unityErr = request.error ?? string.Empty;
                    string unityErrLower = unityErr.ToLowerInvariant();
                    bool isTimeout = request.result == UnityWebRequest.Result.ConnectionError &&
                                     (unityErrLower.Contains("timeout") || unityErrLower.Contains("timed out"));
                    if (isTimeout) {
                        WriteLog.LogError($"HTTP {httpCode}，UnityErr: {unityErr}（判定為 timeout）");
                        qResp.Content = null;
                        qResp.ErrorCode = TIMEOUT_ERROR;
                        qResp.Result = false;
                        return qResp;
                    }
                    string apiErrorCode = tryParseErrorCode(responseText);
                    WriteLog.LogError($"HTTP {httpCode}，UnityErr: {unityErr}，API ErrorCode: {apiErrorCode}");
                    qResp.Content = responseText;
                    qResp.ErrorCode = apiErrorCode;
                    qResp.Result = false;
                    return qResp;
                }
            } catch (Exception ex) {
                string apiErrorCode = tryParseErrorCode(extractJsonFromText(ex.Message ?? string.Empty));
                string msgLower = ex.Message?.ToLowerInvariant() ?? string.Empty;
                bool isTimeoutEx = msgLower.Contains("timeout") || msgLower.Contains("timed out");
                if (isTimeoutEx) {
                    WriteLog.LogError($"Poster Post url:{_url} 例外(timeout)：{ex.Message}");
                    return new QueryResp { Content = null, ErrorCode = TIMEOUT_ERROR, Result = false };
                }

                WriteLog.LogError($"Poster Post url:{_url} 例外: {ex.Message}，API ErrorCode: {apiErrorCode}");
                return new QueryResp { Content = null, ErrorCode = apiErrorCode, Result = false };
            }
        }

        /// <summary>
        /// 執行 HTTP GET，回傳 (responseContent, errorCode, isSuccess)
        /// </summary>
        public static async UniTask<QueryResp> Get(string _url, string _token) {
            WriteLog.LogColorFormat($"Get URL: {_url}", WriteLog.LogType.ServerAPI_Query);
            var qResp = new QueryResp();
            try {
                using (var request = UnityWebRequest.Get(_url)) {
                    if (!string.IsNullOrEmpty(_token))
                        request.SetRequestHeader("Authorization", $"Bearer {_token}");

                    request.SetRequestHeader("Accept", "application/json");

                    request.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                    request.SetRequestHeader("Pragma", "no-cache");
                    request.SetRequestHeader("If-Modified-Since", DateTime.UtcNow.ToString("R"));

                    request.timeout = TIMEOUT_SECS;

                    request.downloadHandler ??= new DownloadHandlerBuffer();

                    await request.SendWebRequest();

                    string responseText = request.downloadHandler?.text ?? string.Empty;
                    long httpCode = request.responseCode;

                    // 成功
                    if (request.result == UnityWebRequest.Result.Success) {
                        WriteLog.LogColor($"Url: {_url} Response Code: {httpCode}  Content: {responseText}", WriteLog.LogType.ServerAPI_Resp);
                        qResp.Content = responseText;
                        qResp.ErrorCode = null;
                        qResp.Result = true;
                        return qResp;
                    }

                    // 失敗：是否 timeout
                    string unityErr = request.error ?? string.Empty;
                    string unityErrLower = unityErr.ToLowerInvariant();
                    bool isTimeout = request.result == UnityWebRequest.Result.ConnectionError &&
                                     (unityErrLower.Contains("timeout") || unityErrLower.Contains("timed out"));
                    if (isTimeout) {
                        WriteLog.LogError($"HTTP {httpCode}，UnityErr: {unityErr}（判定為 timeout）");
                        qResp.Content = null;
                        qResp.ErrorCode = TIMEOUT_ERROR;
                        qResp.Result = false;
                        return qResp;
                    }

                    string apiErrorCode = tryParseErrorCode(responseText);
                    WriteLog.LogError($"HTTP {httpCode}，UnityErr: {unityErr}，API ErrorCode: {apiErrorCode}");
                    qResp.Content = responseText;
                    qResp.ErrorCode = apiErrorCode;
                    qResp.Result = false;
                    return qResp;
                }
            } catch (Exception ex) {
                string apiErrorCode = tryParseErrorCode(extractJsonFromText(ex.Message ?? string.Empty));

                string msgLower = ex.Message?.ToLowerInvariant() ?? string.Empty;
                bool isTimeoutEx = msgLower.Contains("timeout") || msgLower.Contains("timed out");
                if (isTimeoutEx) {
                    WriteLog.LogError($"Poster Get url:{_url} 例外(timeout)：{ex.Message}");
                    return new QueryResp { Content = null, ErrorCode = TIMEOUT_ERROR, Result = false };
                }

                WriteLog.LogError($"Poster Get url:{_url} 例外: {ex.Message}，API ErrorCode: {apiErrorCode}");
                return new QueryResp { Content = null, ErrorCode = apiErrorCode, Result = false };
            }
        }

    }
}
