using UnityEngine;

/// <summary>
/// プレイヤーの見た目を管理するシステム
/// モデル、テクスチャ、アニメーションを変更可能
/// </summary>
public class PlayerAppearanceManager : MonoBehaviour
{
    [Header("プレイヤーモデル設定")]
    [SerializeField] private GameObject defaultPlayerModel; // デフォルトモデル
    [SerializeField] private GameObject[] alternativeModels; // 代替モデル配列
    [SerializeField] private int currentModelIndex = 0; // 現在のモデルインデックス
    
    [Header("テクスチャ設定")]
    [SerializeField] private Material defaultMaterial; // デフォルト素材
    [SerializeField] private Material[] alternativeMaterials; // 代替素材配列
    [SerializeField] private int currentMaterialIndex = 0; // 現在の素材インデックス
    
    [Header("スプライト設定（2D風表現）")]
    [SerializeField] private bool useSpriteMode = false; // スプライトモード
    [SerializeField] private SpriteRenderer playerSpriteRenderer; // スプライト表示用
    [SerializeField] private Sprite[] playerSprites; // プレイヤースプライト配列
    [SerializeField] private int currentSpriteIndex = 0; // 現在のスプライトインデックス
    
    [Header("自動回転設定（スプライトモード用）")]
    [SerializeField] private bool autoRotateToCamera = true; // カメラ方向に自動回転
    [SerializeField] private Vector3 spriteBillboardOffset = Vector3.zero; // ビルボード調整
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = true; // デバッグ情報表示
    
    private GameObject currentActiveModel; // 現在のアクティブモデル
    private Camera mainCamera;
    private Renderer[] currentRenderers; // 現在のレンダラー配列
    
    private void Start()
    {
        // Playerオブジェクトを確実にアクティブ化
        gameObject.SetActive(true);
        
        mainCamera = Camera.main;
        InitializePlayerAppearance();
        
        LogDebug($"PlayerAppearanceManager開始 - Player位置: {transform.position}");
    }
    
    private void Update()
    {
        if (useSpriteMode && autoRotateToCamera && mainCamera != null)
        {
            RotateTowardsCamera();
        }
    }
    
    /// <summary>
    /// プレイヤー外観の初期化
    /// </summary>
    private void InitializePlayerAppearance()
    {
        // 初期位置の確保
        EnsureProperPlayerPosition();
        
        if (useSpriteMode)
        {
            SetupSpriteMode();
        }
        else
        {
            SetupModelMode();
        }
        
        LogDebug("プレイヤー外観を初期化しました");
    }
    
    /// <summary>
    /// プレイヤーの適切な位置を保証
    /// </summary>
    private void EnsureProperPlayerPosition()
    {
        // Y座標が低すぎる場合は修正
        Vector3 position = transform.position;
        if (position.y < 1.0f)
        {
            position.y = 1.5f; // 適切な高さに設定
            transform.position = position;
            LogDebug($"プレイヤー位置を修正: {position}");
        }
        
        // Rigidbodyの確認
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            LogDebug("Rigidbodyが見つからないため追加します");
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Rigidbodyの設定
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.drag = 5f; // 滑りにくくする
        
        // コライダーの確認
        if (GetComponent<Collider>() == null)
        {
            LogDebug("Colliderが見つからないため追加します");
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.5f;
        }
    }
    
    /// <summary>
    /// スプライトモードの設定
    /// </summary>
    private void SetupSpriteMode()
    {
        // 3Dモデルを非表示
        HideAllModels();
        
        // SpriteRendererの設定
        if (playerSpriteRenderer == null)
        {
            GameObject spriteObject = new GameObject("PlayerSprite");
            spriteObject.transform.SetParent(transform);
            spriteObject.transform.localPosition = spriteBillboardOffset;
            playerSpriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        }
        
        // 初期スプライト設定
        if (playerSprites != null && playerSprites.Length > 0)
        {
            playerSpriteRenderer.sprite = playerSprites[currentSpriteIndex];
        }
        
        playerSpriteRenderer.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// モデルモードの設定
    /// </summary>
    private void SetupModelMode()
    {
        // スプライトを非表示
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.gameObject.SetActive(false);
        }
        
        // 3Dモデルの表示
        ActivateModel(currentModelIndex);
        ApplyMaterial(currentMaterialIndex);
    }
    
