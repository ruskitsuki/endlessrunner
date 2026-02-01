using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifeTime = 2f;

    [Header("Effects")]
    public GameObject hitEffectPrefab; // เอฟเฟกต์ระเบิดตอนชน
    public GameObject popupPrefab;     // ป้ายคะแนนลอย (+5) ใส่ Prefab "FloatingText" ที่นี่

    void Start()
    {
        // ทำลายตัวเองเมื่อหมดเวลา (กันกระสุนล้นฉาก)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // สั่งให้กระสุนพุ่งไปข้างหน้า
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // =============================================================
        // กรณีที่ 1: ยิงโดน "บอส" (Boss)
        // =============================================================
        if (other.CompareTag("Boss"))
        {
            // พยายามดึงสคริปต์ BossEnemy ออกมาสั่งลดเลือด
            BossEnemy boss = other.GetComponent<BossEnemy>();
            if (boss != null)
            {
                boss.TakeDamage(1); // ยิงเข้า 1 ดาเมจ
            }

            // เล่นเอฟเฟกต์ระเบิด
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // ทำลายกระสุนทิ้ง
            Destroy(gameObject);
            return; // จบการทำงาน (ไม่ไปทำส่วนอื่นต่อ)
        }

        // =============================================================
        // กรณีที่ 2: ยิงโดน "หนู" (Rat) หรือ "สิ่งกีดขวาง" (Obstacle)
        // =============================================================
        if (other.CompareTag("Rat") || other.CompareTag("Obstacle"))
        {
            // 1. เสกเอฟเฟกต์ระเบิด
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // 2. ถ้าเป็นหนู -> ให้แต้ม + เสกป้ายคะแนน
            if (other.CompareTag("Rat"))
            {
                // ค้นหาตัวผู้เล่นเพื่อบวกเงิน
                CharacterInputController player = FindObjectOfType<CharacterInputController>();
                if (player != null)
                {
                    player.coins += 5;
                    // (ถ้ามีระบบ Save ข้อมูลรวมก็บวกด้วย)
                    if (PlayerData.instance != null) PlayerData.instance.coins += 5;

                    // --- [Floating Text] เสกป้ายคะแนนลอย ---
                    if (popupPrefab != null)
                    {
                        // คำนวณจุดเกิดป้าย (เอาตำแหน่งกระสุน + สูงขึ้นมา 2.5 หน่วย)
                        Vector3 spawnPos = transform.position + new Vector3(0, 2.5f, 0);
                        Instantiate(popupPrefab, spawnPos, Quaternion.identity);
                    }
                    // ----------------------------------------
                }

                // สั่งให้หนูหายไป (ตาย)
                other.gameObject.SetActive(false);
            }
            // 3. ถ้าเป็นสิ่งกีดขวาง -> ทำลายทิ้งเลย
            else if (other.CompareTag("Obstacle"))
            {
                other.gameObject.SetActive(false);
                // หรือ Destroy(other.gameObject); ถ้าไม่ได้ใช้ Object Pooling
            }

            // สุดท้าย ทำลายลูกกระสุนทิ้ง
            Destroy(gameObject);
        }
    }
}