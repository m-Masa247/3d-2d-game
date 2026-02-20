using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// タイトル画面管理（title.png統合版）
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button startButton;
    [SerializeField] private Text startButtonText;
    [SerializeField] private Text versionText;
    
    [Header("演出設定")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float buttonBlinkSpeed = 2f;
    
    private bool canStart = false;
    
    private void Start()
    {
        StartCoroutine(InitializeTitle());
    }
    
    private void Update()
    {
        // エンターキーでもゲーム開始
        if (canStart && Input.GetKeyDown(KeyCode.Return))
        {
            StartGame();
        }
        
        // スタートボタンの点滅効果
        if (canStart && startButtonText != null)
        {
            float alpha = Mathf.PingPong(Time.time * buttonBlinkSpeed, 1f);
            Color color = startButtonText.color;
            color.a = Mathf.Clamp(alpha + 0.3f, 0.3f, 1f); // 最小透明度30%
            startButtonText.color = color;
        }
    }
    
    private System.Collections.IEnumerator InitializeTitle()
    {
        // UI初期化
        SetupUI();
        
        // 背景画像のフェードイン
        if (backgroundImage != null)
        {
            LoadTitleBackground();
            yield return StartCoroutine(FadeInBackground());
        }
        
        // スタートボタン有効化
        canStart = true;
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
            startButton.interactable = true;
        }
        
        Debug.Log("[タイトル] 初期化完了");
    }
    
    private void SetupUI()
    {
        // UI要素の自動検索
        if (backgroundImage == null)
        {
            GameObject bgObj = GameObject.Find("BackgroundImage");
            if (bgObj != null) backgroundImage = bgObj.GetComponent<Image>();
        }
        
        if (startButton == null)
        {
            GameObject buttonObj = GameObject.Find("StartButton");
            if (buttonObj != null) startButton = buttonObj.GetComponent<Button>();
        }
        
        if (startButtonText == null && startButton != null)
        {
            startButtonText = startButton.GetComponentInChildren<Text>();
        }
        
        if (versionText == null)
        {
            GameObject versionObj = GameObject.Find("VersionText");
            if (versionObj != null) versionText = versionObj.GetComponent<Text>();
        }
        
        // 初期設定
        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = 0f; // 透明から開始
            backgroundImage.color = color;
        }
        
        if (startButtonText != null)
        {
            startButtonText.text = "PRESS START";
        }
        
        if (versionText != null)
        {
            versionText.text = "Yaminabe Game v1.0";
        }
    }
    
    private void LoadTitleBackground()
    {
        Sprite titleSprite = Resources.Load<Sprite>("Illustrations/title");
        if (titleSprite != null && backgroundImage != null)
        {
            backgroundImage.sprite = titleSprite;
            Debug.Log("[タイトル] title.png読み込み成功");
        }
        else
        {
            Debug.LogWarning("[タイトル] title.pngが見つかりません");
        }
    }
    
    private System.Collections.IEnumerator FadeInBackground()
    {
        if (backgroundImage == null) yield break;
        
        Color startColor = backgroundImage.color;
        Color endColor = startColor;
        endColor.a = 1f;
        
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeInDuration;
            backgroundImage.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }
        
        backgroundImage.color = endColor;
    }
    
    private void StartGame()
    {
        if (!canStart) return;
        
        canStart = false; // 重複クリック防止
        Debug.Log("[タイトル] ゲーム開始！");
        
        // ゲーム開始前にデータをクリア
        PlayerPositionManager.ClearSavedPosition();
        PlayerPrefs.DeleteKey("DeleteTargetEnemy");
        
        SceneManager.LoadScene("MazeScene");
    }
}