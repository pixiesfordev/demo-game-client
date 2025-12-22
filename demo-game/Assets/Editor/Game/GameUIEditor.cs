using UnityEngine;
using UnityEditor;
using tower.main;

[CustomEditor(typeof(GameUI))]
public class GameUIEditor : Editor {
    public override void OnInspectorGUI() {

        GameUI script = (GameUI)target;

        GUILayout.Space(10);
        GUILayout.Label("=== 切換UI排版工具 ===", EditorStyles.boldLabel);
        if (GUILayout.Button("下注")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Bet);
        }
        if (GUILayout.Button("下注-輸入錯誤")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Bet_HasError);
        }
        if (GUILayout.Button("下注-本地試玩")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Bet_FunPlay);
        }
        if (GUILayout.Button("遊玩中")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Playing);
        }
        if (GUILayout.Button("自動遊玩中")) {
            script.SwtichControlPanel(GameUI.ControlUIState.AutoPlaying);
        }
        if (GUILayout.Button("自動遊玩中-進階設定開啟")) {
            script.SwtichControlPanel(GameUI.ControlUIState.AutoPlaying_SetIsOn);
        }
        if (GUILayout.Button("遊戲結束")) {
            script.SwtichControlPanel(GameUI.ControlUIState.End);
        }
        if (GUILayout.Button("自動模式")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Auto);
        }
        if (GUILayout.Button("自動模式-進階設定開啟")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Auto_SetIsOn);
        }
        if (GUILayout.Button("自動模式-下注小鍵盤開啟")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Auto_Keyboard_Bet);
        }
        if (GUILayout.Button("自動模式-次數小鍵盤開啟")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Auto_Keyboard_Round);
        }
        if (GUILayout.Button("自動模式-輸入錯誤")) {
            script.SwtichControlPanel(GameUI.ControlUIState.Auto_HasError);
        }

        DrawDefaultInspector();
    }

}
