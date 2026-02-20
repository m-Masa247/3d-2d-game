using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    [Header("ゲーム設定")]
    public float gameTime = 60f; // ゲーム時間（秒）
    
    [Header("UI要素")]
    public Slider gaugeSlider; // ゲージスライダー
    public Text timeText; // 時間表示テキスト
    public Text scoreText; // スコア表示テキスト
    public Image equipmentIcon; // 装備アイコン表示（右上）
    public Text equipmentText; // 装備説明テキスト
    
    [Header("プレイヤー装備アイコン")]
    public Image playerEquipmentIcon; // 右上に表示するプレイヤー装備アイコン
    
    private bool gameEnded = false;
    private MiniGamePlayer player;
    private float startTime; // ゲーム開始時間

    void Start()
    {
        // プレイヤー参照を取得
        player = FindObjectOfType<MiniGamePlayer>();
        
        // ゲーム開始時刻を記録
        startTime = Time.time;
        
        // ゲーム状態をリセット
        gameEnded = false;
        Debug.Log("[MiniGameManager] ゲーム状態をリセットしました");
        
        // UI要素を自動取得（少し遅延して実行）
        StartCoroutine(InitializeUIWithDelay());
        
        // プレイヤー装備アイコン初期化
        SetupPlayerEquipmentIcon();
        
        // ゲームタイマー開始
        StartCoroutine(GameTimer());
        
        Debug.Log("■■■ ミニゲーム開始 ■■■");
        Debug.Log("目標: ゲージを満タンにしよう！");
    }
    
    /// <summary>
    /// UI初期化（遅延実行）
    /// </summary>
    System.Collections.IEnumerator InitializeUIWithDelay()
    {
        // UIが完全に生成されるまで少し待機
        yield return new WaitForSeconds(0.5f);
        
        InitializeUI();
        
        // 装備表示を強制更新
        yield return new WaitForSeconds(0.1f);
        UpdateEquipmentDisplay();
    }

    IEnumerator GameTimer()
    {
        yield return new WaitForSeconds(gameTime);
        
        if (!gameEnded)
        {
            TimeUp();
        }
    }
    
    public void GameClear()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log("■■■ ゲームクリア！ ■■■");
            
            // 成功時：敵を削除
            Debug.Log("[MiniGameManager] GameClear実行 - 敵削除処理を呼び出します");
            EnemyEvent.RemoveTriggeredEnemy();
            EnemyEvent.ResetMiniGameInProgress(); // フラグリセット
            PlayerPositionManager.SetGameResult(true); // 成功を設定
            
            // フラグを確実にリセット
            Debug.Log("[MiniGameManager] ゲーム終了によりフラグをリセット");
            
            StartCoroutine(EndGameWithDelay("ゲームクリア！迷路に戻ります", 2f, true));
        }
    }
    
    public void GameOver()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log("■■■ ゲームオーバー... ■■■");
            
            // 失敗時：敵を保持
            Debug.Log("[MiniGameManager] GameOver実行 - 敵保持処理を呼び出します");
            EnemyEvent.KeepTriggeredEnemy();
            EnemyEvent.ResetMiniGameInProgress(); // フラグリセット
            PlayerPositionManager.SetGameResult(false); // 失敗を設定
            
            // フラグを確実にリセット
            Debug.Log("[MiniGameManager] ゲーム終了によりフラグをリセット");
            
            StartCoroutine(EndGameWithDelay("ゲームオーバー...迷路に戻ります", 2f, false));
        }
    }
    
    void TimeUp()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            Debug.Log("■■■ 時間切れ！ ■■■");
            
            // 時間切れ時のゲージをチェック
            if (player != null && player.currentGauge >= player.maxGauge)
            {
                // ゲージ満タンでクリア扱い
                Debug.Log("[MiniGameManager] 時間切れ但しゲージ満タン - 敵削除処理を呼び出します");
                EnemyEvent.RemoveTriggeredEnemy();
                EnemyEvent.ResetMiniGameInProgress(); // フラグリセット
                PlayerPositionManager.SetGameResult(true); // 成功を設定
                StartCoroutine(EndGameWithDelay("時間切れですがゲージ満タン！迷路に戻ります", 2f, true));
            }
            else
            {
                // 失敗扱い
                Debug.Log("[MiniGameManager] 時間切れ失敗 - 敵保持処理を呼び出します");
                EnemyEvent.KeepTriggeredEnemy();
                EnemyEvent.ResetMiniGameInProgress(); // フラグリセット
                PlayerPositionManager.SetGameResult(false); // 失敗を設定
                StartCoroutine(EndGameWithDelay("時間切れ！迷路に戻ります", 2f, false));
            }
        }
    }

    IEnumerator EndGameWithDelay(string message, float delay, bool isSuccess)
    {
        Debug.Log(message);
        
        // スポナーを停止
        ObjectSpawner spawner = FindObjectOfType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        yield return new WaitForSeconds(delay);
        
        // 迷路シーンに戻る
        Debug.Log($"[シーン遷移] MazeSceneに戻ります (成功: {isSuccess})");
        SceneManager.LoadScene("MazeScene");
    }
    
    void Update()
    {
        if (!gameEnded)
        {
            UpdateUI();
        }
    }
    
    /// <summary>
    /// UI要素の初期化と自動取得
    /// </summary>
    void InitializeUI()
    {
        // Slider自動取得
        if (gaugeSlider == null)
        {
            gaugeSlider = FindObjectOfType<Slider>();
            if (gaugeSlider != null)
            {
                Debug.Log("[MiniGameManager] ゲージスライダーを自動取得");
            }
        }
        
        // Text要素を自動取得
        Text[] allTexts = FindObjectsOfType<Text>();
        foreach (Text text in allTexts)
        {
            string textContent = text.text.ToLower();
            
            // 時間関連のテキストを検索
            if ((textContent.Contains("time") || textContent.Contains("時間")) && timeText == null)
            {
                timeText = text;
                Debug.Log($"[MiniGameManager] 時間テキストを自動取得: {text.name}");
            }
            // スコア関連の検索は無効化（スコア表示不要）
            // else if ((textContent.Contains("score") || textContent.Contains("スコア")) && scoreText == null)
            // {
            //     scoreText = text;
            //     Debug.Log($"[MiniGameManager] スコアテキストを自動取得: {text.name}");
            // }
            // 装備関連のテキストを検索
            else if ((textContent.Contains("equipment") || textContent.Contains("装備")) && equipmentText == null)
            {
                equipmentText = text;
                Debug.Log($"[MiniGameManager] 装備テキストを自動取得: {text.name}");
            }
        }
        
        // Image要素を自動取得
        Image[] allImages = FindObjectsOfType<Image>();
        Debug.Log($"[MiniGameManager] 見つかったImage要素数: {allImages.Length}");
        
        foreach (Image image in allImages)
        {
            Debug.Log($"[MiniGameManager] Image発見: {image.name}");
            
            // 装備アイコン用のImageを検索（名前で判定）
            if (image.name.Contains("Equipment") && equipmentIcon == null)
            {
                equipmentIcon = image;
                Debug.Log($"[MiniGameManager] 装備アイコンを自動取得: {image.name}");
                break;
            }
        }
        
        // 装備アイコンが見つからない場合の警告
        if (equipmentIcon == null)
        {
            Debug.LogWarning("[MiniGameManager] 装備アイコンが見つかりません - 手動で作成してください");
            
            // Image要素のリストを出力（デバッグ用）
            foreach (Image img in allImages)
            {
                Debug.Log($"  - Image名: '{img.name}'");
            }
        }
        
        // UI初期化
        UpdateUI();
        
        Debug.Log("[MiniGameManager] UI初期化完了 - スコアと表示をリセットしました");
        
        // 装備表示も初期化
        Debug.Log("[MiniGameManager] 装備表示を初期化します");
        UpdateEquipmentDisplay();
    }
    
    /// <summary>
    /// UI要素の更新
    /// </summary>
    void UpdateUI()
    {
        if (player == null) return;
        
        // 残り時間計算
        float elapsed = Time.time - startTime;
        float remainingTime = Mathf.Max(0, gameTime - elapsed);
        
        // ゲージ更新
        if (gaugeSlider != null)
        {
            gaugeSlider.value = player.currentGauge;
        }
        
        // 時間表示更新
        if (timeText != null)
        {
            timeText.text = $"Time: {remainingTime:F0}";
        }
        
        // スコア表示は不要（ゲージ満タンでクリアのシンプル仕様）
        // if (scoreText != null && player != null)
        // {
        //     int totalScore = player.GetTotalScore();
        //     scoreText.text = $"Score: {totalScore}";
        // }
        
        // 装備アイコン表示更新
        UpdateEquipmentDisplay();
    }
    
    /// <summary>
    /// 装備表示の更新（右上アイコン）
    /// </summary>
    void UpdateEquipmentDisplay()
    {
        if (player == null) return;
        
        // 装備アイコンが未取得の場合、再検索
        if (equipmentIcon == null)
        {
            Image[] allImages = FindObjectsOfType<Image>();
            foreach (Image image in allImages)
            {
                if (image.name.Contains("Equipment"))
                {
                    equipmentIcon = image;
                    Debug.Log($"[MiniGameManager] 装備アイコンを再取得: {image.name}");
                    break;
                }
            }
        }
        
        // 装備テキストが未取得の場合、再検索
        if (equipmentText == null)
        {
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (Text text in allTexts)
            {
                if (text.name.Contains("Equipment"))
                {
                    equipmentText = text;
                    Debug.Log($"[MiniGameManager] 装備テキストを再取得: {text.name}");
                    break;
                }
            }
        }
        
        // プレイヤーの装備タイプを取得
        EquipmentData playerEquipment = EquipmentManager.GetEquipmentByType(player.playerEquipmentType);
        
        // 装備アイコン更新
        if (equipmentIcon != null)
        {
            if (playerEquipment.icon != null)
            {
                equipmentIcon.sprite = playerEquipment.icon;
                equipmentIcon.color = Color.white;
                Debug.Log($"[装備UI] アイコン更新: {playerEquipment.name}");
            }
            else
            {
                // スプライトがない場合は色で表示
                equipmentIcon.color = playerEquipment.color;
                Debug.Log($"[装備UI] 色更新: {playerEquipment.color}");
            }
        }
        else
        {
            Debug.LogWarning("[装備UI] 装備アイコンImage が見つかりません");
        }
        
        // 装備説明テキスト更新
        if (equipmentText != null)
        {
            equipmentText.text = $"目標: {playerEquipment.name}";
        }
        else
        {
            Debug.LogWarning("[装備UI] 装備テキスト が見つかりません");
        }
    }
    
    /// <summary>
    /// プレイヤー装備アイコンの初期設定
    /// </summary>
    void SetupPlayerEquipmentIcon()
    {
        // プレイヤー装備アイコンを自動作成
        if (playerEquipmentIcon == null)
        {
            // Canvas を検索
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // 新しいImageオブジェクトを作成
                GameObject iconObject = new GameObject("PlayerEquipmentIcon");
                iconObject.transform.SetParent(canvas.transform, false);
                
                // Image コンポーネント追加
                playerEquipmentIcon = iconObject.AddComponent<Image>();
                
                // RectTransform設定（右上の小さなアイコン）
                RectTransform rectTransform = playerEquipmentIcon.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(1, 1); // 右上アンカー
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(1, 1);
                rectTransform.anchoredPosition = new Vector2(-20, -20); // 右上から少し内側
                rectTransform.sizeDelta = new Vector2(50, 50); // 50x50のアイコン
                
                // デフォルト設定
                playerEquipmentIcon.color = Color.white;
                
                Debug.Log("[MiniGameManager] プレイヤー装備アイコンを作成しました");
            }
            else
            {
                Debug.LogWarning("[MiniGameManager] Canvasが見つからないため、装備アイコンを作成できません");
            }
        }
    }
    
    /// <summary>
    /// プレイヤーの装備アイコンを表示
    /// </summary>
    public void DisplayPlayerEquipmentIcon(EquipmentType equipmentType)
    {
        if (playerEquipmentIcon == null)
        {
            Debug.LogWarning("[MiniGameManager] プレイヤー装備アイコンが見つかりません");
            return;
        }
        
        // 装備データを取得
        EquipmentData equipmentData = EquipmentManager.GetEquipmentByType(equipmentType);
        if (equipmentData == null)
        {
            Debug.LogWarning($"[MiniGameManager] 装備データが見つかりません: {equipmentType}");
            return;
        }
        
        // アイコン画像を設定
        if (equipmentData.icon != null)
        {
            Debug.Log($"[MiniGameManager] プレイヤー装備アイコン設定: {equipmentData.name}");
            playerEquipmentIcon.sprite = equipmentData.icon;
            playerEquipmentIcon.color = Color.white; // アイコンがある場合は白色
        }
        else
        {
            // アイコンがない場合は色で表示
            Debug.Log($"[MiniGameManager] プレイヤー装備色設定: {equipmentData.color}");
            playerEquipmentIcon.sprite = null;
            playerEquipmentIcon.color = equipmentData.color;
        }
        
        // アイコンを表示
        playerEquipmentIcon.gameObject.SetActive(true);
    }
}