using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scoz.Func;
using LitJson;
using System.Reflection;

namespace tower.main {
    public class JsonKeyboard : JsonBase {
        public static string DataName { get; set; }

        public int Preset_Bet_CNY { get; set; }
        public int Preset_Bet_USD { get; set; }
        public int Preset_Bet_KVND { get; set; }

        protected override void SetDataFromJson(JsonData _item) {
            JsonData item = _item;
            //反射屬性
            var myData = JsonMapper.ToObject<JsonKeyboard>(item.ToJson());
            foreach (PropertyInfo propertyInfo in this.GetType().GetProperties()) {
                if (propertyInfo.CanRead && propertyInfo.CanWrite) {
                    //下面這行如果報錯誤代表上方的sonMapper.ToObject<XXXXX>(item.ToJson());<---XXXXX忘了改目前Class名稱
                    var value = propertyInfo.GetValue(myData, null);
                    propertyInfo.SetValue(this, value, null);
                }
            }
            //自定義屬性
            //foreach (string key in item.Keys) {
            //    switch (key) {
            //        case "ID":
            //            ID = int.Parse(item[key].ToString());
            //            break;
            //        default:
            //            WriteLog.LogWarning(string.Format("{0}表有不明屬性:{1}", DataName, key));
            //            break;
            //    }
            //}
        }
        protected override void ResetStaticData() {
        }

        public int GetCurrencyPresetBet(Currency _currency) {
            switch (_currency) {
                case Currency.CNY:
                    return Preset_Bet_CNY;
                case Currency.USD:
                    return Preset_Bet_USD;
                case Currency.KVND:
                    return Preset_Bet_KVND;
                default:
                    WriteLog.LogError($"尚未實作的幣值: {_currency}");
                    return Preset_Bet_KVND;
            }
        }
    }

}
