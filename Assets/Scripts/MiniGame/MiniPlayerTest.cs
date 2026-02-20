using UnityEngine;

public class MiniPlayerTest : MonoBehaviour
{
    public float speed = 10f;

    void Start()
    {
        Debug.Log("=== MiniPlayerTest 開始 ===");
        Debug.Log($"Tag: '{gameObject.tag}'");
        Debug.Log($"Position: {transform.position}");
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
        }
        else
        {
            Debug.LogError("Colliderが見つかりません！");
        }
    }

    void Update()
    {
        // 左右移動
        float h = Input.GetAxis("Horizontal");
        Vector3 move = new Vector3(h * speed * Time.deltaTime, 0, 0);
        transform.Translate(move);

        // 位置制限
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -8f, 8f);
        transform.position = pos;
        
        // 1秒ごとに位置を報告
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[MiniPlayerTest] 位置: {transform.position:F1}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[MiniPlayerTest] Trigger接触: {other.name}");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[MiniPlayerTest] Collision接触: {collision.gameObject.name}");
    }
}