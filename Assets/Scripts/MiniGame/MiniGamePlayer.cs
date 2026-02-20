using UnityEngine;
using UnityEngine.UI;

public class MiniGamePlayer : MonoBehaviour
{
    [Header("移動設定")]
    public float speed = 10f;
    public float leftBound = -8f;
    public float rightBound = 8f;
    
    [Header("外観設定")]
    public bool usePlayerSprite = true; // プレイヤーをスプライト表示するか
    public float playerScale = 1f; // プレイヤーのサイズ
    public string playerSpriteName = "player"; // プレイヤー画像名（Resources内）
    
    [Header("装備設定")]
    public EquipmentType playerEquipmentType;
    
    [Header("ゲーム進行")]
    public float maxGauge = 100f;    // ログと一致するよう戻す
    public float currentGauge = 50f; // 60 → 50に調整（ログと一致）
    
    [Header("収集設定")]
    public int skillTriggerCount = 3; // スキル発動に必要な収集数
    
    // 各装備の収集数
    private int swordCount = 0;
    private int shieldCount = 0;
    private int staffCount = 0;
    
    // ゲーム状態管理
    private bool gameEnded = false; // 重複処理防止
    private MiniGameManager gameManager; // MiniGameManagerへの参照
    private EquipmentUI equipmentUI; // 装備UI参照
    
    private Renderer playerRenderer;
    private Rigidbody rb; // Rigidbody（FallingObject、3D）使用

    void Start()
    {
        // Rigidbody取得
        rb = GetComponent<Rigidbody>();
        
        // MiniGameManager参照取得
        gameManager = FindObjectOfType<MiniGameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("[MiniPlayer] MiniGameManagerが見つかりません。自動で作成します。");
            
            // 新しいGameObjectを作成してMiniGameManagerを追加
            GameObject managerObject = new GameObject("MiniGameManager");
            gameManager = managerObject.AddComponent<MiniGameManager>();
            
            Debug.Log("[MiniPlayer] MiniGameManagerを自動作成しました。");
        }
        
        // プレイヤー外観設定
        SetupPlayerAppearance();
        
        // EquipmentUI参照取得
        equipmentUI = FindObjectOfType<EquipmentUI>();
        if (equipmentUI != null)
        {
            Debug.Log("[MiniPlayer] EquipmentUIと連携します");
        }
        
        // 重要な初期化のみログ出力
        Debug.Log($"[MiniPlayer] 初期化開始");
        
