using UnityEngine;

/// <summary>
/// シンプルなカメラ追従システム - 位置のみ追従、回転は Unity で固定設定
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("追従設定")]
    [SerializeField] private Transform target; // 追従対象（Player）
    [SerializeField] private bool findPlayerAutomatically = true; // プレイヤー自動検索
    
    [Header("俯瞰視点設定")]
    [SerializeField] private float topDownHeight = 20f; // 俯瞰高さ
    [SerializeField] private Vector3 offsetPosition = Vector3.zero; // 追加オフセット
    
    [Header("スムージング")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float smoothSpeed = 8f; // 追従速度
    
    void Start()
    {
        InitializeCameraFollow();
    }
    
    /// <summary>
    /// カメラ追従初期化
    /// </summary>
    private void InitializeCameraFollow()
    {
        // プレイヤー自動検索
        if (findPlayerAutomatically && target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"[カメラ追従] プレイヤーを発見: {player.name}");
            }
            else
            {
                Debug.LogError("[カメラ追従] プレイヤーが見つかりません");
                return;
            }
        }
        
        Debug.Log("[カメラ追従] 初期化完了 - 位置追従のみ（回転は Unity で固定）");
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // プレイヤーの真上＋オフセット位置に配置
        Vector3 desiredPosition = target.position + Vector3.up * topDownHeight + offsetPosition;
        
        if (useSmoothing)
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = desiredPosition;
        }
        
        // 回転は一切触らない - Unity エディターで設定された回転を維持
    }
}