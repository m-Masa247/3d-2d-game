using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 迷路の環境（壁、床）の見た目を管理するシステム
/// マテリアルやテクスチャを一括変更可能
/// </summary>
public class MazeEnvironmentManager : MonoBehaviour
{
    [Header("環境設定")]
    [SerializeField] private bool autoDetectObjects = false; // オブジェクトの自動検出（一旦無効）
    [SerializeField] private bool applyOnStart = true; // 開始時に適用
    
    [Header("壁設定")]
    [SerializeField] private Material[] wallMaterials; // 壁素材配列
    [SerializeField] private string wallTag = "Wall"; // 壁のタグ
    [SerializeField] private int currentWallMaterialIndex = 0; // 現在の壁素材インデックス
    
    [Header("床設定")]
    [SerializeField] private Material[] floorMaterials; // 床素材配列
    [SerializeField] private string floorTag = "Floor"; // 床のタグ
    [SerializeField] private int currentFloorMaterialIndex = 0; // 現在の床素材インデックス
    
    [Header("天井設定")]
    [SerializeField] private Material[] ceilingMaterials; // 天井素材配列
    [SerializeField] private string ceilingTag = "Ceiling"; // 天井のタグ
    [SerializeField] private int currentCeilingMaterialIndex = 0; // 現在の天井素材インデックス
    
    [Header("テーマ設定")]
    [SerializeField] private EnvironmentTheme[] themes; // 環境テーマ
    [SerializeField] private int currentThemeIndex = 0; // 現在のテーマインデックス
    
    [Header("ライティング設定")]
    [SerializeField] private bool adjustLightingWithTheme = true; // テーマ連動照明
    [SerializeField] private Light[] targetLights; // 対象照明
    
    [Header("デバッグ設定")]
    [SerializeField] private bool showDebugInfo = true; // デバッグ情報
    [SerializeField] private bool showGizmos = false; // ギズモ表示
    
    private List<Renderer> wallRenderers = new List<Renderer>();
    private List<Renderer> floorRenderers = new List<Renderer>();
    private List<Renderer> ceilingRenderers = new List<Renderer>();
    
    [System.Serializable]
    public class EnvironmentTheme
    {
        public string themeName = "デフォルト"; // テーマ名
        public Material wallMaterial; // 壁素材
        public Material floorMaterial; // 床素材
        public Material ceilingMaterial; // 天井素材
        public Color lightColor = Color.white; // 照明色
        public float lightIntensity = 1f; // 照明強度
        public Color fogColor = Color.white; // フォグ色
        [Range(0f, 1f)]
        public float fogDensity = 0.01f; // フォグ濃度
    }
    
    private void Start()
    {
        // 一時的に環境管理を無効化（タグ問題回避）
        LogDebug("環境管理システム - 一時的に無効化中");
        return;
    }
    
