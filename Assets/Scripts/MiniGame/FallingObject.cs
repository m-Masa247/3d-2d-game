using UnityEngine;

public class FallingObject : MonoBehaviour
{
    [Header("落下設定")]
    public float fallSpeed = 5f;
    public float destroyY = -10f;
    
    [Header("表示設定")]
    public float objectScale = 0.3f; // オブジェクトのサイズ（調整可能）
    
    [Header("装備設定")]
    public EquipmentType equipmentType;
    
    private bool isCollected = false;

    void Start()
    {
        InitializeEquipment();
        AdjustScale(); // サイズ自動調整
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeEquipment()
    {
        // ランダムな装備タイプを設定
        EquipmentData equipment = EquipmentManager.GetRandomEquipment();
        equipmentType = equipment.type;
        
        // SpriteRenderer対応（なければMeshRenderer対応）
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = equipment.icon;
            spriteRenderer.color = Color.white;
        }
        else
        {
            // フォールバック：MeshRendererで色表示
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = equipment.color;
                Debug.Log($"[FallingObject] {equipment.name}をMeshRendererで表示中（色：{equipment.color}）");
            }
            else
            {
                Debug.LogError($"[FallingObject] SpriteRendererもMeshRendererも見つかりません！");
            }
        }
    }
    
    public void SetEquipmentType(EquipmentType type)
    {
        equipmentType = type;
        EquipmentData equipment = EquipmentManager.GetEquipmentByType(type);
        
        // SpriteRenderer対応（なければMeshRenderer対応）
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = equipment.icon;
            spriteRenderer.color = Color.white;
        }
        else
        {
            // フォールバック：MeshRendererで色表示
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = equipment.color;
            }
            else
            {
                Debug.LogError($"[FallingObject] SpriteRendererもMeshRendererも見つかりません！");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            
            MiniGamePlayer player = other.GetComponent<MiniGamePlayer>();
            if (player != null)
            {
                player.CollectEquipment(equipmentType);
                
                string equipmentName = EquipmentManager.GetEquipmentByType(equipmentType).name;
                Debug.Log($"[収集] {equipmentName}を取得！");
            }
            else
            {
                Debug.LogError("[FallingObject] プレイヤーコンポーネントが見つかりません！");
            }
            
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// オブジェクトサイズの自動調整
    /// </summary>
    void AdjustScale()
    {
        transform.localScale = Vector3.one * objectScale;
        
        // SpriteRenderer がある場合、表示順序を設定
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1; // 前面表示
        }
    }
}