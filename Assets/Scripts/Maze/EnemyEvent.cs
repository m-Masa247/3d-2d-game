using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEvent : MonoBehaviour
{
    private static string deleteTargetEnemyName; // 削除対象の敵名
    private static bool isMiniGameInProgress = false; // MiniGame進行中フラグ
    private bool isMarkedForDestroy = false; // 削除マーク
    
    private void Start()
    {
        // 削除マークされていたら削除
        string deleteTarget = PlayerPrefs.GetString("DeleteTargetEnemy", "");
        if (name == deleteTarget)
        {
            Debug.Log($"[敵管理] 敵 {name} は削除対象 - 削除します");
            PlayerPrefs.DeleteKey("DeleteTargetEnemy"); // 削除完了後にクリア
            deleteTargetEnemyName = null;
            
            // 敵削除完了後にフラグをリセット
            isMiniGameInProgress = false;
            Debug.Log("[敵管理] 敵削除完了により進行フラグをリセット");
            
            Destroy(gameObject); // 即座に削除
            return;
        }
        
        // 削除対象でない場合、初期起動時のみフラグリセット
        if (Time.timeSinceLevelLoad < 0.1f) // シーン開始直後のみ
        {
            isMiniGameInProgress = false;
            Debug.Log("[敵管理] シーン開始により進行フラグをリセット");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 削除対象かチェック
        string deleteTarget = PlayerPrefs.GetString("DeleteTargetEnemy", "");
        if (name == deleteTarget)
        {
            Debug.Log($"[敵管理] {name} は削除対象のためトリガー無効化");
            return; // 削除対象の敵はトリガー無効
        }
        
        if (other.CompareTag("Player") && !isMarkedForDestroy && !isMiniGameInProgress)
        {
            isMiniGameInProgress = true; // 重複防止フラグ設定
            Debug.Log($"[敵管理] 進行フラグ設定: {isMiniGameInProgress}");
            
            Debug.Log($"■ 敵 {name} と接敵！MiniGameに挑戦します ■");
            
            // プレイヤーの現在位置を保存
            PlayerPositionManager.SavePlayerPosition(other.transform);
            
            // この敵を削除対象として記録（PlayerPrefsで永続化）
            PlayerPrefs.SetString("DeleteTargetEnemy", name);
            PlayerPrefs.Save();
            Debug.Log($"[敵管理] 削除対象敵名設定: {name}");
            
            // MiniGameSceneへ遷移
            SceneManager.LoadScene("MiniGameScene");
        }
        else if (isMiniGameInProgress)
        {
            Debug.Log($"[敵管理] MiniGame進行中のため {name} のトリガーを無視");
        }
    }
    
    /// <summary>
    /// ゲームクリア時に敵を削除マーク
    /// </summary>
    public static void RemoveTriggeredEnemy()
    {
        deleteTargetEnemyName = PlayerPrefs.GetString("DeleteTargetEnemy", "");
        Debug.Log($"[敵管理] RemoveTriggeredEnemy呼び出し - 削除対象: {deleteTargetEnemyName}");
        
        if (!string.IsNullOrEmpty(deleteTargetEnemyName))
        {
            Debug.Log($"[敵管理] 敵 {deleteTargetEnemyName} を削除マーク設定");
            // 削除処理はMazeScene復帰時のStart()で実行される
        }
        else
        {
            Debug.LogWarning("[敵管理] 削除対象の敵名が見つかりません");
        }
    }
    
    /// <summary>
    /// 失敗時は敵を保持（削除マークをクリア）
    /// </summary>
    public static void KeepTriggeredEnemy()
    {
        string enemyName = PlayerPrefs.GetString("DeleteTargetEnemy", "");
        Debug.Log($"[敵管理] KeepTriggeredEnemy呼び出し - 対象: {enemyName}");
        
        if (!string.IsNullOrEmpty(enemyName))
        {
            Debug.Log("[敵管理] ゲーム失敗により削除マークをクリア");
            PlayerPrefs.DeleteKey("DeleteTargetEnemy"); // マークをクリア（敵は残す）
            deleteTargetEnemyName = null;
        }
    }
    
    /// <summary>
    /// MiniGame進行中フラグをリセット
    /// </summary>
    public static void ResetMiniGameInProgress()
    {
        isMiniGameInProgress = false;
        Debug.Log("[敵管理] MiniGame進行中フラグをリセット");
    }
}