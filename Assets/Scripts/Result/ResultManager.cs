using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

// ゲームクリア画面管理
public class ResultManager : MonoBehaviour
{
    [Header("演出設定")]
    [SerializeField] private float celebrationDuration = 3f;
    
    [Header("UI要素")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text instructionText;
    [SerializeField] private Image robotImage; // ロボット画像追加
    
    [Header("背景画像設定")]
    [SerializeField] private Image backgroundImage; // 背景画像
    [SerializeField] private string backgroundImageName; // 背景画像ファイル名
    [SerializeField] private Color defaultBackgroundColor = new Color(0.2f, 0.6f, 0.2f, 1f); // デフォルト背景色（クリア用緑系）
    
    private bool canClick = false; // クリック有効化フラグ
    
    private void Start()
    {
        StartCoroutine(ShowResultSequence());
    }
    
    private void Update()
    {
        // クリック/タップ検出
        if (canClick && (Input.GetMouseButtonDown(0) || Input.touchCount > 0))
        {
            RestartGame();
        }
    }
    
    private IEnumerator ShowResultSequence()
    {
        // UI初期化
        InitializeUI();
        
        // 背景画像設定
        SetupBackground();
        
        // ゲームクリア演出
        Debug.Log("🎉🎉🎉 ゲームクリア！ 🎉🎉🎉");
        if (titleText != null) titleText.text = "🎉 ゲームクリア！ 🎉";
        
        yield return new WaitForSeconds(1f);
        
        Debug.Log("迷路を突破しました！");
        if (messageText != null) messageText.text = "迷路を突破しました！\n素晴らしい冒険でした！";
        
        // お祝い演出時間
        yield return new WaitForSeconds(celebrationDuration);
        
        // クリック案内を表示
        Debug.Log("画面をクリックでタイトル画面に戻ります");
        if (instructionText != null) instructionText.text = "画面をクリック/タップでタイトル画面へ";
        
        // クリック有効化
        canClick = true;
        
        Debug.Log("[ゲーム終了まで少々お待ちください...]");
    }    
    /// <summary>
    /// ゲーム結果を確認
    /// </summary>
    private bool CheckGameResult()
    {
        // PlayerPositionManagerから結果を取得
        return PlayerPositionManager.GetGameResult();
    }
    
    /// <summary>
    /// 背景画像を設定
    /// </summary>
    private void SetupBackground()
    {
        if (backgroundImage != null)
        {
            LoadBackgroundImage();
            SetupBackgroundLayout();
        }
        else
        {
            // 背景画像を自動作成
            CreateBackgroundImage();
        }
    }
    
    /// <summary>
    /// 背景画像を自動作成
    /// </summary>
    private void CreateBackgroundImage()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[ResultManager] Canvasが見つからないため、背景画像を作成できません");
            return;
        }
        
        // 背景画像オブジェクトを作成
        GameObject backgroundObj = new GameObject("BackgroundImage");
        backgroundObj.transform.SetParent(canvas.transform, false);
        
        // Imageコンポーネント追加
        backgroundImage = backgroundObj.AddComponent<Image>();
        
        // 最背面に設定するため、最初の子として追加
        backgroundObj.transform.SetAsFirstSibling();
        
        // ゲーム結果に応じて背景設定
        LoadBackgroundImage();
        SetupBackgroundLayout();
        