        // エラーチェック
        if (rb == null)
        {
            Debug.LogError("[MiniPlayer] Rigidbodyコンポーネントが見つかりません！");
        }
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[MiniPlayer] Colliderコンポーネントが見つかりません！");
        }
        
        InitializePlayer();
    }

    void Update()
    {
        HandleMovement();
    }
    
    void OnTriggerEnter(Collider other)
    {
        FallingObject fallingObject = other.GetComponent<FallingObject>();
        if (fallingObject != null)
        {
            // FallingObject側で処理される
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 必要に応じてログ追加
    }
    
    void InitializePlayer()
    {
        // ゲーム状態をリセット
        gameEnded = false;
        currentGauge = 50f; // 初期ゲージ値にリセット
        
        // スコアカウンターをリセット
        swordCount = 0;
        shieldCount = 0;
        staffCount = 0;
        Debug.Log("[MiniPlayer] ゲーム状態とスコアカウンターをリセットしました");
        
        // ランダムな装備タイプを設定
        EquipmentData randomEquipment = EquipmentManager.GetRandomEquipment();
        playerEquipmentType = randomEquipment.type;
        
        // プレイヤーの色を装備に応じて変更
        playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.material.color = randomEquipment.color;
        }
        
        // 装備アイコンをUIに表示
        if (gameManager != null)
        {
            gameManager.DisplayPlayerEquipmentIcon(playerEquipmentType);
        }
        
        Debug.Log($"[ゲーム開始] プレイヤー装備: {randomEquipment.name} | ゲージ: {currentGauge}/{maxGauge}");
        Debug.Log($"[目標] {randomEquipment.name}を集めてゲージを満タンにしよう！");
    }
    
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        
        Vector3 move = new Vector3(h * speed * Time.deltaTime, 0, 0);
        
        // 緊急対策：強制的にTransform移動
        transform.Translate(move);
        
        // 画面端に留める
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);
        transform.position = pos;
    }
    
    public void CollectEquipment(EquipmentType collectedType)
    {
        // 装備タイプに応じて収集数をカウント
        switch (collectedType)
        {
            case EquipmentType.Sword:
                swordCount++;
                Debug.Log($"剣を取得！ (計{swordCount}個)");
                break;
            case EquipmentType.Shield:
                shieldCount++;
                Debug.Log($"盾を取得！ (計{shieldCount}個)");
                break;
            case EquipmentType.Staff:
                staffCount++;
                Debug.Log($"杖を取得！ (計{staffCount}個)");
                break;
        }
        
        // 装備UI通知
        if (equipmentUI != null)
        {
            equipmentUI.ShowCollectNotification(collectedType);
        }
        
        // ゲージ更新
        UpdateGauge(collectedType);
        
        // スキル発動チェック
        CheckSkillActivation(collectedType);
    }
    
    void UpdateGauge(EquipmentType collectedType)
    {
        float oldGauge = currentGauge;
        
        if (collectedType == playerEquipmentType)
        {
            // 有利な装備：ゲージ増加 (3個で満タンになるよう調整)
            currentGauge += 10f; // 15f → 10f に変更
            Debug.Log($"[ゲージ] 有利装備収集！ {oldGauge} → {currentGauge} (+10)");
        }
        else
        {
            // 不利な装備：ゲージ減少  
            currentGauge -= 6f; // 10f → 6f に変更
            Debug.Log($"[ゲージ] 不利装備収集... {oldGauge} → {currentGauge} (-6)");
        }
        
        // ゲージの制限（先に制限してから判定）
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
        Debug.Log($"[ゲージ] 最終値: {currentGauge}/{maxGauge} ({(currentGauge/maxGauge*100):F1}%)");
        
        // ゲーム終了チェック（制限後の値で判定）
        if (!gameEnded && currentGauge >= maxGauge)
        {
            gameEnded = true;
            Debug.Log("■■■ ゲージ満タン！ゲームクリア！ ■■■");
            
            // キャッシュされたMiniGameManager参照を使用
            if (gameManager != null)
            {
                gameManager.GameClear();
            }
            else
            {
                Debug.LogError("[エラー] MiniGameManagerが無効です！ゲーム終了処理を実行できません。");
            }
        }
        else if (!gameEnded && currentGauge <= 0)
        {
            gameEnded = true;
            Debug.Log("■■■ ゲージ0！ゲームオーバー... ■■■");
            
            // キャッシュされたMiniGameManager参照を使用
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
            else
            {
                Debug.LogError("[エラー] MiniGameManagerが無効です！ゲーム終了処理を実行できません。");
            }
        }
    }
    
    void CheckSkillActivation(EquipmentType collectedType)
    {
        int count = GetEquipmentCount(collectedType);
        
        if (count >= skillTriggerCount && count % skillTriggerCount == 0)
        {
            ActivateSkill(collectedType);
        }
    }
    
    void ActivateSkill(EquipmentType equipmentType)
    {
        string skillName = GetSkillName(equipmentType);
        Debug.Log($"■■■ {skillName}発動！ ■■■");
        
        // スキル効果（調整済み）
        currentGauge += 15f; // 20f → 15f に変更
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
        Debug.Log($"スキル効果でゲージ+15！ 現在: {currentGauge}/{maxGauge}");
    }
    
    string GetSkillName(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Sword:
                return "剣の一閃";
            case EquipmentType.Shield:
                return "鉄壁の守り";
            case EquipmentType.Staff:
                return "魔力の奔流";
            default:
                return "スキル";
        }
    }
    
    /// <summary>
    /// 個別装備の収集数を取得
    /// </summary>
    public int GetEquipmentCount(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Sword:
                return swordCount;
            case EquipmentType.Shield:
                return shieldCount;
            case EquipmentType.Staff:
                return staffCount;
            default:
                return 0;
        }
    }
    
    /// <summary>
    /// プレイヤー外観の設定
    /// </summary>
    void SetupPlayerAppearance()
    {
        if (!usePlayerSprite) return;
        
        // 既存のMeshRendererを削除してSpriteRendererに変更
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            DestroyImmediate(meshRenderer);
            Debug.Log("[MiniPlayer] MeshRenderer削除");
        }
        
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            DestroyImmediate(meshFilter);
            Debug.Log("[MiniPlayer] MeshFilter削除");
        }
        
        // SpriteRendererを追加
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.Log("[MiniPlayer] SpriteRenderer追加");
        }
        
        // プレイヤー装備に応じた画像を設定
        SetPlayerSprite();
        
        // サイズ調整
        transform.localScale = Vector3.one * playerScale;
        
        Debug.Log("[MiniPlayer] プレイヤー外観設定完了");
    }
    
    /// <summary>
    /// プレイヤーのスプライト設定
    /// </summary>
    void SetPlayerSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        
        // プレイヤー専用画像を読み込み
        Sprite playerSprite = Resources.Load<Sprite>($"Illustrations/{playerSpriteName}");
        
        if (playerSprite != null)
        {
            spriteRenderer.sprite = playerSprite;
            // スプライト使用時は元の色を保持（色を変更しない）
            // spriteRenderer.color = Color.white; // この行を削除
            spriteRenderer.sortingOrder = 2; // プレイヤーを最前面
            Debug.Log($"[MiniPlayer] プレイヤー画像設定: {playerSpriteName} - 元の色を保持");
        }
        else
        {
            // フォールバック：シンプルな色付きシェイプ（画像がない場合のみ色設定）
            spriteRenderer.color = Color.cyan; // シアン色でプレイヤーとして表示
            spriteRenderer.sortingOrder = 2;
            Debug.LogWarning($"[MiniPlayer] {playerSpriteName}.pngが見つかりません - 色で表示");
        }
    }
    
    /// <summary>
    /// 合計スコアを取得（装備収集数の合計）
    /// </summary>
    public int GetTotalScore()
    {
        return swordCount + shieldCount + staffCount;
    }
}