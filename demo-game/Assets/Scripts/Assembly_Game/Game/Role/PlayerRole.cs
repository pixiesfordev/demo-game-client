using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Threading;

[RequireComponent(typeof(CharacterController))]
public class PlayerRole : Role {
    [Header("模型")]
    public Transform modelRoot;

    [Header("預設 Config")]
    [SerializeField]
    private PlayerRoleConfig defaultConfig;

    private Animator animator;
    // 動畫 Hash ID (先設定效能較好，且避免打錯字)
    private readonly int animID_IsMoving = Animator.StringToHash("IsMoving");
    private readonly int animID_DoHurt = Animator.StringToHash("DoHurt");
    private readonly int animID_DoSkill = Animator.StringToHash("DoSkill");
    private readonly int animID_IsDead = Animator.StringToHash("IsDead");

    public float DashDistance { get; private set; }
    public float DashDuration { get; private set; }
    public int MaxStamina { get; private set; }
    public float StaminaRecoverTime { get; private set; }
    public int CurrentStamina { get; private set; }

    private bool isDashing;
    private bool isInvincible;

    private CharacterController controller;
    private Transform camTransform;
    private Transform lockTarget;
    private Vector3 inputVector;
    private Vector3 moveDirection;

    public override void Initialize(RoleConfig config = null) {
        RoleConfig finalConfig = config ?? defaultConfig;
        if (finalConfig == null) {
            finalConfig = new PlayerRoleConfig();
            Debug.LogWarning("[PlayerRole] 使用全新預設參數");
        }

        base.Initialize(finalConfig);

        if (finalConfig is PlayerRoleConfig pConfig) {
            DashDistance = pConfig.DashDistance;
            DashDuration = pConfig.DashDuration;
            MaxStamina = pConfig.MaxStamina;
            StaminaRecoverTime = pConfig.StaminaRecoverTime;
        } else {
            Debug.LogError("Config 錯誤");
            return;
        }

        controller = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogWarning("找不到 Animator");

        if (Camera.main != null) camTransform = Camera.main.transform;
        if (modelRoot == null) modelRoot = transform;

        CurrentStamina = MaxStamina;
        isDashing = false;
        isInvincible = false;

        RecoverStaminaLoop(lifeCycleCTS.Token).Forget();
    }

    void Start() {
        if (controller == null) Initialize(null);
    }

    void Update() {
        if (IsDead) return;

        HandleInput();
        HandleMove();
        HandleRotate();

        // 更新動畫狀態
        UpdateAnimationState();
    }

    protected override bool IsInvincible() => isInvincible;


    /// <summary>
    /// 受傷
    /// </summary>
    public override void TakeDamage(float dmg) {
        if (IsDead || IsInvincible()) return;

        base.TakeDamage(dmg);

        // 如果還活著，就播受傷動畫
        if (!IsDead && animator != null) {
            animator.SetTrigger(animID_DoHurt);
        }
    }

    /// <summary>
    /// 死亡
    /// </summary>
    protected override void Die() {
        base.Die();
        if (animator != null) {
            animator.SetBool(animID_IsDead, true);
        }
    }

    #region Input
    void HandleInput() {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        inputVector = new Vector3(h, 0, v).normalized;

        if (Input.GetKeyDown(KeyCode.Space)) TryDash();

        // 測試用按下 K 鍵觸發技能
        if (Input.GetKeyDown(KeyCode.K)) PlaySkillAnim();
    }
    #endregion

    #region Animation Helper
    void UpdateAnimationState() {
        if (animator == null) return;

        // 判斷是否在移動
        bool isMoving = inputVector.sqrMagnitude > 0 && !isDashing;

        // 設定動畫
        animator.SetBool(animID_IsMoving, isMoving);
    }

    public void PlaySkillAnim() {
        if (animator != null) animator.SetTrigger(animID_DoSkill);
    }
    #endregion

    #region Move Logic
    void HandleMove() {
        if (isDashing) return;

        if (inputVector.sqrMagnitude > 0) {
            moveDirection = CalculateCameraRelativeInput(inputVector);
            controller.Move(moveDirection * MoveSpeed * Time.deltaTime);
        } else {
            moveDirection = Vector3.zero;
        }
    }

    Vector3 CalculateCameraRelativeInput(Vector3 input) {
        if (camTransform == null) return input;
        Vector3 camFwd = camTransform.forward;
        Vector3 camRight = camTransform.right;
        camFwd.y = 0;
        camRight.y = 0;
        camFwd.Normalize();
        camRight.Normalize();
        return (camFwd * input.z + camRight * input.x).normalized;
    }
    #endregion

    #region Rotate Logic
    void HandleRotate() {
        Vector3 lookDir = Vector3.zero;
        if (lockTarget != null) {
            lookDir = lockTarget.position - transform.position;
        } else if (inputVector.sqrMagnitude > 0) {
            lookDir = moveDirection;
        }
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f) {
            modelRoot.rotation = Quaternion.LookRotation(lookDir);
        }
    }
    #endregion

    #region Dash System
    void TryDash() {
        if (CurrentStamina < 1 || isDashing) return;
        CurrentStamina--;
        Vector3 dashDir = (inputVector.sqrMagnitude > 0) ? moveDirection : modelRoot.forward;
        DashAsync(dashDir).Forget();
    }

    async UniTaskVoid DashAsync(Vector3 dir) {
        isDashing = true;
        isInvincible = true;
        controller.enabled = false;

        Vector3 targetPos = transform.position + dir.normalized * DashDistance;
        try {
            await transform.DOMove(targetPos, DashDuration)
                .SetEase(Ease.Linear)
                .ToUniTask(cancellationToken: lifeCycleCTS.Token);
        } catch (System.OperationCanceledException) { } finally {
            if (controller != null) controller.enabled = true;
            isInvincible = false;
            isDashing = false;
        }
    }
    #endregion

    #region Stamina
    private async UniTaskVoid RecoverStaminaLoop(CancellationToken token) {
        while (!token.IsCancellationRequested && !IsDead) {
            await UniTask.Delay((int)(StaminaRecoverTime * 1000), cancellationToken: token);
            if (CurrentStamina < MaxStamina) CurrentStamina++;
        }
    }
    public void SetLockTarget(Transform target) {
        lockTarget = target;
    }
    #endregion
}