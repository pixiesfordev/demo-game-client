using UnityEngine;

[CreateAssetMenu(menuName = "Game/RoleConfig/Enemy")]
public class EnemyRoleConfig : RoleConfig {
    [Header("隨機走動")]
    public float WanderRadius = 10f;    // 隨機走動半徑
    public float WanderInterval = 3f;   // 待機時間（秒）
}