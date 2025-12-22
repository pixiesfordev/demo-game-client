using Scoz.Func;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GlobalClickBlocker : MonoBehaviour, ICanvasRaycastFilter {
    [SerializeField] string Name;
    [SerializeField] Image Img_Unblock;
    [SerializeField] bool IsActive;
    RectTransform rt;


    static Dictionary<string, GlobalClickBlocker> blockers = new Dictionary<string, GlobalClickBlocker>();

    private void Awake() {
        Img_Unblock.color = new Color(1, 1, 1, 0);
        rt = Img_Unblock.GetComponent<RectTransform>();
        addToBlockerDic(Name, this);
        IsActive = false;
    }

    static void addToBlockerDic(string _name, GlobalClickBlocker _blocker) {
        if (blockers.ContainsKey(_name)) {
            WriteLog.LogErrorFormat("GlobalClickBlocker 加入重複 key");
            return;
        }
        blockers.Add(_name, _blocker);
    }

    public static void ActiveBlocker(string _name) {
        foreach (var blocker in blockers.Values) {
            //if (blocker.Name == _name) WriteLog.LogError("_name=" + _name);
            blocker.ActiveBlocker(blocker.Name == _name);
        }
    }
    public static void InActiveAllBlockers() {
        foreach (var blocker in blockers.Values) blocker.ActiveBlocker(false);
    }

    public void ActiveBlocker(bool _active) {
        IsActive = _active;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera) {
        if (!IsActive)
            return false; // 沒啟用不擋

        // 如果點在允許的區域不擋(讓事件往下傳)
        if (rt && RectTransformUtility.RectangleContainsScreenPoint(rt, sp, eventCamera)) {
            return false;
        }

        // 其餘地方擋掉
        return true;
    }
}
