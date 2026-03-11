using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

[RequireComponent(typeof(CharacterController))]
public class EnemyRole : Role {
    [Header("模型")]
    public Transform modelRoot;

    [Header("預設 Config")]
    [SerializeField]
    private EnemyRoleConfig defaultConfig;

    [Header("調試")]
    [SerializeField]
    private bool showDebugLog = false;

    private Animator animator;
    private readonly int animID_IsMoving = Animator.StringToHash("IsMoving");
    private readonly int animID_DoHurt = Animator.StringToHash("DoHurt");
    private readonly int animID_IsDead = Animator.StringToHash("IsDead");

    public float WanderRadius { get; private set; }
    public float WanderInterval { get; private set; }

    private EnemyState currentState;
    private CharacterController controller;

    private Vector3 spawnPosition;
    private Vector3 wanderTarget;
    private float idleEndTime;
    private Vector3 lastPosition;

    public enum EnemyState {
        Idle,
        Walk,
        Dead
    }

    public override void Initialize(RoleConfig config = null) {
        RoleConfig finalConfig = config ?? defaultConfig;
        if (finalConfig == null) {
            finalConfig = new EnemyRoleConfig();
            Debug.LogWarning("[EnemyRole] 使用全新預設參數");
        }

        base.Initialize(finalConfig);

        if (finalConfig is EnemyRoleConfig eConfig) {
            WanderRadius = eConfig.WanderRadius;
            WanderInterval = eConfig.WanderInterval;
        } else {
            Debug.LogError("Config 錯誤");
            return;
        }

        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (animator == null) {
            Debug.LogWarning($"[EnemyRole] 找不到 Animator - {gameObject.name}");
        } else if (showDebugLog) {
            Debug.Log($"[EnemyRole] Animator 初始化成功 - {gameObject.name}");
        }

        if (modelRoot == null) modelRoot = transform;

        spawnPosition = transform.position;
        currentState = EnemyState.Idle;
        idleEndTime = Time.time + WanderInterval;
        lastPosition = transform.position;
    }

    void Start() {
        if (controller == null) Initialize(null);
        RunAI(lifeCycleCTS.Token).Forget();
    }

    public override void TakeDamage(float dmg) {
        if (IsDead) return;

        base.TakeDamage(dmg);

        if (!IsDead && animator != null) {
            animator.SetTrigger(animID_DoHurt);
        }
    }

    protected override void Die() {
        base.Die();
        currentState = EnemyState.Dead;

        if (animator != null) {
            animator.SetBool(animID_IsDead, true);
            animator.SetBool(animID_IsMoving, false);
        }
    }

    #region AI 狀態機
    private async UniTaskVoid RunAI(CancellationToken token) {
        while (!token.IsCancellationRequested && !IsDead) {
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            switch (currentState) {
                case EnemyState.Idle:
                    StateIdle();
                    break;
                case EnemyState.Walk:
                    StateWalk();
                    break;
            }

            UpdateAnimation();
        }
    }

    private void StateIdle() {
        if (Time.time < idleEndTime) return;

        SetNewWanderTarget();
        currentState = EnemyState.Walk;

        if (showDebugLog) {
            Debug.Log($"[EnemyRole] Idle -> Walk - {gameObject.name}");
        }
    }

    private void StateWalk() {
        Vector3 direction = wanderTarget - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.04f) {
            currentState = EnemyState.Idle;
            idleEndTime = Time.time + WanderInterval;

            if (showDebugLog) {
                Debug.Log($"[EnemyRole] Walk -> Idle - {gameObject.name}");
            }

            return;
        }

        Vector3 moveDirection = direction.normalized;
        controller.Move(moveDirection * MoveSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f) {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            modelRoot.rotation = Quaternion.Lerp(modelRoot.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }

    private void SetNewWanderTarget() {
        Vector2 randomCircle = Random.insideUnitCircle * WanderRadius;
        wanderTarget = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (showDebugLog) {
            Debug.Log($"[EnemyRole] 新目標點: {wanderTarget} - {gameObject.name}");
        }
    }

    private void UpdateAnimation() {
        if (animator == null) return;

        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;

        bool isMoving = delta.sqrMagnitude > 0.000001f;
        animator.SetBool(animID_IsMoving, isMoving);

        lastPosition = transform.position;
    }
    #endregion

    public EnemyState GetCurrentState() {
        return currentState;
    }

    #region Gizmos
    private void OnDrawGizmosSelected() {
        Vector3 center = Application.isPlaying ? spawnPosition : transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(center, WanderRadius);

        if (Application.isPlaying && currentState == EnemyState.Walk) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawSphere(wanderTarget, 0.3f);
        }
    }
    #endregion
}