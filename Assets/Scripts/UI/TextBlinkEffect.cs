using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UIテキストを点滅させるエフェクトスクリプト
/// ゲームスタートボタンなどのクリックを促すために使用
/// </summary>
public class TextBlinkEffect : MonoBehaviour
{
    [Header("点滅設定")]
    [SerializeField] private float blinkSpeed = 1.0f; // 点滅の速度
    [SerializeField] private float minAlpha = 0.3f;   // 最小の透明度
    [SerializeField] private float maxAlpha = 1.0f;   // 最大の透明度
    [SerializeField] private bool autoStart = true;   // 自動開始
    
    [Header("詳細設定")]
    [SerializeField] private bool useSmooth = true;   // スムーズな点滅（Sin関数使用）
    [SerializeField] private float fadeDelay = 0f;   // 開始遅延時間
    
    private Text targetText;
    private Image targetImage;
    private Color originalTextColor;
    private Color originalImageColor;
    private bool isBlinking = false;
    private Coroutine blinkCoroutine;
    
    private void Awake()
    {
        // Text または Image コンポーネントを取得
        targetText = GetComponent<Text>();
        targetImage = GetComponent<Image>();
        
        // 元の色を保存
        if (targetText != null)
        {
            originalTextColor = targetText.color;
        }
        if (targetImage != null)
        {
            originalImageColor = targetImage.color;
        }
    }
    
    private void Start()
    {
        if (autoStart)
        {
            StartBlinking();
        }
    }
    
    private void OnEnable()
    {
        // オブジェクトが有効になった時に自動開始
        if (autoStart && !isBlinking)
        {
            StartBlinking();
        }
    }
    
    private void OnDisable()
    {
        // オブジェクトが無効になった時に停止
        StopBlinking();
    }
    
    /// <summary>
    /// 点滅を開始する
    /// </summary>
    public void StartBlinking()
    {
        if (isBlinking) return;
        
        isBlinking = true;
        
        if (fadeDelay > 0f)
        {
            StartCoroutine(DelayedStart());
        }
        else
        {
            blinkCoroutine = StartCoroutine(BlinkingCoroutine());
        }
    }
    
    /// <summary>
    /// 点滅を停止する
    /// </summary>
    public void StopBlinking()
    {
        if (!isBlinking) return;
        
        isBlinking = false;
        
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // 元の透明度に戻す
        SetAlpha(maxAlpha);
    }
    
    /// <summary>
    /// 遅延開始用のコルーチン
    /// </summary>
    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(fadeDelay);
        blinkCoroutine = StartCoroutine(BlinkingCoroutine());
    }
    
    /// <summary>
    /// 点滅処理のメインコルーチン
    /// </summary>
    private IEnumerator BlinkingCoroutine()
    {
        float time = 0f;
        
        while (isBlinking)
        {
            float alpha;
            
            if (useSmooth)
            {
                // Sin関数を使用したスムーズな点滅
                alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(time * blinkSpeed * 2f * Mathf.PI) + 1f) / 2f);
            }
            else
            {
                // 線形な点滅
                float pingPong = Mathf.PingPong(time * blinkSpeed, 1f);
                alpha = Mathf.Lerp(minAlpha, maxAlpha, pingPong);
            }
            
            SetAlpha(alpha);
            
            time += Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// 透明度を設定する
    /// </summary>
    /// <param name="alpha">透明度（0-1）</param>
    private void SetAlpha(float alpha)
    {
        if (targetText != null)
        {
            Color color = originalTextColor;
            color.a = alpha;
            targetText.color = color;
        }
        
        if (targetImage != null)
        {
            Color color = originalImageColor;
            color.a = alpha;
            targetImage.color = color;
        }
    }
    
    /// <summary>
    /// 点滅速度を動的に変更
    /// </summary>
    /// <param name="newSpeed">新しい速度</param>
    public void SetBlinkSpeed(float newSpeed)
    {
        blinkSpeed = newSpeed;
    }
    
    /// <summary>
    /// 透明度の範囲を動的に変更
    /// </summary>
    /// <param name="min">最小透明度</param>
    /// <param name="max">最大透明度</param>
    public void SetAlphaRange(float min, float max)
    {
        minAlpha = Mathf.Clamp01(min);
        maxAlpha = Mathf.Clamp01(max);
    }
    
    /// <summary>
    /// 点滅中かどうかを取得
    /// </summary>
    public bool IsBlinking => isBlinking;
}