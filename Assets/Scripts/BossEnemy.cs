using UnityEngine;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 10;
    public float laneChangeSpeed = 5f;
    public float laneOffset = 2.0f;
    public float timeBetweenMoves = 2.0f;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject deathEffect;
    public GameObject rewardPrefab;

    [Header("Reward Loot")]
    public int rewardCount = 15;
    public float explosionForce = 8f;

    // ลากตัวลูก Rat มาใส่ในช่องนี้ที่ Inspector ด้วยนะ! (เพื่อความชัวร์)
    public Animator anim;

    private int currentHealth;
    private CharacterInputController player;
    private float fixedZDistance;
    private float targetX = 0;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        player = FindObjectOfType<CharacterInputController>();

        if (player != null)
        {
            fixedZDistance = transform.position.z - player.transform.position.z;
        }

        // ถ้าลืมลากใส่ ให้มันหาเอง
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (anim != null)
        {
            anim.enabled = true;

            // ====================================================
            // ✅ แก้ชื่อให้ถูกเป๊ะ: SpeedRatio
            // ====================================================
            // ใส่ 1.0f คือวิ่งความเร็วปกติ
            // ใส่ 2.0f คือวิ่งเร็วขึ้น 2 เท่า (บอสควรกระฉับกระเฉงหน่อย)
            anim.SetFloat("SpeedRatio", 2.0f);

            Debug.Log("สั่ง Animator (SpeedRatio) แล้ว! วิ่งสิลูกพ่อ!");
        }

        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        if (isDead) return;
        if (player == null)
        {
            player = FindObjectOfType<CharacterInputController>();
            if (player == null) return;
        }

        // ล็อคระยะห่าง (เกาะติดผู้เล่น)
        float nextZ = player.transform.position.z + fixedZDistance;
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);

        transform.position = new Vector3(nextX, transform.position.y, nextZ);

        // (กันเหนียว) ยัดค่า SpeedRatio เข้าไปตลอดเวลา เผื่อมีใครมาสั่งหยุด
        if (anim != null) anim.SetFloat("SpeedRatio", 2.0f);
    }

    // ... (ส่วน TakeDamage, Die, OnTriggerEnter เหมือนเดิม) ...
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (hitEffect != null) Instantiate(hitEffect, transform.position, Quaternion.identity);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (rewardPrefab != null)
        {
            for (int i = 0; i < rewardCount; i++)
            {
                Vector3 randomPos = Random.insideUnitSphere * 0.5f;
                Vector3 spawnPos = transform.position + randomPos;
                GameObject coin = Instantiate(rewardPrefab, spawnPos, Quaternion.identity);
                Rigidbody rb = coin.GetComponent<Rigidbody>();
                if (rb == null) rb = coin.AddComponent<Rigidbody>();

                Vector3 dir = Random.insideUnitSphere;
                dir.y = Mathf.Abs(dir.y);
                dir.z = 0;
                rb.AddForce(dir * explosionForce, ForceMode.Impulse);
            }
        }

        CharacterInputController p = FindObjectOfType<CharacterInputController>();
        if (p != null)
        {
            p.coins += 500;
            if (PlayerData.instance != null) PlayerData.instance.coins += 500;
        }
        Destroy(gameObject);
    }

    IEnumerator MoveRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(timeBetweenMoves);
            int randomLane = Random.Range(-1, 2);
            targetX = randomLane * laneOffset;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<CharacterInputController>())
        {
            CharacterCollider playerCol = other.GetComponent<CharacterCollider>();
            if (playerCol != null) Debug.Log("ชนบอส!");
        }
    }
}