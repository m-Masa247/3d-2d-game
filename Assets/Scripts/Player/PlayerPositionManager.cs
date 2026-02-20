using UnityEngine;

/// <summary>
/// プレイヤーの位置を管理する静的クラス
/// MiniGame前後での位置保存・復元を行う
/// </summary>
public static class PlayerPositionManager
{
    private static Vector3 savedPosition = Vector3.zero;
    private static Quaternion savedRotation = Quaternion.identity;
    private static bool hasPositionSaved = false;
    private static bool lastGameResult = false; // 最後のゲーム結果（true=成功, false=失敗）
    private static float groundY = 1.5f; // 地面の高さ（プレイヤーの足元）
    
    /// <summary>
    /// 現在のプレイヤー位置を保存
    /// </summary>
    public static void SavePlayerPosition(Transform playerTransform)
    {
        if (playerTransform != null)
        {
            savedPosition = playerTransform.position;
            savedRotation = playerTransform.rotation;
            hasPositionSaved = true;
            Debug.Log($"[位置管理] プレイヤー位置を保存: {savedPosition}");
        }
    }
    
    /// <summary>
    /// 成功時：接敵位置に復元（敵は削除されるため安全）
    /// </summary>
    public static void RestorePlayerPositionOnSuccess(Transform playerTransform)
    {
        if (playerTransform != null && hasPositionSaved)
        {
            Vector3 restorePosition = new Vector3(savedPosition.x, groundY, savedPosition.z);
            // 成功時でも安全のため、わずかに後退
            restorePosition = GetSafePositionForSuccess(restorePosition);
            
            playerTransform.position = restorePosition;
            playerTransform.rotation = savedRotation;
            Debug.Log($"[位置管理] 成功時位置復元: {restorePosition} (元: {savedPosition})");
        }
        else
        {
            Debug.LogWarning("[位置管理] 保存された位置がありません");
        }
    }
    
    /// <summary>
    /// 成功時の安全位置を取得（削除される敵から離れる）
    /// </summary>
    private static Vector3 GetSafePositionForSuccess(Vector3 originalPosition)
    {
        // 削除対象の敵名を取得
        string deleteTargetName = PlayerPrefs.GetString("DeleteTargetEnemy", "");
        
        Vector3 safePosition = originalPosition;
        
        if (!string.IsNullOrEmpty(deleteTargetName))
        {
            // 削除対象の敵を検索
            EnemyEvent[] enemies = GameObject.FindObjectsOfType<EnemyEvent>();
            
            foreach (EnemyEvent enemy in enemies)
            {
                if (enemy.name == deleteTargetName)
                {
                    // 削除される敵から1m離れた位置に移動
                    Vector3 directionAway = (originalPosition - enemy.transform.position).normalized;
                    safePosition = originalPosition + directionAway * 1f;
                    safePosition.y = groundY;
                    Debug.Log($"[位置管理] 成功時安全位置調整: {originalPosition} → {safePosition}");
                    break;
                }
            }
        }
        
        return safePosition;
    }
    
    /// <summary>
    /// 失敗時：接敵位置より手前に復元（連続エンカウント防止）
    /// </summary>
    public static void RestorePlayerPositionOnFailure(Transform playerTransform)
    {
        if (playerTransform != null && hasPositionSaved)
        {
            Vector3 retreatPosition = GetRetreatPosition(savedPosition);
            playerTransform.position = retreatPosition;
            playerTransform.rotation = savedRotation;
            Debug.Log($"[位置管理] 失敗時位置復元: {retreatPosition} (元: {savedPosition})");
        }
        else
        {
            Debug.LogWarning("[位置管理] 保存された位置がありません");
        }
    }
    
    /// <summary>
    /// ゲーム結果を設定
    /// </summary>
    public static void SetGameResult(bool isSuccess)
    {
        lastGameResult = isSuccess;
        Debug.Log($"[位置管理] ゲーム結果設定: {(isSuccess ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// ゲーム結果を取得
    /// </summary>
    public static bool GetGameResult()
    {
        return lastGameResult;
    }
    
    /// <summary>
    /// 保存されたプレイヤー位置を復元（結果に応じて自動選択）
    /// </summary>
    public static void RestorePlayerPosition(Transform playerTransform)
    {
        if (lastGameResult)
        {
            RestorePlayerPositionOnSuccess(playerTransform);
        }
        else
        {
            RestorePlayerPositionOnFailure(playerTransform);
        }
    }
    
    /// <summary>
    /// 位置が保存されているかチェック
    /// </summary>
    public static bool HasSavedPosition()
    {
        return hasPositionSaved;
    }
    
    /// <summary>
    /// 保存された位置データをクリア
    /// </summary>
    public static void ClearSavedPosition()
    {
        hasPositionSaved = false;
        Debug.Log("[位置管理] 保存された位置データをクリア");
    }
    
    /// <summary>
    /// 後退位置を計算（敵側と反対方向に移動）
    /// </summary>
    private static Vector3 GetRetreatPosition(Vector3 originalPosition)
    {
        // 敵のオブジェクトを検索
        EnemyEvent[] enemies = GameObject.FindObjectsOfType<EnemyEvent>();
        
        Vector3 retreatPosition = originalPosition;
        float retreatDistance = 2f; // 敵から2m手前に移動
        
        foreach (EnemyEvent enemy in enemies)
        {
            float distance = Vector3.Distance(originalPosition, enemy.transform.position);
            Debug.Log($"[位置管理] 敵 {enemy.name} との距離: {distance:F2}m");
            
            if (distance < 4f) // 4m以内の敵から後退
            {
                // 敵から離れた方向（プレイヤーが来た方向）に移動
                Vector3 directionAway = (originalPosition - enemy.transform.position).normalized;
                retreatPosition = originalPosition + directionAway * retreatDistance;
                retreatPosition.y = groundY; // 地面に設置
                Debug.Log($"[位置管理] 敵回避後退: {originalPosition} → {retreatPosition}");
                break; // 最初に見つけた敵からの後退のみ
            }
        }
        
        // Y座標を地面の高さに固定
        retreatPosition.y = groundY;
        return retreatPosition;
    }
}