using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 迷路の敵配置を管理するシステム
/// 複数の敵を簡単に配置・管理できる
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("敵設定")]
    [SerializeField] private GameObject enemyPrefab; // 敵のプレハブ
    [SerializeField] private Transform[] spawnPoints; // 敵の配置座標
    [SerializeField] private bool autoSpawn = true; // 自動配置
    
    [Header("簡単座標設定")]
    [SerializeField] private bool useSimpleCoordinates = true; // 簡単座標使用
    [SerializeField] private Vector3[] simpleCoordinates = new Vector3[]
    {
        new Vector3(5, 1.5f, 5),
        new Vector3(-5, 1.5f, 5),
        new Vector3(8, 1.5f, -3),
        new Vector3(-8, 1.5f, -3),
        new Vector3(0, 1.5f, 10),
        new Vector3(3, 1.5f, -8),
        new Vector3(-7, 1.5f, 8),
        new Vector3(12, 1.5f, 0)
    }; // 敵配置座標
    
    [Header("敵の種類設定")]
    [SerializeField] private EnemyData[] enemyTypes; // 複数の敵データ
    
    [Header("配置設定")]
    [SerializeField] private int maxEnemies = 10; // 最大敵数
    [SerializeField] private float minDistanceBetweenEnemies = 3f; // 敵同士の最小距離
    
    [Header("デバッグ")]
    [SerializeField] private bool showGizmos = true; // ギズモ表示
    [SerializeField] private Color gizmoColor = Color.red; // ギズモ色
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    [System.Serializable]
    public class EnemyData
    {
        public string name = "Enemy"; // 敵の名前
        public GameObject prefab; // 敵のプレハブ
        public Sprite icon; // 敵のアイコン（将来的にUI表示用）
        public float spawnChance = 1f; // 出現確率（0-1）
        public enum EnemyDifficulty { Easy, Normal, Hard }
        public EnemyDifficulty difficulty = EnemyDifficulty.Normal;
    }
    
    private void Start()
    {
        // 簡単座標セットアップ
        if (useSimpleCoordinates && simpleCoordinates.Length > 0)
        {
            SetupSimpleCoordinates();
        }
        
        if (autoSpawn)
        {
            SpawnAllEnemies();
        }
    }
    
    /// <summary>
    /// 簡単座標からSpawnPointsを自動生成
    /// </summary>
    private void SetupSimpleCoordinates()
    {
        spawnPoints = new Transform[simpleCoordinates.Length];
        
        for (int i = 0; i < simpleCoordinates.Length; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i:D2}");
            spawnPoint.transform.position = simpleCoordinates[i];
            spawnPoint.transform.SetParent(transform);
            spawnPoints[i] = spawnPoint.transform;
        }
        
        LogDebug($"簡単座標から {simpleCoordinates.Length} 個の配置点を自動生成");
    }
    
    /// <summary>
    /// すべての敵を配置する
    /// </summary>
    public void SpawnAllEnemies()
    {
        ClearEnemies();
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[敵配置] 配置座標が設定されていません");
            return;
        }
        
        int enemiesSpawned = 0;
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (enemiesSpawned >= maxEnemies) break;
            
            if (spawnPoint == null) continue;
            
            // 距離チェック
            if (IsValidSpawnPosition(spawnPoint.position))
            {
                GameObject enemy = SpawnEnemyAtPosition(spawnPoint.position);
                if (enemy != null)
                {
                    spawnedEnemies.Add(enemy);
                    enemiesSpawned++;
                }
            }
        }
        
        Debug.Log($"[敵配置] {enemiesSpawned}体の敵を配置しました");
    }
    
    /// <summary>
    /// 指定位置に敵をスポーンする
    /// </summary>
    /// <param name="position">配置位置</param>
    /// <returns>作成された敵オブジェクト</returns>
    private GameObject SpawnEnemyAtPosition(Vector3 position)
    {
        GameObject prefabToUse = GetRandomEnemyPrefab();
        
        if (prefabToUse == null)
        {
            Debug.LogWarning("[敵配置] 敵のプレハブが設定されていません");
            return null;
        }
        
        GameObject enemy = Instantiate(prefabToUse, position, Quaternion.identity);
        
        // 敵に名前を設定
        string enemyName = GetEnemyName(prefabToUse);
        enemy.name = $"{enemyName}_{spawnedEnemies.Count + 1:D2}";
        
        // EnemyEventコンポーネントの確認
        if (enemy.GetComponent<EnemyEvent>() == null)
        {
            enemy.AddComponent<EnemyEvent>();
        }
        
        // コライダーの確認
        if (enemy.GetComponent<Collider>() == null)
        {
            enemy.AddComponent<CapsuleCollider>();
            enemy.GetComponent<Collider>().isTrigger = true;
        }
        
        Debug.Log($"[敵配置] {enemy.name} を {position} に配置");
        return enemy;
    }
    
    /// <summary>
    /// ランダムな敵プレハブを取得
    /// </summary>
    private GameObject GetRandomEnemyPrefab()
    {
        if (enemyTypes != null && enemyTypes.Length > 0)
        {
            // 確率に基づいて選択
            List<EnemyData> validEnemies = new List<EnemyData>();
            foreach (EnemyData enemyData in enemyTypes)
            {
                if (enemyData.prefab != null && Random.value <= enemyData.spawnChance)
                {
                    validEnemies.Add(enemyData);
                }
            }
            
            if (validEnemies.Count > 0)
            {
                EnemyData selectedEnemy = validEnemies[Random.Range(0, validEnemies.Count)];
                return selectedEnemy.prefab;
            }
        }
        
        return enemyPrefab; // フォールバック
    }
    
    /// <summary>
    /// 敵の名前を取得
    /// </summary>
    private string GetEnemyName(GameObject prefab)
    {
        if (enemyTypes != null)
        {
            foreach (EnemyData enemyData in enemyTypes)
            {
                if (enemyData.prefab == prefab)
                {
                    return enemyData.name;
                }
            }
        }
        
        return prefab.name;
    }
    
    /// <summary>
    /// 配置位置が有効かチェック
    /// </summary>
    /// <param name="position">チェックする位置</param>
    /// <returns>有効ならtrue</returns>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // 他の敵との距離をチェック
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < minDistanceBetweenEnemies)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// すべての敵を削除
    /// </summary>
    public void ClearEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                DestroyImmediate(enemy);
            }
        }
        spawnedEnemies.Clear();
        
        Debug.Log("[敵配置] すべての敵を削除しました");
    }
    
    /// <summary>
    /// 特定の位置に手動で敵を追加
    /// </summary>
    /// <param name="position">追加位置</param>
    public void AddEnemyAt(Vector3 position)
    {
        if (spawnedEnemies.Count >= maxEnemies)
        {
            Debug.LogWarning("[敵配置] 敵の最大数に達しています");
            return;
        }
        
        GameObject enemy = SpawnEnemyAtPosition(position);
        if (enemy != null)
        {
            spawnedEnemies.Add(enemy);
        }
    }
    
    /// <summary>
    /// ギズモ描画（エディタ用）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || spawnPoints == null) return;
        
        Gizmos.color = gizmoColor;
        
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawWireCube(spawnPoint.position + Vector3.up * 1f, Vector3.one);
            }
        }
    }
    
    /// <summary>
    /// 配置座標を配列で設定（スクリプトから）
    /// </summary>
    /// <param name="positions">座標配列</param>
    public void SetSpawnPositions(Vector3[] positions)
    {
        spawnPoints = new Transform[positions.Length];
        
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject spawnPointObj = new GameObject($"SpawnPoint_{i:D2}");
            spawnPointObj.transform.position = positions[i];
            spawnPointObj.transform.SetParent(transform);
            spawnPoints[i] = spawnPointObj.transform;
        }
        
        Debug.Log($"[敵配置] {positions.Length}個の配置座標を設定しました");
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    /// <param name="message">ログメッセージ</param>
    private void LogDebug(string message)
    {
        Debug.Log($"[敵配置] {message}");
    }
}