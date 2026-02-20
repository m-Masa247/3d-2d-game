using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    public float normalSpeed = 8f;     // 通常移動スピード
    public float dashSpeed = 15f;      // ダッシュスピード
    public KeyCode dashKey = KeyCode.LeftShift; // ダッシュキー
    
    [Header("ダッシュ制限（オプション）")]
    public bool useStamina = false;    // スタミナ制限を使用するか
    public float maxStamina = 3f;      // 最大スタミナ（秒）
    public float staminaRegenRate = 1f; // スタミナ回復率（秒/秒）
    
    private Rigidbody rb;
    private float currentStamina;
    private bool isDashing = false;
    private Camera playerCamera; // プレイヤーのカメラ参照
    private CameraFollow cameraFollow; // カメラ追従システム参照
    
    void Awake()
    {
        Debug.Log("[プレイヤー移動] Awake() 呼び出し - オブジェクトの確認");
        
        // Playerオブジェクトを確実にアクティブ化
        gameObject.SetActive(true);
        
        Debug.Log($"[プレイヤー移動] Player状態確認 - アクティブ: {gameObject.activeInHierarchy}");
    }
    
    void Start()
    {
        Debug.Log("[プレイヤー移動] PlayerMoveを初期化中");
        
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("[プレイヤー移動] Rigidbodyが見つかりません！");
            return;
        }
        
        // カメラを取得（子要素から検索）
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            // メインカメラを使用
            playerCamera = Camera.main;
            Debug.Log("[プレイヤー移動] メインカメラを使用");
        }
        else
        {
            Debug.Log("[プレイヤー移動] プレイヤーカメラを取得");
        }
        
        // CameraFollowコンポーネントを取得
        if (playerCamera != null)
        {
            cameraFollow = playerCamera.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                Debug.Log("[プレイヤー移動] CameraFollowシステムを検出");
            }
        }
        
        currentStamina = maxStamina;
        
        Debug.Log($"[プレイヤー移動] 初期化完了 - 位置: {transform.position}");
        
        // MiniGameから戻った場合、保存された位置を復元
        if (PlayerPositionManager.HasSavedPosition())
        {
            PlayerPositionManager.RestorePlayerPosition(transform);
            
            // 位置復元後、確実にMiniGameフラグをリセット
            EnemyEvent.ResetMiniGameInProgress();
            Debug.Log("[PlayerMove] 位置復元完了 - MiniGameフラグをリセット");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleStamina();
    }
    
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // カメラの向きを基準にした相対移動ベクトルを計算
        Vector3 move = GetRelativeMoveVector(h, v);
        
        // 入力デバッグ（最初の数秒のみ）
        if (Time.timeSinceLevelLoad < 5f && (h != 0 || v != 0))
        {
            Debug.Log($"[プレイヤー移動] 入力検知 H:{h:F2}, V:{v:F2}, 相対移動:{move}");
        }
        
        // ダッシュ判定
        bool wantsToDash = Input.GetKey(dashKey);
        bool canDash = !useStamina || currentStamina > 0;
        
        isDashing = wantsToDash && canDash && move.magnitude > 0;
        
        // 移動速度の決定
        float currentSpeed = isDashing ? dashSpeed : normalSpeed;
        
        // 物理移動
        if (rb != null)
        {
            Vector3 velocity = move * currentSpeed;
            velocity.y = rb.velocity.y; // Y軸の速度は保持（重力など）
            rb.velocity = velocity;
            
            // 移動デバッグ（最初の数秒のみ）
            if (Time.timeSinceLevelLoad < 5f && move.magnitude > 0)
            {
                Debug.Log($"[プレイヤー移動] 移動中 - 速度:{velocity.magnitude:F2}, 位置:{transform.position}");
            }
        }
        else
        {
            Debug.LogError("[プレイヤー移動] Rigidbodyが見つかりません！");
        }
    }
    
    void HandleStamina()
    {
        if (!useStamina) return;
        
        if (isDashing)
        {
            // ダッシュ中はスタミナ減少
            currentStamina -= Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }
        else
        {
            // ダッシュしていない時はスタミナ回復
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }
    
    /// <summary>
    /// カメラの向きを基準にした相対移動ベクトルを計算
    /// </summary>
    /// <param name="horizontal">左右入力 (-1〜1)</param>
    /// <param name="vertical">前後入力 (-1〜1)</param>
    /// <returns>カメラ基準の移動ベクトル</returns>
    private Vector3 GetRelativeMoveVector(float horizontal, float vertical)
    {
        // 俯瞰視点の場合は常に世界座標系で移動（デフォルト動作）
        if (cameraFollow != null && IsTopDownView())
        {
            return new Vector3(horizontal, 0, vertical);
        }
        
        // デフォルトも世界座標系（俯瞰視点メイン運用のため）
        return new Vector3(horizontal, 0, vertical);
    }
    
    /// <summary>
    /// 現在が一人称視点かどうかを判定
    /// </summary>
    /// <returns>true: 一人称視点, false: その他</returns>
    private bool IsFirstPersonView()
    {
        if (cameraFollow != null)
        {
            var field = cameraFollow.GetType().GetField("viewType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var viewTypeValue = field.GetValue(cameraFollow);
                return viewTypeValue.ToString() == "FirstPerson";
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 現在が俯瞰視点かどうかを判定
    /// </summary>
    /// <returns>true: 俯瞰視点, false: 一人称視点</returns>
    private bool IsTopDownView()
    {
        // リフレクションを使ってviewTypeフィールドの値を取得
        if (cameraFollow != null)
        {
            var field = cameraFollow.GetType().GetField("viewType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var viewTypeValue = field.GetValue(cameraFollow);
                return viewTypeValue.ToString() == "TopDown";
            }
        }
        
        return false; // デフォルトは一人称視点として扱う
    }
}