    /// <summary>
    /// 環境オブジェクトの自動検出
    /// </summary>
    private void DetectEnvironmentObjects()
    {
        wallRenderers.Clear();
        floorRenderers.Clear();
        ceilingRenderers.Clear();
        
        // 壁の検出
        try
        {
            GameObject[] walls = GameObject.FindGameObjectsWithTag(wallTag);
            foreach (GameObject wall in walls)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    wallRenderers.Add(renderer);
                }
            }
        }
        catch (UnityException e)
        {
            LogDebug($"壁タグ '{wallTag}' が見つかりません: {e.Message}");
        }
        
        // 床の検出
        try
        {
            GameObject[] floors = GameObject.FindGameObjectsWithTag(floorTag);
            foreach (GameObject floor in floors)
            {
                Renderer renderer = floor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    floorRenderers.Add(renderer);
                }
            }
        }
        catch (UnityException e)
        {
            LogDebug($"床タグ '{floorTag}' が見つかりません: {e.Message}");
        }
        
        // 天井の検出
        try
        {
            GameObject[] ceilings = GameObject.FindGameObjectsWithTag(ceilingTag);
            foreach (GameObject ceiling in ceilings)
            {
                Renderer renderer = ceiling.GetComponent<Renderer>();
                if (renderer != null)
                {
                    ceilingRenderers.Add(renderer);
                }
            }
        }
        catch (UnityException e)
        {
            LogDebug($"天井タグ '{ceilingTag}' が見つかりません: {e.Message}");
        }
        
        LogDebug($"検出結果 - 壁:{wallRenderers.Count}個, 床:{floorRenderers.Count}個, 天井:{ceilingRenderers.Count}個");
    }
    
    /// <summary>
    /// 現在のテーマを適用
    /// </summary>
    public void ApplyCurrentTheme()
    {
        if (themes != null && themes.Length > 0 && currentThemeIndex < themes.Length)
        {
            ApplyTheme(themes[currentThemeIndex]);
        }
        else
        {
            // テーマが設定されていない場合は個別設定を適用
            ApplyWallMaterial(currentWallMaterialIndex);
            ApplyFloorMaterial(currentFloorMaterialIndex);
            ApplyCeilingMaterial(currentCeilingMaterialIndex);
        }
    }
    
    /// <summary>
    /// 特定テーマを適用
    /// </summary>
    /// <param name="theme">適用するテーマ</param>
    public void ApplyTheme(EnvironmentTheme theme)
    {
        if (theme == null) return;
        
        // 素材の適用
        if (theme.wallMaterial != null)
        {
            ApplyMaterialToRenderers(wallRenderers, theme.wallMaterial);
        }
        
        if (theme.floorMaterial != null)
        {
            ApplyMaterialToRenderers(floorRenderers, theme.floorMaterial);
        }
        
        if (theme.ceilingMaterial != null)
        {
            ApplyMaterialToRenderers(ceilingRenderers, theme.ceilingMaterial);
        }
        
        // ライティング調整
        if (adjustLightingWithTheme)
        {
            AdjustLighting(theme.lightColor, theme.lightIntensity);
            AdjustFog(theme.fogColor, theme.fogDensity);
        }
        
        LogDebug($"テーマ '{theme.themeName}' を適用しました");
    }
    
    /// <summary>
    /// 壁素材を適用
    /// </summary>
    /// <param name="materialIndex">素材インデックス</param>
    public void ApplyWallMaterial(int materialIndex)
    {
        if (wallMaterials != null && materialIndex < wallMaterials.Length)
        {
            currentWallMaterialIndex = materialIndex;
            ApplyMaterialToRenderers(wallRenderers, wallMaterials[materialIndex]);
            LogDebug($"壁素材 {materialIndex} を適用");
        }
    }
    
    /// <summary>
    /// 床素材を適用
    /// </summary>
    /// <param name="materialIndex">素材インデックス</param>
    public void ApplyFloorMaterial(int materialIndex)
    {
        if (floorMaterials != null && materialIndex < floorMaterials.Length)
        {
            currentFloorMaterialIndex = materialIndex;
            ApplyMaterialToRenderers(floorRenderers, floorMaterials[materialIndex]);
            LogDebug($"床素材 {materialIndex} を適用");
        }
    }
    
    /// <summary>
    /// 天井素材を適用
    /// </summary>
    /// <param name="materialIndex">素材インデックス</param>
    public void ApplyCeilingMaterial(int materialIndex)
    {
        if (ceilingMaterials != null && materialIndex < ceilingMaterials.Length)
        {
            currentCeilingMaterialIndex = materialIndex;
            ApplyMaterialToRenderers(ceilingRenderers, ceilingMaterials[materialIndex]);
            LogDebug($"天井素材 {materialIndex} を適用");
        }
    }
    
    /// <summary>
    /// レンダラー群に素材を適用
    /// </summary>
    /// <param name="renderers">対象レンダラー</param>
    /// <param name="material">適用素材</param>
    private void ApplyMaterialToRenderers(List<Renderer> renderers, Material material)
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }
    }
    
    /// <summary>
    /// ライティング調整
    /// </summary>
    /// <param name="color">光の色</param>
    /// <param name="intensity">光の強度</param>
    private void AdjustLighting(Color color, float intensity)
    {
        if (targetLights == null) return;
        
        foreach (Light light in targetLights)
        {
            if (light != null)
            {
                light.color = color;
                light.intensity = intensity;
            }
        }
    }
    
    /// <summary>
    /// フォグ調整
    /// </summary>
    /// <param name="color">フォグ色</param>
    /// <param name="density">フォグ濃度</param>
    private void AdjustFog(Color color, float density)
    {
        RenderSettings.fog = density > 0f;
        RenderSettings.fogColor = color;
        RenderSettings.fogDensity = density;
    }
    
    /// <summary>
    /// 次のテーマに切り替え
    /// </summary>
    public void NextTheme()
    {
        if (themes != null && themes.Length > 0)
        {
            currentThemeIndex = (currentThemeIndex + 1) % themes.Length;
            ApplyCurrentTheme();
        }
    }
    
    /// <summary>
    /// 次の壁素材に切り替え
    /// </summary>
    public void NextWallMaterial()
    {
        if (wallMaterials != null && wallMaterials.Length > 0)
        {
            int nextIndex = (currentWallMaterialIndex + 1) % wallMaterials.Length;
            ApplyWallMaterial(nextIndex);
        }
    }
    
    /// <summary>
    /// 次の床素材に切り替え
    /// </summary>
    public void NextFloorMaterial()
    {
        if (floorMaterials != null && floorMaterials.Length > 0)
        {
            int nextIndex = (currentFloorMaterialIndex + 1) % floorMaterials.Length;
            ApplyFloorMaterial(nextIndex);
        }
    }
    
    /// <summary>
    /// デバッグログ出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    private void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[環境管理] {message}");
        }
    }
    
    /// <summary>
    /// ギズモ描画
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // 壁の表示
        Gizmos.color = Color.red;
        foreach (Renderer renderer in wallRenderers)
        {
            if (renderer != null)
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
        
        // 床の表示
        Gizmos.color = Color.green;
        foreach (Renderer renderer in floorRenderers)
        {
            if (renderer != null)
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
        
        // 天井の表示
        Gizmos.color = Color.blue;
        foreach (Renderer renderer in ceilingRenderers)
        {
            if (renderer != null)
            {
                Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
            }
        }
    }
}