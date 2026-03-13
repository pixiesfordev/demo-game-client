using UnityEngine;
using UnityEngine.UI;

public class SoulVerdictUI : MonoBehaviour
{
    //TODO:
    //V 1.物件化通用的能力ICON物件 >> 圖案+文字+升級能力文字(綠色) (2026.3.11已完成)
    //2.初步宣告可能有演出的物件
    //3.列出缺少的物件給美術去補齊素材

    [SerializeField] Image LeftCloud;
    [SerializeField] Image LeftWord1;
    [SerializeField] Image LeftWord2;
    [SerializeField] Image PartnerIcon; //未來應該會變成物件 等有具體規劃再修改
    [SerializeField] AbilityItemObject[] AbilityObjs;
    [SerializeField] Text LeftInfoText;

    [SerializeField] Image RightCloud;
    [SerializeField] Image RightWord1;
    [SerializeField] Image RightWord2;
    [SerializeField] Image RightItemIcon; //未來應該會變成物件 等有具體規劃再修改
    [SerializeField] Text RightInfoText;

}
