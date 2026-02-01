using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerShooter : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("Settings")]
    public float fireRate = 0.5f; // <--- เวลายิงต่อนัด (0.5 วิ)
    public AudioClip shootSound;

    private float nextFireTime = 0f; // <--- ตัวนับเวลา
    private CharacterInputController playerController;
    private AudioSource audioSource;

    void Start()
    {
        playerController = FindObjectOfType<CharacterInputController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (TrackManager.instance != null && TrackManager.instance.isMoving)
        {
            // เงื่อนไข 1: กดปุ่ม
            // เงื่อนไข 2: เวลาปัจจุบัน (Time.time) ต้องมากกว่า เวลาที่กำหนด (nextFireTime)
            if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFireTime)
            {
                // เงื่อนไข 3: ต้องมีเงิน
                if (playerController != null && playerController.coins > 0)
                {
                    // === ยิงได้แล้ว! ===

                    // 1. ตั้งเวลาสำหรับนัดต่อไปทันที (ปัจจุบัน + 0.5วิ)
                    nextFireTime = Time.time + fireRate;

                    // 2. หักเงิน
                    playerController.coins -= 1;
                    if (PlayerData.instance != null && PlayerData.instance.coins > 0)
                        PlayerData.instance.coins -= 1;

                    // 3. ปล่อยกระสุน
                    Vector3 firePos = transform.position + new Vector3(0, 1, 0);
                    Instantiate(bulletPrefab, firePos, transform.rotation);

                    // 4. เล่นเสียง
                    if (shootSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(shootSound);
                    }
                }
                else
                {
                    Debug.Log("ก้างปลาหมด!");
                }
            }
        }
    }
}