using UnityEngine;

/// <summary>
/// マウス移動によるカメラ制御
/// 3D迷路ゲーム用の一人称視点制御
/// </summary>
public class MouseLook : MonoBehaviour
{
    [Header("マウス感度設定")]
    [SerializeField] private float mouseSensitivity = 100f; // マウス感度（調整済み）
    [SerializeField] private float verticalLookLimit = 80f; // 縦方向の視線制限（度）
    
    [Header("制御設定")]
    [SerializeField] private bool lockCursor = false; // カーソルロック（俯瞰視点では無効）
    [SerializeField] private bool invertY = false; // Y軸反転
    [SerializeField] private bool requireMouseButton = false; // マウスボタン必須（falseで常時有効）
    
    [Header("スムージング設定")]
    [SerializeField] private bool useSmoothing = false; // スムージング使用（無効にしてレスポンス向上）
    [SerializeField] private float smoothTime = 0.05f; // スムージング時間（短縮）
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInput = true; // 入力デバッグ表示（初期有効）
    
    private Transform playerBody; // プレイヤー本体のTransform
    private float xRotation = 0f; // X軸（上下）回転値
    private Vector2 currentMouseDelta = Vector2.zero; // 現在のマウス移動量
    private Vector2 currentMouseVelocity = Vector2.zero; // マウス移動の速度（スムージング用）
    
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeMouseLook();
    }
    
    /// <summary>
    /// マウスルック初期化
    /// </summary>
    private void InitializeMouseLook()
    {
        // プレイヤー本体の Transform を取得
        playerBody = transform.parent;
        if (playerBody == null)
        {
            // カメラがプレイヤーの子でない場合、プレイヤーを検索
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Playerタグがない場合、名前で検索
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                playerBody = player.transform;
                Debug.Log($"[マウスルック] プレイヤーを発見: {player.name}");
            }
            else
            {
                Debug.LogError("[マウスルック] プレイヤーオブジェクトが見つかりません！ Playerタグまたは'Player'という名前のオブジェクトを確認してください");
                return;
            }
        }
        
        Debug.Log($"[マウスルック] プレイヤー本体: {playerBody.name}, カメラ位置: {transform.position}");
        
        // カーソルロック設定
        if (lockCursor)
        {
            LockCursor();
        }
        
        isInitialized = true;
        
        // 俯瞰視点メインのため初期状態では無効化
        enabled = false;
        
        Debug.Log("[マウスルック] 初期化完了（俯瞰視点用に無効化）");
    }
    
    void Update()
    {
        // 俯瞰視点メインのためMouseLook処理を無効化
        return;
    }
    
    /// <summary>
    /// マウスルック処理
    /// </summary>
    private void HandleMouseLook()
    {
        // マウスボタン必須の場合のチェック
        if (requireMouseButton && !Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            return;
        }
        
        // マウス入力取得（安定した処理）
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Y軸反転処理
        if (invertY)
        {
            mouseY = -mouseY;
        }
        
        // デバッグ出力（最初の数秒のみ）
        if (showDebugInput && Time.timeSinceLevelLoad < 5f && (Mathf.Abs(mouseX) > 0.1f || Mathf.Abs(mouseY) > 0.1f))
        {
            Debug.Log($"[マウスルック] マウス入力 X:{mouseX:F2}, Y:{mouseY:F2}");
        }
        
        Vector2 targetDelta = new Vector2(mouseX, mouseY);
        
        // スムージング処理
        if (useSmoothing)
        {
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetDelta, ref currentMouseVelocity, smoothTime);
        }
        else
        {
            currentMouseDelta = targetDelta;
        }
        
        // 横方向回転（Y軸）- プレイヤー本体を回転
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * currentMouseDelta.x);
            
            // デバッグ：プレイヤー回転確認
            if (showDebugInput && Mathf.Abs(currentMouseDelta.x) > 0.1f)
            {
                Debug.Log($"[マウスルック] プレイヤー回転: {currentMouseDelta.x:F2}");
            }
        }
        
        // 縦方向回転（X軸）- カメラを回転
        xRotation -= currentMouseDelta.y;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);
        
        // カメラに回転を適用
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // デバッグ：カメラ回転確認
        if (showDebugInput && Mathf.Abs(currentMouseDelta.y) > 0.1f)
        {
            Debug.Log($"[マウスルック] カメラ回転: {xRotation:F1}度");
        }
    }
    
    /// <summary>
    /// カーソルロック
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[マウスルック] カーソルをロックしました");
    }
    
    /// <summary>
    /// カーソルロック解除
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[マウスルック] カーソルロックを解除しました");
    }
    
    /// <summary>
    /// カーソルロック切り替え
    /// </summary>
    public void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
    
    /// <summary>
    /// マウス感度設定
    /// </summary>
    /// <param name="sensitivity">感度</param>
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
        Debug.Log($"[マウスルック] マウス感度を {sensitivity} に設定");
    }
    
    /// <summary>
    /// Y軸反転設定
    /// </summary>
    /// <param name="invert">反転するか</param>
    public void SetInvertY(bool invert)
    {
        invertY = invert;
        Debug.Log($"[マウスルック] Y軸反転: {(invert ? "ON" : "OFF")}");
    }
    
    /// <summary>
    /// 視線制限角度設定
    /// </summary>
    /// <param name="limit">制限角度（度）</param>
    public void SetVerticalLookLimit(float limit)
    {
        verticalLookLimit = Mathf.Clamp(limit, 0f, 90f);
        Debug.Log($"[マウスルック] 縦方向制限を {verticalLookLimit} 度に設定");
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        // アプリケーションがフォーカスを失った時の処理
        if (!hasFocus && lockCursor)
        {
            UnlockCursor();
        }
    }
    
    /// <summary>
    /// 現在の設定をデバッグ表示
    /// </summary>
    [ContextMenu("デバッグ情報表示")]
    public void ShowDebugInfo()
    {
        Debug.Log($"[マウスルック] デバッグ情報:");
        Debug.Log($"  - マウス感度: {mouseSensitivity}");
        Debug.Log($"  - カーソルロック: {Cursor.lockState}");
        Debug.Log($"  - プレイヤー本体: {(playerBody != null ? playerBody.name : "なし")}");
        Debug.Log($"  - 初期化状態: {isInitialized}");
        Debug.Log($"  - 現在のX回転: {xRotation:F1}度");
    }
}