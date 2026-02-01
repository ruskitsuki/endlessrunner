using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

[RequireComponent(typeof(AudioSource))]
public class CharacterCollider : MonoBehaviour
{
    static int s_HitHash = Animator.StringToHash("Hit");
    static int s_BlinkingValueHash;

    public struct DeathEvent
    {
        public string character;
        public string obstacleType;
        public string themeUsed;
        public int coins;
        public int premium;
        public int score;
        public float worldDistance;
    }

    public CharacterInputController controller;

    public ParticleSystem koParticle;

    // --- เอฟเฟกต์ชนแหลก (Giant Mode) ---
    [Header("Invincible Smash")]
    public GameObject smashEffect;
    public AudioClip smashSound;
    // --------------------------------

    [Header("Sound")]
    public AudioClip coinSound;
    public AudioClip premiumSound;

    public DeathEvent deathData { get { return m_DeathData; } }
    public new BoxCollider collider { get { return m_Collider; } }

    public new AudioSource audio { get { return m_Audio; } }

    [HideInInspector]
    public List<GameObject> magnetCoins = new List<GameObject>();

    public bool tutorialHitObstacle { get { return m_TutorialHitObstacle; } set { m_TutorialHitObstacle = value; } }

    protected bool m_TutorialHitObstacle;

    protected bool m_Invincible;
    protected DeathEvent m_DeathData;
    protected BoxCollider m_Collider;
    protected AudioSource m_Audio;

    protected float m_StartingColliderHeight;

    // ✅ ตัวแปรสำหรับจำค่าเดิม (สำคัญมากสำหรับแก้บั๊ก Slide)
    protected Vector3 m_OriginalSize;
    protected Vector3 m_OriginalCenter;

    protected const float k_MagnetSpeed = 10f;
    protected const int k_CoinsLayerIndex = 8;
    protected const int k_ObstacleLayerIndex = 9;
    protected const int k_PowerupLayerIndex = 10;
    protected const float k_DefaultInvinsibleTime = 2f;

    protected void Start()
    {
        m_Collider = GetComponent<BoxCollider>();
        m_Audio = GetComponent<AudioSource>();
        m_StartingColliderHeight = m_Collider.bounds.size.y;

        // ✅ จำค่าเริ่มต้นของกล่องเอาไว้ (ใช้สำหรับคืนค่าตอนเลิกสไลด์)
        m_OriginalSize = m_Collider.size;
        m_OriginalCenter = m_Collider.center;
    }

    public void Init()
    {
        if (koParticle != null)
        {
            koParticle.gameObject.SetActive(false);
        }

        s_BlinkingValueHash = Shader.PropertyToID("_BlinkingValue");
        m_Invincible = false;
    }

    // ✅ ฟังก์ชัน Slide แบบใหม่ (แก้บั๊กชนสิ่งกีดขวาง)
    public void Slide(bool sliding)
    {
        if (m_Collider == null) return;

        if (sliding)
        {
            // === ตอนสไลด์: ย่อเหลือครึ่งเดียว ===
            Vector3 newSize = m_OriginalSize;
            newSize.y = m_OriginalSize.y * 0.5f; // ลดความสูงเหลือ 50%
            m_Collider.size = newSize;

            // เลื่อนจุดศูนย์กลางลงมา (เพื่อให้เท้าอยู่ที่เดิม แต่หัวหดลง)
            Vector3 newCenter = m_OriginalCenter;
            newCenter.y = m_OriginalCenter.y - (m_OriginalSize.y * 0.25f);
            m_Collider.center = newCenter;
        }
        else
        {
            // === ตอนยืน: คืนค่าเดิมเป๊ะๆ ===
            // ไม่ใช้การคูณกลับ แต่ใช้ค่าเดิมที่จำไว้เลย (ชัวร์สุด)
            m_Collider.size = m_OriginalSize;
            m_Collider.center = m_OriginalCenter;
        }
    }

    protected void Update()
    {
        for (int i = 0; i < magnetCoins.Count; ++i)
        {
            magnetCoins[i].transform.position = Vector3.MoveTowards(magnetCoins[i].transform.position, transform.position, k_MagnetSpeed * Time.deltaTime);
        }
    }

