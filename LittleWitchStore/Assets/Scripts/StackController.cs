using System;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

/// <summary>
/// 挂在场景单例上，负责举高高 & 弹射
/// </summary>
public class StackController : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] Transform player;          // 拖 Player
    [SerializeField] List<MiniStackable> minis; // 运行时动态赋值
    [SerializeField] StackPathProvider pathProvider; // 弧线路径提供者

    [Header("参数")]
    [SerializeField] float stepHeight = 0.6f;   // 每节高度
    [SerializeField] float maxTiltDistance = 1.5f; // 顶端最大水平偏移
    [SerializeField] float shootHorizSpeed = 8f;
    [SerializeField] float shootVertSpeed  = 10f;

    bool stacking   = false;   // 是否处于举高高
    Vector3 basePos;           // 栈底
    Vector2 tiltInput;         // 摇杆平面向量（x,z）

    void Update()
    {
        // —— 按键监听仅作示例，你可以换成 Input System Action —— //
        if (Input.GetKeyDown(KeyCode.R))        // 第一次 RB => 进入栈
            TryEnterStack();
        else if (stacking && Input.GetKeyDown(KeyCode.R))  // 第二次 RB => 弹射
            ShootAndExit();
        
        if (stacking)
        {
            UpdateTilt();
            UpdateMiniTargets();
        }
    }

    #region 进入栈
    void TryEnterStack()
    {
        if (stacking) return;   // 已在栈
        if (minis.Count == 0) return;

        stacking = true;
        basePos = new Vector3(player.position.x, player.position.y - 0.1f, player.position.z);

        // 锁玩家移动（示例：把 Move Action 输入清零）
        //PlayerInputBlocker.BlockMove = true;

        // 打开 MiniStackable 更新
        foreach (var m in minis)
            m.enabled = true;

        UpdateMiniTargets(); // 初始竖直
        pathProvider.IsActive = true; 
        
        CharacterStateController controller = player.GetComponentInChildren<CharacterStateController>();
        controller.EnqueueTransition<StackClimbingState>();
    }
    #endregion

    #region 更新弯曲
    void UpdateTilt()
    {
        // 读取摇杆（示例）——换成你实际 Input
        tiltInput.x = Input.GetAxis("Horizontal");
        tiltInput.y = Input.GetAxis("Vertical");
        tiltInput = Vector2.ClampMagnitude(tiltInput, 1f);
    }

    void UpdateMiniTargets()
    {
        // 把摇杆平面向量转换到世界 XZ
        Vector3 tiltDir = (Vector3.right * tiltInput.x + Vector3.forward * tiltInput.y);
        float   tiltLen = maxTiltDistance * Mathf.Clamp01(tiltInput.magnitude);

        int n = minis.Count;
        if (n == 0) return;

        Vector3 prevPos = Vector3.zero;

        for (int i = 0; i < n; i++)
        {
            // ① 竖直高度
            Vector3 pos = basePos + Vector3.up * stepHeight * i;

            // ② 水平偏移：随高度递增（可调指数 >1 让弯曲更明显）
            float t = (float)i / (n - 1);               // 0-->1
            float curve = Mathf.Pow(t, 1.3f);           // 指数 1.3f ≈ 轻微 S 曲；改 2.0f 更弯
            pos += tiltDir.normalized * tiltLen * curve;

            minis[i].targetWorldPos = pos;

            // ③ 朝向：让当前胶囊 local +Z 指向 **下一节（或下方）**
            if (i > 0)
            {
                Vector3 dir = prevPos - pos;            // 指向下一个(下方)位置
                minis[i].transform.rotation = Quaternion.LookRotation(dir);
            }
            prevPos = pos;
        }

        // 循环结束后 prevPos 仍然是顶端位置
        Vector3 dirDown = minis[^2].targetWorldPos - minis[^1].targetWorldPos; // 顶-次顶
        minis[^1].transform.rotation = Quaternion.LookRotation(dirDown);

    }

    #endregion

    #region 发射 & 退出
    void ShootAndExit()
    {
        stacking = false;

        // 计算冲量
        Vector3 tiltDir = (Vector3.right * tiltInput.x + Vector3.forward * tiltInput.y).normalized;
        Vector3 shootVel = tiltDir * shootHorizSpeed + Vector3.up * shootVertSpeed;

        // 给玩家刚体速度（示例）
        var rb = player.GetComponent<Rigidbody>();
        rb.velocity = shootVel;

        // 恢复玩家控制
        //PlayerInputBlocker.BlockMove = false;

        // 小使魔禁用栈脚本，回到散兵（你后续再实现）
        foreach (var m in minis)
            m.enabled = false;
        pathProvider.IsActive = false;
    }
    #endregion
}
