using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    // พอเกิดมาปุ๊บ ให้นับถอยหลัง 1.5 วินาที แล้วทำลายตัวเองทิ้งซะ
    void Start()
    {
        Destroy(gameObject, 1.5f);
    }
}