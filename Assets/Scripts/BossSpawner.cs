using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;

    [Header("Loop Settings")]
    public float startDistance = 500f;    // บอสตัวแรกมาตอนกี่เมตร
    public float repeatInterval = 500f;  // ตัวต่อไปมาทุกๆ กี่เมตร (นี่คือค่า X ที่คุณต้องการ)
    public float bossZOffset = 5f;      // เกิดห่างจากตัวเราเท่าไหร่ (ควรใกล้ๆ จะได้ยิงง่าย)

    private float nextSpawnDistance;     // เก็บระยะเป้าหมายที่จะเสกตัวถัดไป
    private GameObject currentBoss;      // เก็บตัวบอสปัจจุบัน (เพื่อเช็คว่าตายหรือยัง)

    void Start()
    {
        // เริ่มเกมมา ตั้งเป้าหมายตัวแรกไว้เลย
        nextSpawnDistance = startDistance;
    }

    void Update()
    {
        // ถ้าไม่มี TrackManager ก็ทำอะไรไม่ได้
        if (TrackManager.instance == null) return;

        // เช็คว่าเราวิ่งถึงระยะที่กำหนดหรือยัง?
        if (TrackManager.instance.worldDistance >= nextSpawnDistance)
        {
            TrySpawnBoss();
        }
    }

    void TrySpawnBoss()
    {
        // 1. เช็คก่อนว่า "บอสตัวเก่าตายหรือยัง?" 
        // (ถ้าตัวเก่ายังวิ่งไล่เราอยู่ เราไม่ควรเสกตัวใหม่มาซ้อน เดี๋ยวจะรุมกินโต๊ะ)
        if (currentBoss != null)
        {
            // ถ้าตัวเก่ายังอยู่ -> ให้เลื่อนระยะเสกไปอีกนิด (เช่น อีก 50 เมตรค่อยมาเช็คใหม่)
            nextSpawnDistance += 50f;
            return;
        }

        // 2. ถ้าทางสะดวก (ไม่มีบอส) -> เสกเลย!
        SpawnBoss();

        // 3. กำหนดเป้าหมายรอบถัดไป (เอาระยะปัจจุบัน + อีก 200 เมตร)
        nextSpawnDistance = TrackManager.instance.worldDistance + repeatInterval;
    }

    void SpawnBoss()
    {
        Debug.Log(">>> 🐀 บอสรอบใหม่มาแล้ว! <<<");

        if (bossPrefab == null) return;

        // คำนวณจุดเกิด (ใช้ตำแหน่ง Player + Offset)
        Vector3 spawnPos = new Vector3(0, 0f, transform.position.z + bossZOffset);

        currentBoss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
    }
}