    /// <summary>
    /// すべてのモデルを非表示
    /// </summary>
    private void HideAllModels()
    {
        // デフォルトモデル（元のPlayerモデル）のみ非表示
        if (defaultPlayerModel != null && !defaultPlayerModel.name.ToLower().Contains("ratcar"))
        {
            // デフォルトPlayerモデルのレンダラーのみを無効化
            Renderer[] renderers = defaultPlayerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null && !renderer.gameObject.name.ToLower().Contains("ratcar"))
                {
                    renderer.enabled = false;
                }
            }
            LogDebug($"デフォルトモデル '{defaultPlayerModel.name}' を非表示");
        }
        
        // 代替モデルはすべて非表示（ratcar以外）
        if (alternativeModels != null)
        {
            foreach (GameObject model in alternativeModels)
            {
                if (model != null && !model.name.ToLower().Contains("ratcar"))
                {
                    model.SetActive(false);
                }
            }
        }
        
        LogDebug("不要なモデルを非表示にしました（ratcarは保持）");
    }
    
    /// <summary>
    /// 指定されたモデルをアクティブ化
    /// </summary>
    /// <param name="modelIndex">モデルインデックス</param>
    private void ActivateModel(int modelIndex)
    {
        // Playerオブジェクト本体を確実にアクティブに
        gameObject.SetActive(true);
        
        HideAllModels();
        
        GameObject targetModel = null;
        
        if (modelIndex == 0)
        {
            targetModel = defaultPlayerModel;
        }
        else if (alternativeModels != null && modelIndex - 1 < alternativeModels.Length)
        {
            targetModel = alternativeModels[modelIndex - 1];
        }
        
        if (targetModel != null)
        {
            targetModel.SetActive(true);
            currentActiveModel = targetModel;
            currentRenderers = targetModel.GetComponentsInChildren<Renderer>();
            
            // ratcarの場合は確実に表示
            if (targetModel.name.ToLower().Contains("ratcar"))
            {
                // ratcarの全レンダラーを強制的に有効化
                foreach (Renderer renderer in currentRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
                LogDebug("ratcarレンダラーを強制有効化");
            }
            else
            {
                // その他のモデルの通常処理
                foreach (Renderer renderer in currentRenderers)
                {
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                    }
                }
            }
            
            // モデル位置とスケールの自動調整
            AdjustModelTransform(targetModel);
            
            LogDebug($"モデル {modelIndex} をアクティブ化: {targetModel.name}");
        }
        
        // Playerオブジェクトの状態を再確認
        LogDebug($"Player状態確認 - アクティブ: {gameObject.activeInHierarchy}, 位置: {transform.position}");
    }
    
    /// <summary>
    /// モデルの位置とスケールを調整
    /// </summary>
    /// <param name="model">調整対象モデル</param>
    private void AdjustModelTransform(GameObject model)
    {
        if (model == null) return;
        
        Transform modelTransform = model.transform;
        
        // ratcarモデルの特別処理
        if (model.name.ToLower().Contains("ratcar"))
        {
            // ratcarを適切なサイズと位置に調整
            modelTransform.localPosition = new Vector3(0, 0.5f, 0); // 少し上に
            modelTransform.localRotation = Quaternion.identity;
            modelTransform.localScale = Vector3.one * 2f; // 2倍サイズ
            
            LogDebug("ratcarモデルの位置・スケールを調整");
        }
        else
        {
            // デフォルトモデルの設定
            modelTransform.localPosition = Vector3.zero;
            modelTransform.localRotation = Quaternion.identity;
            modelTransform.localScale = Vector3.one;
        }
        
        // 親のRigidbodyを確認・調整
        Rigidbody parentRigidbody = GetComponent<Rigidbody>();
        if (parentRigidbody != null)
        {
            // 地面に適切に配置
            Vector3 currentPos = transform.position;
            currentPos.y = Mathf.Max(currentPos.y, 1.5f); // 最低でもY=1.5
            transform.position = currentPos;
            
            LogDebug($"プレイヤー位置を調整: {transform.position}");
        }
    }
    
    /// <summary>
    /// 素材を適用
    /// </summary>
    /// <param name="materialIndex">素材インデックス</param>
    private void ApplyMaterial(int materialIndex)
    {
        if (currentRenderers == null) return;
        
        Material targetMaterial = null;
        
        if (materialIndex == 0)
        {
            targetMaterial = defaultMaterial;
        }
        else if (alternativeMaterials != null && materialIndex - 1 < alternativeMaterials.Length)
        {
            targetMaterial = alternativeMaterials[materialIndex - 1];
        }
        
        if (targetMaterial != null)
        {
            foreach (Renderer renderer in currentRenderers)
            {
                if (renderer != null)
                {
                    renderer.material = targetMaterial;
                }
            }
            
            LogDebug($"素材 {materialIndex} を適用: {targetMaterial.name}");
        }
    }
    
    /// <summary>
    /// カメラの方向に回転（ビルボード効果）
    /// </summary>
    private void RotateTowardsCamera()
    {
        if (playerSpriteRenderer == null) return;
        
        Vector3 directionToCamera = mainCamera.transform.position - playerSpriteRenderer.transform.position;
        directionToCamera.y = 0; // Y軸回転のみ（垂直方向は固定）
        
        if (directionToCamera != Vector3.zero)
        {
            playerSpriteRenderer.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    /// <summary>
    /// 次のモデルに切り替え
    /// </summary>
    public void NextModel()
    {
        if (!useSpriteMode)
        {
            int maxModels = 1 + (alternativeModels != null ? alternativeModels.Length : 0);
            currentModelIndex = (currentModelIndex + 1) % maxModels;
            ActivateModel(currentModelIndex);
        }
    }
    
    /// <summary>
    /// 次の素材に切り替え
    /// </summary>
    public void NextMaterial()
    {
        if (!useSpriteMode)
        {
            int maxMaterials = 1 + (alternativeMaterials != null ? alternativeMaterials.Length : 0);
            currentMaterialIndex = (currentMaterialIndex + 1) % maxMaterials;
            ApplyMaterial(currentMaterialIndex);
        }
    }
    
    /// <summary>
    /// 次のスプライトに切り替え
    /// </summary>
    public void NextSprite()
    {
        if (useSpriteMode && playerSprites != null && playerSprites.Length > 0)
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % playerSprites.Length;
            playerSpriteRenderer.sprite = playerSprites[currentSpriteIndex];
            LogDebug($"スプライト変更: {currentSpriteIndex}");
        }
    }
    
    /// <summary>
    /// スプライトモードの切り替え
    /// </summary>
    /// <param name="enableSprite">スプライトモード有効</param>
    public void SetSpriteMode(bool enableSprite)
    {
        useSpriteMode = enableSprite;
        InitializePlayerAppearance();
        LogDebug($"スプライトモード: {(enableSprite ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// 特定のモデルを設定
    /// </summary>
    /// <param name="modelIndex">モデルインデックス</param>
    public void SetModel(int modelIndex)
    {
        if (modelIndex >= 0 && modelIndex < GetMaxModelCount())
        {
            currentModelIndex = modelIndex;
            if (!useSpriteMode)
            {
                ActivateModel(currentModelIndex);
            }
        }
    }
    
    /// <summary>
    /// 特定の素材を設定
    /// </summary>
    /// <param name="materialIndex">素材インデックス</param>
    public void SetMaterial(int materialIndex)
    {
        if (materialIndex >= 0 && materialIndex < GetMaxMaterialCount())
        {
            currentMaterialIndex = materialIndex;
            if (!useSpriteMode)
            {
                ApplyMaterial(currentMaterialIndex);
            }
        }
    }
    
    /// <summary>
    /// 最大モデル数を取得
    /// </summary>
    /// <returns>最大モデル数</returns>
    public int GetMaxModelCount()
    {
        return 1 + (alternativeModels != null ? alternativeModels.Length : 0);
    }
    
    /// <summary>
    /// 最大素材数を取得
    /// </summary>
    /// <returns>最大素材数</returns>
    public int GetMaxMaterialCount()
    {
        return 1 + (alternativeMaterials != null ? alternativeMaterials.Length : 0);
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    private void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[プレイヤー外観] {message}");
        }
    }
}