    protected void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.layer == k_CoinsLayerIndex)
        {
            if (magnetCoins.Contains(c.gameObject))
                magnetCoins.Remove(c.gameObject);

            if (c.GetComponent<Coin>().isPremium)
            {
                Addressables.ReleaseInstance(c.gameObject);
                PlayerData.instance.premium += 1;
                controller.premium += 1;
                m_Audio.PlayOneShot(premiumSound);
            }
            else
            {
                Coin.coinPool.Free(c.gameObject);
                PlayerData.instance.coins += 1;
                controller.coins += 1;
                m_Audio.PlayOneShot(coinSound);
            }
        }
        else if (c.gameObject.layer == k_ObstacleLayerIndex)
        {
            // === ถ้าเป็นอมตะ ให้ชนแหลก! ===
            if (m_Invincible || controller.IsCheatInvincible())
            {
                if (smashSound != null) m_Audio.PlayOneShot(smashSound);

                if (smashEffect != null)
                {
                    // ขยับจุดระเบิดขึ้นมาสูงๆ (2 เมตร)
                    Vector3 spawnPos = c.transform.position + new Vector3(0, 2.0f, 0);
                    GameObject vfx = Instantiate(smashEffect, spawnPos, Quaternion.identity);

                    // เบิ้ลขนาดเอฟเฟกต์ 3 เท่า
                    vfx.transform.localScale = vfx.transform.localScale * 3f;
                }

                c.gameObject.SetActive(false);
                return;
            }
            // ============================

            controller.StopMoving();
            c.enabled = false;

            Obstacle ob = c.gameObject.GetComponent<Obstacle>();

            if (ob != null)
            {
                ob.Impacted();
            }
            else
            {
                Addressables.ReleaseInstance(c.gameObject);
            }

            if (TrackManager.instance.isTutorial)
            {
                m_TutorialHitObstacle = true;
            }
            else
            {
                controller.currentLife -= 1;
            }

            controller.character.animator.SetTrigger(s_HitHash);

            if (controller.currentLife > 0)
            {
                m_Audio.PlayOneShot(controller.character.hitSound);
                SetInvincible();
            }
            else
            {
                m_Audio.PlayOneShot(controller.character.deathSound);

                m_DeathData.character = controller.character.characterName;
                m_DeathData.themeUsed = controller.trackManager.currentTheme.themeName;
                // ป้องกัน Error กรณีชนของที่ไม่มีสคริปต์ Obstacle
                m_DeathData.obstacleType = ob != null ? ob.GetType().ToString() : "Unknown";
                m_DeathData.coins = controller.coins;
                m_DeathData.premium = controller.premium;
                m_DeathData.score = controller.trackManager.score;
                m_DeathData.worldDistance = controller.trackManager.worldDistance;
            }
        }
        else if (c.gameObject.layer == k_PowerupLayerIndex)
        {
            Consumable consumable = c.GetComponent<Consumable>();
            if (consumable != null)
            {
                controller.UseConsumable(consumable);
            }
        }
    }

    public void SetInvincibleExplicit(bool invincible)
    {
        m_Invincible = invincible;
    }

    public void SetInvincible(float timer = k_DefaultInvinsibleTime)
    {
        StartCoroutine(InvincibleTimer(timer));
    }

    protected IEnumerator InvincibleTimer(float timer)
    {
        m_Invincible = true;

        float time = 0;
        float currentBlink = 1.0f;
        float lastBlink = 0.0f;
        const float blinkPeriod = 0.1f;

        while (time < timer && m_Invincible)
        {
            Shader.SetGlobalFloat(s_BlinkingValueHash, currentBlink);
            yield return null;
            time += Time.deltaTime;
            lastBlink += Time.deltaTime;

            if (blinkPeriod < lastBlink)
            {
                lastBlink = 0;
                currentBlink = 1.0f - currentBlink;
            }
        }

        Shader.SetGlobalFloat(s_BlinkingValueHash, 0.0f);

        m_Invincible = false;
    }
}