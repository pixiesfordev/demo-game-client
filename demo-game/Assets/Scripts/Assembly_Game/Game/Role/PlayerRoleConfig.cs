using UnityEngine;

[CreateAssetMenu(menuName = "Game/RoleConfig/Player")]
public class PlayerRoleConfig : RoleConfig {
    [Header("衝刺")]
    public float DashDistance = 6f;    // 衝刺距離
    public float DashDuration = 0.2f;  // 衝刺耗時
    public int MaxStamina = 2;         // 氣力上限
    public float StaminaRecoverTime = 2f; // 氣力回復時間
}