        Debug.Log("[ResultManager] 背景画像を自動作成しました");
    }
    
    /// <summary>
    /// 背景画像を読み込み
    /// </summary>
    private void LoadBackgroundImage()
    {
        if (backgroundImage == null) return;
        
        // 背景画像名が設定されていない場合はデフォルト名を使用
        if (string.IsNullOrEmpty(backgroundImageName))
        {
            backgroundImageName = "result_bg";
            Debug.Log($"[ResultManager] 背景画像名が未設定のため、デフォルト名を使用: {backgroundImageName}");
        }
        
        // デバッグ: Resources フォルダ内の Backgrounds を確認
        Debug.Log($"[ResultManager] 背景画像読み込み開始: {backgroundImageName}");
        
        // 全てのBackgroundsフォルダ内容を確認
        Object[] allBackgroundAssets = Resources.LoadAll("Backgrounds");
        Debug.Log($"[ResultManager] Backgroundsフォルダ内のアセット数: {allBackgroundAssets.Length}");
        
        foreach (Object asset in allBackgroundAssets)
        {
            Debug.Log($"  - アセット発見: {asset.name} (型: {asset.GetType()})");
        }
        
        // 拡張子なしで試行
        Sprite backgroundSprite = Resources.Load<Sprite>($"Backgrounds/{backgroundImageName}");
        
        // 見つからない場合は拡張子付きで試行
        if (backgroundSprite == null)
        {
            Debug.LogWarning($"[ResultManager] 拡張子なしで見つからず。拡張子付きで再試行...");
            backgroundSprite = Resources.Load<Sprite>($"Backgrounds/{backgroundImageName}.png");
        }
        
        if (backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white;
            Debug.Log($"[ResultManager] 背景画像読み込み成功: {backgroundImageName}");
        }
        else
        {
            // 画像がない場合はデフォルト色で表示
            backgroundImage.sprite = null;
            backgroundImage.color = defaultBackgroundColor;
            Debug.LogWarning($"[ResultManager] 背景画像が見つかりません: {backgroundImageName} - デフォルト色で表示");
            
            // さらにデバッグ: Textureとして読み込めるか確認
            Texture2D backgroundTexture = Resources.Load<Texture2D>($"Backgrounds/{backgroundImageName}");
            if (backgroundTexture != null)
            {
                Debug.LogError($"[ResultManager] Textureとしては読み込める！Spriteインポート設定を確認してください: {backgroundImageName}");
            }
        }
    }
    
    /// <summary>
    /// 背景画像のレイアウト設定
    /// </summary>
    private void SetupBackgroundLayout()
    {
        if (backgroundImage == null) return;
        
        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
        if (backgroundRect != null)
        {
            // アンカーを全画面に設定
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            
            // 位置とサイズをリセット（全画面をカバー）
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            
            Debug.Log("[ResultManager] 背景画像レイアウト設定完了");
        }
    }    
    private void LoadRobotImage()
    {
        if (robotImage != null)
        {
            Sprite robotSprite = Resources.Load<Sprite>("Illustrations/robot");
            if (robotSprite != null)
            {
                robotImage.sprite = robotSprite;
                robotImage.color = Color.white;
                Debug.Log("[ResultManager] robot.png読み込み成功");
            }
            else
            {
                Debug.LogWarning("[ResultManager] robot.pngが見つかりません");
            }
        }
    }
    
    private void InitializeUI()
    {
        // UI要素が設定されていない場合の自動検索
        if (titleText == null)
        {
            GameObject titleObj = GameObject.Find("TitleText");
            if (titleObj != null) titleText = titleObj.GetComponent<Text>();
        }
        
        if (messageText == null)
        {
            GameObject messageObj = GameObject.Find("MessageText");
            if (messageObj != null) messageText = messageObj.GetComponent<Text>();
        }
        
        if (instructionText == null)
        {
            GameObject instructionObj = GameObject.Find("InstructionText");
            if (instructionObj != null) instructionText = instructionObj.GetComponent<Text>();
        }
        
        if (robotImage == null)
        {
            GameObject robotObj = GameObject.Find("RobotImage");
            if (robotObj != null) robotImage = robotObj.GetComponent<Image>();
        }
        
        if (backgroundImage == null)
        {
            GameObject backgroundObj = GameObject.Find("BackgroundImage");
            if (backgroundObj != null) backgroundImage = backgroundObj.GetComponent<Image>();
        }
        
        // ロボット画像を読み込み
        LoadRobotImage();
        
        // UI要素の自動レイアウト調整
        SetupUILayout();
        
        // 初期状態では空にする
        if (titleText != null) titleText.text = "";
        if (messageText != null) messageText.text = "";
        if (instructionText != null) instructionText.text = "";
    }
    
    private void SetupUILayout()
    {
        // 画面サイズを取得
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;
        
        // タイトルテキストの調整
        if (titleText != null)
        {
            RectTransform titleRect = titleText.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                // アンカーを top-center に設定
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                
                // 位置とサイズを設定
                titleRect.anchoredPosition = new Vector2(0, -50);
                titleRect.sizeDelta = new Vector2(screenWidth * 0.8f, 80);
                
                // フォント設定
                titleText.fontSize = (int)Mathf.Clamp(screenHeight / 20, 24, 48);
                titleText.alignment = TextAnchor.MiddleCenter;
                // titleText.colorはInspectorで設定した色を使用
            }
        }
        
        // メッセージテキストの調整
        if (messageText != null)
        {
            RectTransform messageRect = messageText.GetComponent<RectTransform>();
            if (messageRect != null)
            {
                // アンカーを middle-center に設定
                messageRect.anchorMin = new Vector2(0.5f, 0.5f);
                messageRect.anchorMax = new Vector2(0.5f, 0.5f);
                messageRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 位置とサイズを設定
                messageRect.anchoredPosition = new Vector2(0, 0);
                messageRect.sizeDelta = new Vector2(screenWidth * 0.7f, 120);
                
                // フォント設定
                messageText.fontSize = (int)Mathf.Clamp(screenHeight / 30, 18, 32);
                messageText.alignment = TextAnchor.MiddleCenter;
                // messageText.colorはInspectorで設定した色を使用
            }
        }
        
        // 操作案内テキストの調整
        if (instructionText != null)
        {
            RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
            if (instructionRect != null)
            {
                // アンカーを bottom-center に設定
                instructionRect.anchorMin = new Vector2(0.5f, 0f);
                instructionRect.anchorMax = new Vector2(0.5f, 0f);
                instructionRect.pivot = new Vector2(0.5f, 0f);
                
                // 位置とサイズを設定
                instructionRect.anchoredPosition = new Vector2(0, 50);
                instructionRect.sizeDelta = new Vector2(screenWidth * 0.9f, 60);
                
                // フォント設定
                instructionText.fontSize = (int)Mathf.Clamp(screenHeight / 40, 14, 24);
                instructionText.alignment = TextAnchor.MiddleCenter;
                // instructionText.colorはInspectorで設定した色を使用
            }
        }
        
        // ロボット画像の調整
        if (robotImage != null)
        {
            RectTransform robotRect = robotImage.GetComponent<RectTransform>();
            if (robotRect != null)
            {
                // アンカーを center に設定
                robotRect.anchorMin = new Vector2(0.5f, 0.5f);
                robotRect.anchorMax = new Vector2(0.5f, 0.5f);
                robotRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 位置とサイズを設定（画面の右側）
                robotRect.anchoredPosition = new Vector2(screenWidth * 0.25f, -50);
                float robotSize = Mathf.Min(screenWidth * 0.3f, screenHeight * 0.4f);
                robotRect.sizeDelta = new Vector2(robotSize, robotSize);
                
                Debug.Log($"[ResultManager] ロボット画像レイアウト設定完了: サイズ{robotSize}px");
            }
        }
        
        Debug.Log($"[ResultManager] UI要素を画面サイズ {screenWidth}x{screenHeight} に合わせて調整しました");
    }
    
    /// <summary>
    /// 背景画像を手動で設定するメソッド（Inspectorで使用）
    /// </summary>
    public void SetBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null && sprite != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.color = Color.white;
            Debug.Log($"[ResultManager] 背景画像を手動設定: {sprite.name}");
        }
    }
    
    /// <summary>
    /// 背景色を手動で設定するメソッド（Inspectorで使用）
    /// </summary>
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = null;
            backgroundImage.color = color;
            Debug.Log($"[ResultManager] 背景色を手動設定: {color}");
        }
    }
    
    // UIボタン用メソッド（後でUIが追加された時用）
    public void RestartGame()
    {
        if (!canClick) return; // クリック有効化前は無効
        
        Debug.Log("ゲームを再開します!");
        
        // 位置情報をクリア（新しいゲーム用）
        PlayerPositionManager.ClearSavedPosition();
        PlayerPrefs.DeleteKey("DeleteTargetEnemy");
        
        SceneManager.LoadScene("TitleScene");
    }
    
    public void QuitGame()
    {
        Debug.Log("ゲームを終了します!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}