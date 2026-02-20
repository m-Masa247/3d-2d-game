using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ミニゲームのUI管理と背景設定
/// </summary>
public class MiniGameUI : MonoBehaviour
{
    [Header("背景設定")]
    public Color backgroundColor = new Color(0.2f, 0.3f, 0.6f, 1f); // 青っぽい背景
    public bool useGradient = true;
    public Color gradientTopColor = new Color(0.1f, 0.2f, 0.8f, 1f);
    public Color gradientBottomColor = new Color(0.6f, 0.4f, 0.8f, 1f);
    
    [Header("UI要素")]
    public Canvas mainCanvas;
    public Image backgroundImage; // 背景用Image
    
    void Start()
    {
        SetupBackground();
        SetupUI();
    }
    
    /// <summary>
    /// 背景設定
    /// </summary>
    void SetupBackground()
    {
        // カメラの背景色設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        
        // Canvas背景の設定
        SetupCanvasBackground();
    }
    
    /// <summary>
    /// Canvas背景の設定
    /// </summary>
    void SetupCanvasBackground()
    {
        // Canvas背景は使用しない（3Dオブジェクトを隠すため）
        // カメラの backgroundColor のみ使用
        Debug.Log("[MiniGameUI] Canvas背景をスキップ - 3Dオブジェクト表示優先");
        
        /*
        // Canvas自動取得
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
        
        if (mainCanvas != null)
        {
            // 背景用Imageを作成
            if (backgroundImage == null)
            {
                GameObject bgObject = new GameObject("Background");
                bgObject.transform.SetParent(mainCanvas.transform, false);
                
                backgroundImage = bgObject.AddComponent<Image>();
                RectTransform bgRect = bgObject.GetComponent<RectTransform>();
                
                // フルスクリーン設定
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                
                // 背景を最背面に
                bgObject.transform.SetAsFirstSibling();
            }
            
            // 背景色設定
            if (useGradient)
            {
                // グラデーション（簡易版：単色で代用）
                backgroundImage.color = gradientTopColor;
            }
            else
            {
                backgroundImage.color = backgroundColor;
            }
        }
        */
        
        Debug.Log("[MiniGameUI] 背景設定完了");
    }
    
    /// <summary>
    /// UI要素の配置調整
    /// </summary>
    void SetupUI()
    {
        // 自動的にUI要素を整列
        OrganizeUIElements();
    }
    
    /// <summary>
    /// UI要素の自動整列
    /// </summary>
    void OrganizeUIElements()
    {
        if (mainCanvas == null) return;
        
        // Sliderを上部に配置
        Slider[] sliders = FindObjectsOfType<Slider>();
        foreach (Slider slider in sliders)
        {
            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                // アンカーを上部中央に
                sliderRect.anchorMin = new Vector2(0.1f, 0.9f);
                sliderRect.anchorMax = new Vector2(0.9f, 0.95f);
                sliderRect.offsetMin = Vector2.zero;
                sliderRect.offsetMax = Vector2.zero;
            }
        }
        
        // Text要素を適切に配置
        Text[] texts = FindObjectsOfType<Text>();
        int textIndex = 0;
        foreach (Text text in texts)
        {
            if (text.name.Contains("Time") || text.text.Contains("Time"))
            {
                // 時間表示：左上
                PositionText(text, new Vector2(0f, 1f), new Vector2(0.3f, 1f), new Vector2(10, -10));
            }
            else if (text.name.Contains("Score") || text.text.Contains("Score"))
            {
                // スコア表示：左上（時間の下）
                PositionText(text, new Vector2(0f, 1f), new Vector2(0.3f, 1f), new Vector2(10, -60));
            }
            else if (text.name.Contains("Equipment") || text.text.Contains("目標"))
            {
                // 装備説明：右上（アイコンの下）
                PositionText(text, new Vector2(0.7f, 1f), new Vector2(1f, 1f), new Vector2(-10, -60));
            }
            textIndex++;
        }
        
        // 装備アイコンUI自動作成
        CreateEquipmentIconUI();
        
        Debug.Log("[MiniGameUI] UI要素配置完了");
    }
    
    /// <summary>
    /// テキスト位置設定ヘルパー
    /// </summary>
    void PositionText(Text text, Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
    {
        RectTransform textRect = text.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = anchorMin;
            textRect.anchorMax = anchorMax;
            textRect.anchoredPosition = position;
            textRect.sizeDelta = new Vector2(200, 50);
        }
    }
    
    /// <summary>
    /// 装備アイコンUI自動作成
    /// </summary>
    void CreateEquipmentIconUI()
    {
        if (mainCanvas == null) return;
        
        // 常に新しく装備アイコンを作成
        Debug.Log("[MiniGameUI] 装備アイコンUIを作成中...");
        
        // 装備アイコン用UI作成
        GameObject equipmentIconObj = new GameObject("EquipmentIcon");
        equipmentIconObj.transform.SetParent(mainCanvas.transform, false);
        
        Image equipmentIcon = equipmentIconObj.AddComponent<Image>();
        RectTransform iconRect = equipmentIconObj.GetComponent<RectTransform>();
        
        // 右上配置
        iconRect.anchorMin = new Vector2(1f, 1f);
        iconRect.anchorMax = new Vector2(1f, 1f);
        iconRect.pivot = new Vector2(1f, 1f);
        iconRect.anchoredPosition = new Vector2(-10, -10);
        iconRect.sizeDelta = new Vector2(60, 60); // 少し大きめに変更
        
        // 初期設定（仮のスプライトまたは色）
        equipmentIcon.color = Color.white;
        
        // テスト用の色設定
        equipmentIcon.color = Color.red;
        
        Debug.Log("[MiniGameUI] 装備アイコンUIを作成完了");
        
        // 装備説明テキストも作成
        CreateEquipmentTextUI();
    }
    
    /// <summary>
    /// 装備説明テキストUI作成
    /// </summary>
    void CreateEquipmentTextUI()
    {
        if (mainCanvas == null) return;
        
        // 装備説明テキスト作成
        GameObject equipmentTextObj = new GameObject("EquipmentText");
        equipmentTextObj.transform.SetParent(mainCanvas.transform, false);
        
        Text equipmentText = equipmentTextObj.AddComponent<Text>();
        RectTransform textRect = equipmentTextObj.GetComponent<RectTransform>();
        
        // フォント設定
        equipmentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        equipmentText.fontSize = 14;
        equipmentText.color = Color.white;
        equipmentText.alignment = TextAnchor.UpperRight;
        equipmentText.text = "目標: 装備";
        
        // 右上配置（アイコンの下）
        textRect.anchorMin = new Vector2(1f, 1f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(1f, 1f);
        textRect.anchoredPosition = new Vector2(-10, -55);
        textRect.sizeDelta = new Vector2(100, 30);
        
        Debug.Log("[MiniGameUI] 装備説明テキストUIを自動作成");
    }
}