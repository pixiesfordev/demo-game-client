using UnityEngine;
using UnityEngine.UI;

public class AbilityItemObject : MonoBehaviour
{
    [SerializeField] Image Icon;
    [SerializeField] Text CurrentVal;
    [SerializeField] Text AddVal;

    [SerializeField] string IconPath;
    [SerializeField] string IconName;

    [SerializeField] float TextSpacing = 15f; //目前值與增加值字距

    [SerializeField] bool UpdateValRT = false;
    //TODO:
    //1.設定Icon功能 >> 等架構確立如何讀取路徑後添加實際功能 根據設定路徑去讀取圖片

    void Start () {
        
    }

    void Update () {
        if (UpdateValRT) {
            UpdateValRT = false;
            AdjustAddValRT();
        }
    }

    void OnEnable() {
        AdjustAddValRT();
    }

    /// <summary>
    /// 增加值自動適配目前值位置
    /// </summary>
    void AdjustAddValRT() {
        Vector3 currentValPos = CurrentVal.rectTransform.localPosition;
        float currentValWidth = CurrentVal.preferredWidth;
        float addValPosX = currentValPos.x + currentValWidth + TextSpacing;
        AddVal.rectTransform.localPosition = new Vector3(addValPosX, currentValPos.y, currentValPos.z);
    }

    public void SetInfo(int current, int add) {
        //TODO:
        //1.設定圖片
        //設定Icon
        /*
        AssetGet.GetSpriteFromAtlas("Relics skillicon", SkillData.ID.ToString(), (sprite) => {
            if (sprite != null)
                RelicSkill.sprite = sprite;
            else {
                AssetGet.GetSpriteFromAtlas("Relics skillicon", "Relics_Click_10110", (sprite) => { 
                RelicSkill.sprite = sprite; 
                WriteLog.LogWarningFormat("圖片缺少! 用Relics_Click_10110代替顯示! ID: {0}", SkillData.Ref);
                } );
            }
        });
        */
        //設定目前值
        CurrentVal.text = current.ToString();
        //設定增加值
        AddVal.text = add.ToString();
        AdjustAddValRT();
    }
}
