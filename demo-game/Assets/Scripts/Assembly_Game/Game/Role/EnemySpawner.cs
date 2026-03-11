using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    [Header("生成設定")]
    [SerializeField]
    private GameObject enemyPrefab;          // 敵人 Prefab

    [SerializeField]
    private EnemyRoleConfig enemyConfig;     // 敵人 Config

    [SerializeField]
    private int spawnCount = 5;              // 生成數量

    [SerializeField]
    private float spawnRadius = 10f;         // 生成範圍半徑

    [SerializeField]
    private bool spawnOnStart = true;        // 啟動時自動生成

    [SerializeField]
    private Transform spawnCenter;           // 生成中心點（不設定則使用此物件位置）

    [Header("高度檢測")]
    [SerializeField]
    private bool useRaycast = true;          // 是否使用射線檢測地面高度

    [SerializeField]
    private float raycastHeight = 50f;       // 射線起始高度

    [SerializeField]
    private LayerMask groundLayer = -1;      // 地面層級

    private void Start() {
        if (spawnOnStart) {
            SpawnEnemies();
        }
    }

    /// <summary>
    /// 生成敵人
    /// </summary>
    public void SpawnEnemies() {
        if (enemyPrefab == null) {
            Debug.LogError("[EnemySpawner] 未設定 Enemy Prefab！");
            return;
        }

        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;

        for (int i = 0; i < spawnCount; i++) {
            Vector3 spawnPos = GetRandomSpawnPosition(center);
            SpawnEnemy(spawnPos);
        }

        Debug.Log($"[EnemySpawner] 已生成 {spawnCount} 個敵人");
    }

    /// <summary>
    /// 在指定位置生成單個敵人
    /// </summary>
    private void SpawnEnemy(Vector3 position) {
        GameObject enemyObj = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemyObj.transform.SetParent(transform);

        EnemyRole enemy = enemyObj.GetComponent<EnemyRole>();
        if (enemy != null) {
            enemy.Initialize(enemyConfig);
        } else {
            Debug.LogWarning("[EnemySpawner] Prefab 上找不到 EnemyRole 組件");
        }
    }

    /// <summary>
    /// 取得隨機生成位置
    /// </summary>
    private Vector3 GetRandomSpawnPosition(Vector3 center) {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPos = center + new Vector3(randomCircle.x, 0, randomCircle.y);

        // 使用射線檢測地面高度
        if (useRaycast) {
            Vector3 rayStart = new Vector3(randomPos.x, center.y + raycastHeight, randomPos.z);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastHeight * 2, groundLayer)) {
                randomPos.y = hit.point.y;
            } else {
                randomPos.y = center.y;
            }
        } else {
            randomPos.y = center.y;
        }

        return randomPos;
    }

    /// <summary>
    /// 清除所有已生成的敵人
    /// </summary>
    public void ClearEnemies() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        Debug.Log("[EnemySpawner] 已清除所有敵人");
    }

    #region Gizmos（編輯器可視化）
    private void OnDrawGizmosSelected() {
        Vector3 center = spawnCenter != null ? spawnCenter.position : transform.position;

        // 生成範圍
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, spawnRadius);

        // 中心點標記
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(center, 0.5f);
    }
    #endregion
}