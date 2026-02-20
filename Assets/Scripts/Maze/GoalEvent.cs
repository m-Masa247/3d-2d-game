using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoalEvent : MonoBehaviour
{
    private bool hasTriggered = false; // 重複トリガーを防ぐ

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("★★★ 迷路クリア！★★★");
            StartCoroutine(GoalReachedSequence());
        }
    }

    private IEnumerator GoalReachedSequence()
    {
        Debug.Log("おめでとうございます！ゲームクリアです！");
        Debug.Log("結果画面に移動します...");
        
        // プレイヤーを停止
        PlayerMove playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false; // プレイヤー移動を停止
        }
        
        // 2秒待ってからResult画面へ
        yield return new WaitForSeconds(2f);
        
        SceneManager.LoadScene("ResultScene");
    }
}