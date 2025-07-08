using UnityEngine;

public class MiniStackable : MonoBehaviour
{
    // 在栈模式里每帧由 StackController 更新这个目标
    public Vector3 targetWorldPos { get; set; }

    [SerializeField] float moveLerp = 15f;   // 越大越快对齐
    [SerializeField] bool  rotateTowardNext = true;

    void Update()
    {
        // 仅在栈模式中启用：由控制器打开 / 关闭
        if (!enabled) return;

        transform.position =
            Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * moveLerp);
    }
}