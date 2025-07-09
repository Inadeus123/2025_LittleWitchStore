using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine.InputSystem;

/// <summary>
/// "叠高高" 爬绳状态（基于 CCP CharacterState）。
/// 
/// ● 左摇杆：Y = 沿弧长上/下爬；X = 绕绳旋转身体。
/// ● 右摇杆：实时弯曲绳子；松手瞬间按弯曲反方向将角色弹射。
/// </summary>
public class StackClimbingState : CharacterState
{
    /* ────────── 可在 Inspector 中配置 ────────── */
    [Header("References")]
    [SerializeField] StackPathProvider pathProvider;   // 提供七段栈路径 & 弯曲接口
    [SerializeField] Transform playerGraphics;         // 角色可视模型，用于同步朝向（可选）
    [SerializeField] InputActionReference characterCameraAction; 

    [Header("Climb Tuning")]
    [SerializeField] float climbSpeed = 2f;            // m/s 沿弧长
    [SerializeField] float rotationSpeed = 120f;       // °/s 左右旋转
    [SerializeField] float forwardOffset = -0.25f;     // 贴绳距离（负值越贴）

    [Header("Bend & Launch")]
    [SerializeField] float bendSensitivity = 1f;       // 右摇杆 → 弯曲比例
    [SerializeField] float launchSpeed = 12f;          // 弹射速度（m/s）
    [SerializeField] float stickReleaseThreshold = 0.1f; // 判定“松手”阈值

    [Header("Animator Params")]
    [SerializeField] string vertVelParam = "VerticalVelocity";
    

    /* ────────── 运行时变量 ────────── */
    float progress;          // 0~1 在 path 上的位置
    float curSpeed;          // 当帧沿弧长速度（带符号）
    float yawOffset;         // 角色绕绳轴累计旋转

    Vector2 prevBend;        // 右摇杆上一帧值
    Vector2 curBend;         // 右摇杆当前帧值
    Vector3 lastBendDir;     // 世界坐标最后一次有效弯曲方向

    /* ────────── 状态切换 ────────── */
    public override bool CheckEnterTransition(CharacterState fromState)
    {
        return pathProvider != null && pathProvider.IsActive; // StackController 负责设置 IsActive
    }

    public override void EnterBehaviour(float dt, CharacterState prev)
    {
        CharacterActor.alwaysNotGrounded = true;
        CharacterActor.IsKinematic = false;
        CharacterActor.UseRootMotion = false;
        //禁用相机旋转
        Camera3D cam = Camera.main.GetComponent<Camera3D>();
        cam.updatePitch = false;
        cam.updateYaw = false;
        characterCameraAction.action.canceled += OnStickRelease;
        // 把脚底投影到栈曲线得到初始 progress
        pathProvider.Sample(0, out _, out _); // 确保缓存表有效
        progress = FindClosestProgress(CharacterActor.Bottom);

        CharacterActor.Velocity = Vector3.zero;
        yawOffset = 0f;
        prevBend = curBend = Vector2.zero;
    }

    void OnStickRelease(InputAction.CallbackContext ctx)
    {
        Vector2 stick = ctx.ReadValue<Vector2>();

        // 若松手瞬间几乎没有输入，就不发射
        if (lastBendDir.sqrMagnitude < 1e-4f)
            return;     

        // 反方向 → 世界坐标
        Vector3 dir = new Vector3(-lastBendDir.x, 0f, -lastBendDir.y).normalized;

        Launch(dir);                                   // 固定力度
        CharacterStateController.EnqueueTransition<NormalMovement>();
    }
    

    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        CharacterActor.alwaysNotGrounded = false;
    }

    /* ────────── 主循环 ────────── */
    public override void UpdateBehaviour(float dt)
    {
        // ── 1. 左摇杆：沿弧长移动 ──
        Vector2 moveInput = CharacterActions.movement.value; // (-1..1, -1..1)
        curSpeed = moveInput.y * climbSpeed;
        progress = Mathf.Clamp01(progress + curSpeed * dt / pathProvider.TotalLength);

        // ── 2. 从 path 采样位置 & 切主角位置 ──
        pathProvider.Sample(progress, out var pos, out var tangent);

        // 基础 outward (右手方向) = tangent × up 叉积方向
        Vector3 outward = Vector3.Cross(tangent, Vector3.up).normalized;

        // 把主角粘到绳子外侧
        Vector3 targetPos = pos + outward * forwardOffset;
        CharacterActor.Position = targetPos;
        CharacterActor.Velocity = Vector3.zero;

        // 更新朝向
        //CharacterActor.SetYaw(outward);
        //if (playerGraphics) playerGraphics.rotation = CharacterActor.transform.rotation;

        // ── 4. 右摇杆：弯曲 ──
        prevBend = curBend;
        curBend = CharacterActions.camera.value * bendSensitivity; // 默认绑定右摇杆
        Debug.Log("Current Bend: " + curBend);
        pathProvider.SetBendInput(curBend);  // 由 provider 负责实际弯曲实现

        // 保存最后一次非零弯曲方向（世界坐标）
        //lastBendDir = (CharacterActor.transform.right * curBend.x + CharacterActor.transform.forward * curBend.y).normalized;
        if (curBend.sqrMagnitude > 1e-4f)
        //lastBendDir = (Vector3.right * curBend.x + Vector3.forward * curBend.y).normalized;   // 纯世界轴
        lastBendDir = curBend.normalized;
        Debug.Log("lastBendDir: " + lastBendDir);
        //Debug.Log("CharacterActorRight: " + lastBendDir);
        // ── 5. 检测松手 → Launch ──
        //bool released = prevBend.magnitude > stickReleaseThreshold && curBend.magnitude <= stickReleaseThreshold;
        /*if (released)
        {
            Launch();
            CharacterStateController.EnqueueTransition<NormalMovement>(); // 交给普通移动
        }*/
        
        //检测右摇杆松开
        
    }

    public override void PostCharacterSimulation(float dt)
    {
        if (CharacterActor.IsAnimatorValid())
            CharacterActor.Animator.SetFloat(vertVelParam, curSpeed);
    }

    /* ────────── 私有方法 ────────── */
    void Launch(Vector3 launchDir)
    {
        CharacterActor.Velocity = launchDir * launchSpeed;
        pathProvider.IsActive   = false;               // 解除攀爬
    }

    float FindClosestProgress(Vector3 worldPos)
    {
        float best = 0;
        float bestDist = float.MaxValue;
        const int samples = 100;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            pathProvider.Sample(t, out var p, out _);
            float d = (worldPos - p).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = t; }
        }
        return best;
    }
}
