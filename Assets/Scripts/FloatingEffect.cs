using UnityEngine;
using TMPro; // อย่าลืมบรรทัดนี้!

public class FloatingEffect : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float destroyTime = 1f;

    void Start()
    {
        // 1. หันหน้าเข้าหากล้องเสมอ (คนเล่นจะได้อ่านออก)
        transform.rotation = Camera.main.transform.rotation;

        // 2. สั่งให้ทำลายตัวเองตามเวลาที่กำหนด
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 3. ลอยขึ้นข้างบนเรื่อยๆ
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}