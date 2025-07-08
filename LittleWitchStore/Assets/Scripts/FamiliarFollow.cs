using UnityEngine;

public class FamiliarFollow : MonoBehaviour
{
    [SerializeField] Transform player;                // 拖 Player
    [SerializeField] Vector3 localOffset = new(0.8f, 1.2f, -0.8f);
    [SerializeField] float followTightness = 10f;     // 越大越黏 5~15
    [SerializeField] float rotateTightness = 10f;     // 朝向插值

    Vector3 velocity;                                 // SmoothDamp 嵌套值

    void Reset() => player = GameObject.FindWithTag("Player")?.transform;

    void LateUpdate()
    {
        if (!player) return;

        // 1. 目标点：玩家局部偏移
        Vector3 desired = player.TransformPoint(localOffset);

        // 2. 平滑移动
        transform.position = Vector3.SmoothDamp(
            transform.position, desired,
            ref velocity,
            1f / followTightness,  // 时间常数
            Mathf.Infinity,
            Time.deltaTime);

        // 3. 跟随玩家朝向
        Quaternion targetRot = Quaternion.LookRotation(player.forward, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot,
            Time.deltaTime * rotateTightness);
    }
}