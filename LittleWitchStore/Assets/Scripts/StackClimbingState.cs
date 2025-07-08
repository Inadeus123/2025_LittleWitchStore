using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;


public class StackClimbingState : CharacterState
{

   [Header("References")]
    [SerializeField] StackPathProvider pathProvider;   // 拖 StackController
    [SerializeField] Transform playerGraphics;         // 可选，让角色转向

    [Header("Tuning")]
    [SerializeField] float climbSpeed = 2f;            // m/s 沿弧长
    [SerializeField] float forwardOffset = -0.25f;     // 贴紧负值
    [SerializeField] string vertVelParam = "VerticalVelocity";

    float progress;        // 0~1
    float curSpeed;        // 当前沿弧长速度

    /* ───────────────── Enter / Exit ───────────────── */

    public override bool CheckEnterTransition(CharacterState fromState)
    {
        // 由 StackController 设置全局 bool
        Debug.Log("PathProvider is active: " + pathProvider != null && pathProvider.IsActive);
        return pathProvider != null && pathProvider.IsActive;
    }

    public override void EnterBehaviour(float dt, CharacterState prev)
    {
        CharacterActor.alwaysNotGrounded = true;
        CharacterActor.IsKinematic = false;
        CharacterActor.UseRootMotion = false;

        // 从当前脚底投影到栈，得到初始 progress
        pathProvider.Sample(0, out _, out _); // 确保弧长表已更新
        progress = FindClosestProgress(CharacterActor.Bottom);

        CharacterActor.Velocity = Vector3.zero;
    }

    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        CharacterActor.alwaysNotGrounded = false;
        // 若因 Jump 退出，由 NormalMovement 负责赋跳跃速度
    }

    /* ───────────────── Update ───────────────── */

    public override void UpdateBehaviour(float dt)
    {
        // 1. 进度 += 输入
        float inputY = CharacterActions.movement.value.y;      // -1~1
        curSpeed = inputY * climbSpeed;
        progress = Mathf.Clamp01(progress + curSpeed * dt / pathProvider.TotalLength);

        // 2. 采样位置 & 切位置
        pathProvider.Sample(progress, out var pos, out var tangent);

        Vector3 outward = Vector3.Cross(tangent, Vector3.up).normalized; // 栈右手方向
        Vector3 targetPos = pos + outward * forwardOffset;

        CharacterActor.Position = targetPos;
        CharacterActor.Velocity = Vector3.zero;    // 完全粘附

        // 角色朝向 → 贴着栈正对
        CharacterActor.SetYaw(outward);

        if (playerGraphics)
            playerGraphics.rotation = CharacterActor.transform.rotation;
    }

    public override void PostUpdateBehaviour(float dt)
    {
        if (CharacterActions.jump.Started || !pathProvider.IsActive)
            CharacterStateController.EnqueueTransition<NormalMovement>();
    }

    public override void PostCharacterSimulation(float dt)
    {
        if (CharacterActor.IsAnimatorValid())
            CharacterActor.Animator.SetFloat(vertVelParam, curSpeed);
    }

    /* ───────────────── Helpers ───────────────── */

    float FindClosestProgress(Vector3 worldPos)
    {
        // 粗采样：线性搜索 100 步即可
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
