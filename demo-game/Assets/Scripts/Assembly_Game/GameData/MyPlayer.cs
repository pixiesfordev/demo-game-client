using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;
using tower.main;

namespace Scoz.Func {
    public abstract class MyPlayer {
        public static bool IsInit = false;
        public static MyPlayer Instance {
            get {
                return GamePlayer.Instance;
            }
        }
        public Language UsingLanguage { get; protected set; } = Language.VN;//語系
        public string GameToken { get; private set; } // 遊戲驗證用Token
        public Dictionary<string, long> UIDDic = new Dictionary<string, long>();//UID管理器 單機遊戲需要控管玩家擁有物件的UID不重複使用

        public bool UseLocoToken {
            get {
#if UNITY_EDITOR && Dev
                return true;
#endif
                return false;
            }
        }


        /// <summary>
        /// 取的本機Setting資料
        /// </summary>
        public virtual void LoadLocoData() {
            LoadSettingFromLoco();
        }
        public void LoadSettingFromLoco() {

            string json = LocoDataManager.GetDataFromLoco(LocoDataName.PlayerSetting);
            if (!string.IsNullOrEmpty(json)) {
                JSONNode jsNode = JSON.Parse(json);
                //UsingLanguage = (Language)jsNode["UseLanguage"].AsInt;
                AudioPlayer.SetSoundVolume(jsNode["SoundVolume"].AsFloat);
                AudioPlayer.SetMusicVolume(jsNode["MusicVolume"].AsFloat);
                AudioPlayer.SetVoiceVolume(jsNode["VoiceVolume"].AsFloat);
                if (UseLocoToken) GameToken = jsNode["GameToken"];
                UIDDic.Clear();
                foreach (var key in jsNode["UIDDic"].Keys) {
                    UIDDic[key] = jsNode["UIDDic"][key].AsLong;
                }

            } else {
                //SetLanguage((Language)JsonGameSetting.GetInt(GameSetting.DefaultLanguage));
                AudioPlayer.SetSoundVolume(JsonGameSetting.GetFloat(GameSetting.DefaultSound));
                AudioPlayer.SetMusicVolume(JsonGameSetting.GetFloat(GameSetting.DefaultMusic));
                AudioPlayer.SetVoiceVolume(JsonGameSetting.GetFloat(GameSetting.DefaultVoice));
                GameToken = "";
                UIDDic.Clear();
            }
        }
        public void SaveSettingToLoco() {
            JSONObject jsObj = new JSONObject();
            //jsObj.Add("UseLanguage", (int)UsingLanguage);
            jsObj.Add("SoundVolume", AudioPlayer.SoundVolumeRatio);
            jsObj.Add("MusicVolume", AudioPlayer.MusicVolumeRatio);
            jsObj.Add("VoiceVolume", AudioPlayer.VoiceVolumeRatio);
            if (UseLocoToken) jsObj.Add("GameToken", GameToken);
            JSONObject uidObj = new JSONObject();
            foreach (string key in UIDDic.Keys) uidObj.Add(key, UIDDic[key]);
            jsObj.Add("UIDDic", uidObj);
            LocoDataManager.SaveDataToLoco(LocoDataName.PlayerSetting, jsObj.ToString());
        }

        /// <summary>
        /// 取得新的UID
        /// </summary>
        public string GetNextUID(string _key) {
            if (UIDDic.ContainsKey(_key)) return (++UIDDic[_key]).ToString();
            UIDDic.Add(_key, 0);
            return UIDDic[_key].ToString();
        }

        public void SetLanguage(Language _value) {
            if (_value != UsingLanguage) {
                UsingLanguage = _value;
            }
            WriteLog.Log($"設定語系為: {UsingLanguage}");
        }
        public void SetGameToken(string _token) {
            GameToken = _token;
        }
    }
}