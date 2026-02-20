using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン間で音楽を管理する統合BGMシステム
/// </summary>
public class BGMManager : MonoBehaviour
{
    [Header("BGM設定")]
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip mazeBGM;
    [SerializeField] private AudioClip miniGameBGM;
    [SerializeField] private AudioClip resultBGM;
    
    [Header("フェード設定")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float volume = 0.05f;
    
    private AudioSource audioSource;
    private static BGMManager instance;
    private string currentScene = "";
    
    private void Awake()
    {
        // シングルトンパターン
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.loop = true;
            audioSource.volume = volume;
            LoadBGMClips();
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
        PlaySceneBGM(SceneManager.GetActiveScene().name);
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    
    private void LoadBGMClips()
    {
        // Resources.Loadで各BGMを読み込み
        titleBGM = Resources.Load<AudioClip>("BGM/bgm001_title");
        mazeBGM = Resources.Load<AudioClip>("BGM/bgm011_3dField");
        miniGameBGM = Resources.Load<AudioClip>("BGM/bgm021_pazzle");
        resultBGM = Resources.Load<AudioClip>("BGM/bgm002_enging");
        
        Debug.Log("[BGMManager] BGMファイル読み込み完了");
        LogLoadResults();
    }
    
    private void LogLoadResults()
    {
        Debug.Log($"[BGMManager] タイトルBGM: {(titleBGM != null ? "OK" : "NG")}");
        Debug.Log($"[BGMManager] 迷路BGM: {(mazeBGM != null ? "OK" : "NG")}");
        Debug.Log($"[BGMManager] MiniGameBGM: {(miniGameBGM != null ? "OK" : "NG")}");
        Debug.Log($"[BGMManager] ResultBGM: {(resultBGM != null ? "OK" : "NG")}");
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneBGM(scene.name);
    }
    
    private void PlaySceneBGM(string sceneName)
    {
        if (currentScene == sceneName) return; // 同じシーンなら何もしない
        
        currentScene = sceneName;
        AudioClip targetClip = null;
        
        switch (sceneName)
        {
            case "TitleScene":
                targetClip = titleBGM;
                break;
            case "MazeScene":
                targetClip = mazeBGM;
                break;
            case "MiniGameScene":
                targetClip = miniGameBGM;
                break;
            case "ResultScene":
                targetClip = resultBGM;
                break;
            default:
                Debug.LogWarning($"[BGMManager] 未定義のシーン: {sceneName}");
                return;
        }
        
        if (targetClip != null)
        {
            StartCoroutine(FadeToNewBGM(targetClip));
            Debug.Log($"[BGMManager] {sceneName} のBGMを開始: {targetClip.name}");
        }
        else
        {
            Debug.LogWarning($"[BGMManager] {sceneName} 用のBGMが見つかりません");
        }
    }
    
    private System.Collections.IEnumerator FadeToNewBGM(AudioClip newClip)
    {
        // フェードアウト
        float startVolume = audioSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeTime);
            yield return null;
        }
        audioSource.volume = 0;
        
        // 新しいクリップに切り替え
        audioSource.clip = newClip;
        audioSource.Play();
        
        // フェードイン
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, volume, t / fadeTime);
            yield return null;
        }
        audioSource.volume = volume;
    }
    
    public static void SetVolume(float newVolume)
    {
        if (instance != null)
        {
            instance.volume = newVolume;
            instance.audioSource.volume = newVolume;
        }
    }
    
    public static void Stop()
    {
        if (instance != null && instance.audioSource.isPlaying)
        {
            instance.audioSource.Stop();
        }
    }
}