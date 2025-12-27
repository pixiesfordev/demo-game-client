using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

public abstract class Role : MonoBehaviour {

    public int ID { get; private set; }
    public string RoleName { get; private set; }
    public float MaxHP { get; private set; }
    public float HPRegenInterval { get; private set; }
    public float HPRegenAmount { get; private set; }
    public float MoveSpeed { get; private set; }

    public float CurrentHP { get; protected set; }
    public bool IsDead { get; protected set; }

    protected CancellationTokenSource lifeCycleCTS = new CancellationTokenSource();

    public virtual void Initialize(RoleConfig config) {
        ID = config.ID;
        MaxHP = config.MaxHP;
        HPRegenInterval = config.HpRegenInterval;
        HPRegenAmount = config.HpRegenAmount;
        MoveSpeed = config.MoveSpeed;

        CurrentHP = MaxHP;
        IsDead = false;

        RegenHPLoop(lifeCycleCTS.Token).Forget();
    }

    public virtual void TakeDamage(float dmg) {
        if (IsDead) return;
        if (IsInvincible()) return;

        CurrentHP -= dmg;
        if (CurrentHP <= 0) {
            CurrentHP = 0;
            Die();
        }
    }

    protected virtual bool IsInvincible() => false;

    protected virtual void Die() {
        IsDead = true;
        lifeCycleCTS.Cancel();
        Debug.Log($"{RoleName} Dead");
    }

    private async UniTaskVoid RegenHPLoop(CancellationToken token) {
        while (!token.IsCancellationRequested && !IsDead) {
            await UniTask.Delay((int)(HPRegenInterval * 1000), cancellationToken: token);
            if (CurrentHP < MaxHP && CurrentHP > 0) {
                CurrentHP = Mathf.Min(CurrentHP + HPRegenAmount, MaxHP);
            }
        }
    }

    protected virtual void OnDestroy() {
        lifeCycleCTS?.Cancel();
        lifeCycleCTS?.Dispose();
    }
}