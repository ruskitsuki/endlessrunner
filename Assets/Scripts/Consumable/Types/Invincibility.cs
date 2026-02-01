using UnityEngine;
using System;
using System.Collections;

public class Invincibility : Consumable
{
    // ตัวแปรสำหรับจำค่าเดิม (Private เพราะใช้แค่ในนี้)
    private Vector3 originalScale;
    private Vector3 originalColSize;

    public override string GetConsumableName()
    {
        return "Invincible";
    }

    public override ConsumableType GetConsumableType()
    {
        return ConsumableType.INVINCIBILITY;
    }

    public override int GetPrice()
    {
        return 1500;
    }

    public override int GetPremiumCost()
    {
        return 5;
    }

    // Tick ทำงานทุกเฟรม (เอาไว้ย้ำสถานะอมตะ)
    public override void Tick(CharacterInputController c)
    {
        base.Tick(c);
        c.characterCollider.SetInvincibleExplicit(true);
    }

    // เริ่มต้นทำงาน (ขยายร่าง)
    public override IEnumerator Started(CharacterInputController c)
    {
        // 1. จำค่าเดิมไว้ก่อน
        originalScale = c.character.transform.localScale;

        // เช็คเผื่อไว้ก่อนว่ามี Collider ไหม
        if (c.characterCollider != null && c.characterCollider.collider != null)
        {
            originalColSize = c.characterCollider.collider.size;
        }

        // 2. ขยายร่าง 2 เท่า! 😡
        c.character.transform.localScale = originalScale * 2f;

        // 3. ปรับ Collider ให้เล็กลงครึ่งหนึ่งของร่างยักษ์ (จะได้ไม่ติดกำแพง/สิ่งกีดขวางข้างๆ)
        // เพราะพอตัวคูณ 2 กล่องก็คูณ 2 ตาม เราต้องหดกล่องลงเพื่อให้วิ่งในเลนได้สะดวก
        if (c.characterCollider != null && c.characterCollider.collider != null)
        {
            c.characterCollider.collider.size = new Vector3(originalColSize.x * 0.5f, originalColSize.y * 0.5f, originalColSize.z);
        }

        // 4. เปิดอมตะที่ตัว Controller (เพื่อให้วิ่งชนแหลก)
        c.CheatInvincible(true);
        c.characterCollider.SetInvincibleExplicit(true);

        Debug.Log("🛡️ Giant Mode ON: ตัวใหญ่ + อมตะ");

        // 5. รอจนหมดเวลา (เรียกฟังก์ชันแม่)
        yield return base.Started(c);
    }

    // จบการทำงาน (คืนร่าง)
    public override void Ended(CharacterInputController c)
    {
        base.Ended(c);

        // 1. ปิดอมตะ
        c.CheatInvincible(false);
        c.characterCollider.SetInvincibleExplicit(false);

        // 2. คืนร่างขนาดเดิม
        c.character.transform.localScale = originalScale;

        // 3. คืนขนาด Collider เดิม
        if (c.characterCollider != null && c.characterCollider.collider != null)
        {
            c.characterCollider.collider.size = originalColSize;
        }

        Debug.Log("🛡️ Giant Mode OFF: คืนร่างเดิม");
    }
}