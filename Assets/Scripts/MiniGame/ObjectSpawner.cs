using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    public GameObject fallingObjectPrefab;
    public float spawnInterval = 2f;
    public float spawnRangeX = 8f;
    public float spawnHeight = 10f;
    
    [Header("装備分布設定")]
    [Range(0f, 1f)]
    public float swordSpawnRate = 0.33f;
    [Range(0f, 1f)]
    public float shieldSpawnRate = 0.33f;
    // 杖の出現率は残りの確率になる

    private float timer;
    private bool isSpawning = true;

    void Update()
    {
        if (!isSpawning) return;
        
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (fallingObjectPrefab == null)
        {
            Debug.LogError("[ObjectSpawner] fallingObjectPrefabが設定されていません！");
            return;
        }
        
        // ランダムな位置にスポーン
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, 0);
        
        // オブジェクトを生成
        GameObject spawnedObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);
        
        // 装備タイプをランダムに設定（確率に基づく）
        EquipmentType equipmentType = GetRandomEquipmentType();
        
        FallingObject fallingComponent = spawnedObject.GetComponent<FallingObject>();
        if (fallingComponent != null)
        {
            fallingComponent.SetEquipmentType(equipmentType);
        }
        else
        {
            Debug.LogError($"[ObjectSpawner] FallingObjectコンポーネントが見つかりません！");
        }
    }
    
    EquipmentType GetRandomEquipmentType()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue < swordSpawnRate)
        {
            return EquipmentType.Sword;
        }
        else if (randomValue < swordSpawnRate + shieldSpawnRate)
        {
            return EquipmentType.Shield;
        }
        else
        {
            return EquipmentType.Staff;
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("オブジェクトのスポーンを停止しました");
    }
    
    public void StartSpawning()
    {
        isSpawning = true;
        Debug.Log("オブジェクトのスポーンを再開しました");
    }
}