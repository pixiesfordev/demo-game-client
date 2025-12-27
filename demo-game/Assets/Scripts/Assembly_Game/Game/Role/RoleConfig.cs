using UnityEngine;

public abstract class RoleConfig : ScriptableObject {
    public int ID = 0;

    [Header("生命")]
    public float MaxHP = 100f;
    public float HpRegenInterval = 5f;
    public float HpRegenAmount = 1f;

    [Header("移動")]
    public float MoveSpeed = 5f;       // 移動速度
}
