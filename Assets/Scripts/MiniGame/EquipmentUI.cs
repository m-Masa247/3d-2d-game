using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MiniGameでの装備アイコン表示システム
/// </summary>
public class EquipmentUI : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private Image playerEquipmentIcon;
    [SerializeField] private Text equipmentNameText;
    [SerializeField] private Text gaugeText;
    [SerializeField] private Slider gaugeSlider;
    
    [Header("装備通知")]
    [SerializeField] private GameObject collectNotification;
    [SerializeField] private Image collectedIcon;
    [SerializeField] private Text collectedText;
    [SerializeField] private float notificationDuration = 2f;
    
    private MiniGamePlayer player;
    private Coroutine notificationCoroutine;
    
    private void Start()
    {
        player = FindObjectOfType<MiniGamePlayer>();
        InitializeUI();
    }
    
    private void Update()
    {
        UpdateGaugeDisplay();
    }
    
    private void InitializeUI()
    {
        // UI要素の自動検索
        FindUIElements();
        
        // プレイヤー装備アイコンを設定
        if (player != null)
        {
            SetPlayerEquipmentIcon(player.playerEquipmentType);
        }
        
        // 通知パネルを非表示
        if (collectNotification != null)
        {
            collectNotification.SetActive(false);
        }
    }
    
    private void FindUIElements()
    {
        if (playerEquipmentIcon == null)
        {
            GameObject iconObj = GameObject.Find("PlayerEquipmentIcon");
            if (iconObj != null) playerEquipmentIcon = iconObj.GetComponent<Image>();
        }
        
        if (equipmentNameText == null)
        {
            GameObject nameObj = GameObject.Find("EquipmentNameText");
            if (nameObj != null) equipmentNameText = nameObj.GetComponent<Text>();
        }
        
        if (gaugeText == null)
        {
            GameObject gaugeObj = GameObject.Find("GaugeText");
            if (gaugeObj != null) gaugeText = gaugeObj.GetComponent<Text>();
        }
        
        if (gaugeSlider == null)
        {
            GameObject sliderObj = GameObject.Find("GaugeSlider");
            if (sliderObj != null) gaugeSlider = sliderObj.GetComponent<Slider>();
        }
        
        if (collectNotification == null)
        {
            collectNotification = GameObject.Find("CollectNotification");
        }
        
        if (collectedIcon == null && collectNotification != null)
        {
            collectedIcon = collectNotification.transform.Find("CollectedIcon")?.GetComponent<Image>();
        }
        
        if (collectedText == null && collectNotification != null)
        {
            collectedText = collectNotification.transform.Find("CollectedText")?.GetComponent<Text>();
        }
    }
    
    private void SetPlayerEquipmentIcon(EquipmentType equipmentType)
    {
        EquipmentData data = EquipmentManager.GetEquipmentByType(equipmentType);
        
        if (playerEquipmentIcon != null && data.icon != null)
        {
            playerEquipmentIcon.sprite = data.icon;
            playerEquipmentIcon.color = Color.white;
        }
        
        if (equipmentNameText != null)
        {
            equipmentNameText.text = $"必要な装備: {data.name}";
            equipmentNameText.color = data.color;
        }
    }
    
    private void UpdateGaugeDisplay()
    {
        if (player == null) return;
        
        // ゲージテキスト更新
        if (gaugeText != null)
        {
            gaugeText.text = $"ゲージ: {player.currentGauge:F0}/{player.maxGauge:F0}";
        }
        
        // ゲージスライダー更新
        if (gaugeSlider != null)
        {
            gaugeSlider.value = player.currentGauge / player.maxGauge;
        }
    }
    
    public void ShowCollectNotification(EquipmentType collectedType)
    {
        if (collectNotification == null) return;
        
        EquipmentData data = EquipmentManager.GetEquipmentByType(collectedType);
        
        // アイコン設定
        if (collectedIcon != null && data.icon != null)
        {
            collectedIcon.sprite = data.icon;
            collectedIcon.color = Color.white;
        }
        
        // テキスト設定
        if (collectedText != null)
        {
            collectedText.text = $"{data.name} 獲得！";
            collectedText.color = data.color;
        }
        
        // 通知表示
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        notificationCoroutine = StartCoroutine(ShowNotificationCoroutine());
    }
    
    private System.Collections.IEnumerator ShowNotificationCoroutine()
    {
        collectNotification.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        collectNotification.SetActive(false);
    }
}