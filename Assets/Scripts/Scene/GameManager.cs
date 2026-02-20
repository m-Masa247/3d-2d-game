using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の統合管理システム
/// BGM、アセット読み込み、初期化を一元管理
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("システム設定")]
    [SerializeField] private bool autoCreateBGMManager = true;
    [SerializeField] private bool autoCreateEquipmentUI = true;
    
    private static GameManager instance;
    
    private void Awake()
    {
        // シングルトンパターン
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameSystems();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void InitializeGameSystems()
    {
        Debug.Log("[GameManager] ゲームシステム初期化開始");
        
        // BGMManager自動作成
        if (autoCreateBGMManager)
        {
            CreateBGMManagerIfNeeded();
        }
        
        Debug.Log("[GameManager] ゲームシステム初期化完了");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] シーン読み込み: {scene.name}");
        
        // 各シーンに応じた初期化
        switch (scene.name)
        {
            case "TitleScene":
                InitializeTitleScene();
                break;
            case "MazeScene":
                InitializeMazeScene();
                break;
            case "MiniGameScene":
                InitializeMiniGameScene();
                break;
            case "ResultScene":
                InitializeResultScene();
                break;
        }
    }
    
    private void CreateBGMManagerIfNeeded()
    {
        if (FindObjectOfType<BGMManager>() == null)
        {
            GameObject bgmObj = new GameObject("BGMManager");
            bgmObj.AddComponent<BGMManager>();
            Debug.Log("[GameManager] BGMManagerを自動作成");
        }
    }
    
    private void InitializeTitleScene()
    {
        // TitleManagerが存在しない場合は自動作成
        if (FindObjectOfType<TitleManager>() == null)
        {
            GameObject titleObj = new GameObject("TitleManager");
            titleObj.AddComponent<TitleManager>();
            Debug.Log("[GameManager] TitleManagerを自動作成");
        }
    }
    
    private void InitializeMazeScene()
    {
        Debug.Log("[GameManager] 迷路シーン初期化完了");
    }
    
    private void InitializeMiniGameScene()
    {
        // EquipmentUIが存在しない場合は自動作成
        if (autoCreateEquipmentUI && FindObjectOfType<EquipmentUI>() == null)
        {
            GameObject equipUIObj = new GameObject("EquipmentUI");
            equipUIObj.AddComponent<EquipmentUI>();
            Debug.Log("[GameManager] EquipmentUIを自動作成");
        }
    }
    
    private void InitializeResultScene()
    {
        // ResultManagerが存在しない場合は自動作成
        if (FindObjectOfType<ResultManager>() == null)
        {
            GameObject resultObj = new GameObject("ResultManager");
            resultObj.AddComponent<ResultManager>();
            Debug.Log("[GameManager] ResultManagerを自動作成");
        }
    }
    
    public static void RestartGame()
    {
        // ゲーム状態をリセット
        PlayerPositionManager.ClearSavedPosition();
        PlayerPrefs.DeleteKey("DeleteTargetEnemy");
        
        Debug.Log("[GameManager] ゲームリセット完了");
    }
    
    public static void QuitGame()
    {
        Debug.Log("[GameManager] ゲーム終了");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}