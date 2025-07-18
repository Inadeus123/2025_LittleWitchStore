using UnityEngine;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;

/// <summary>
/// 使魔摆荡状态 - CCP自定义状态
/// </summary>
[AddComponentMenu("Character Controller Pro/Demo/Character/States/Familiar Swing")]
public class FamiliarSwingState : CharacterState
{
    [Header("= 状态设置 =")]
    [SerializeField] private float minSwingHeight = 2f;       // 最小摆荡高度
    [SerializeField] private bool allowGroundExit = true;     // 是否允许着地时退出
    
    // 组件引用
    private FamiliarController familiarController;
    private SwingPhysics swingPhysics;
    //private CharacterActor characterActor;
    public InputSystemHandler inputHandler;
    
    // 状态变量
    private bool wantToRelease = false;
    private bool wantToRecall = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 获取组件
        familiarController = GetComponent<FamiliarController>();
        swingPhysics = GetComponent<SwingPhysics>();
        
        // 确保组件存在
        if (familiarController == null)
            familiarController = gameObject.AddComponent<FamiliarController>();
        
        if (swingPhysics == null)
            swingPhysics = gameObject.AddComponent<SwingPhysics>();
    }
    
    public override string GetInfo()
    {
        return "使魔摆荡状态：在空中抛出使魔进行摆荡，松开后沿切线飞出，可召回使魔获得额外冲力。";
    }
    
    public override bool CheckEnterTransition(CharacterState fromState)
    {
            /*// 必须在空中且有一定高度
            if (!CharacterActor.IsGrounded && 
                CharacterActor.Position.y > CharacterActor.GroundPosition.y + minSwingHeight)
            {
                return true;
            }
        
        
        return false;*/

            return true;
    }
    
    public override void CheckExitTransition()
    {
        // 着地退出
        if (allowGroundExit && CharacterActor.IsGrounded)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            return;
        }
        
        // 主动释放退出
        if (wantToRelease && !swingPhysics.IsSwinging)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            return;
        }
    }
    
    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        Debug.Log("进入使魔摆荡状态");
        
        // 部署使魔
        familiarController.DeployFamiliar();
        
        // 订阅事件
        familiarController.OnFamiliarDeployed += OnFamiliarDeployed;
        familiarController.OnRecallImpact += OnRecallImpact;
        
        // 禁用重力（由摆荡物理接管）
        CharacterActor.RigidbodyComponent.UseGravity = false;
        // 重置状态
        wantToRelease = false;
        wantToRecall = false;
    }
    
    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        Debug.Log("退出使魔摆荡状态");
        
        // 取消订阅事件
        familiarController.OnFamiliarDeployed -= OnFamiliarDeployed;
        familiarController.OnRecallImpact -= OnRecallImpact;
        
        // 恢复重力
        //CharacterActor.UseGravity = true;
        CharacterActor.RigidbodyComponent.UseGravity = true;
        // 停止摆荡
        swingPhysics.StopSwing();
        
        // 释放使魔
        familiarController.ReleaseFamiliar();
    }
    
    public override void UpdateBehaviour(float dt)
    {
        // 处理输入
        HandleInput();
        
        // 更新摆荡物理
        if (swingPhysics.IsSwinging)
        {
            swingPhysics.UpdateSwing(dt);
        }
        
        // 处理释放
        if (wantToRelease && swingPhysics.IsSwinging)
        {
            PerformRelease();
        }
        
        // 处理召回
        if (wantToRecall && !familiarController.IsRecalling && !swingPhysics.IsSwinging)
        {
            familiarController.RecallFamiliar();
            wantToRecall = false;
        }
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 释放摆荡（松开F键）
        if (Input.GetKeyUp(KeyCode.F))
        {
            wantToRelease = true;
        }
        
        // 召回使魔（按R键）
        if (Input.GetKeyDown(KeyCode.R))
        {
            wantToRecall = true;
        }
    }
    
    /// <summary>
    /// 使魔部署完成
    /// </summary>
    private void OnFamiliarDeployed(Vector3 anchorPosition)
    {
        // 开始摆荡
        swingPhysics.StartSwing(anchorPosition, CharacterActor.Velocity);
    }
    
    /// <summary>
    /// 执行释放
    /// </summary>
    private void PerformRelease()
    {
        // 获取释放速度
        Vector3 releaseVelocity = swingPhysics.ReleaseSwing();
        
        // 应用到角色
        //CharacterActor.SetVelocity(releaseVelocity);
        CharacterActor.Velocity = releaseVelocity;
        Debug.Log($"释放摆荡，速度: {releaseVelocity}");
    }
    
    /// <summary>
    /// 召回撞击
    /// </summary>
    private void OnRecallImpact(Vector3 impactForce)
    {
        // 应用撞击力
        Vector3 currentVelocity = CharacterActor.Velocity;
        //CharacterActor.SetVelocity(currentVelocity + impactForce);
        CharacterActor.Velocity = currentVelocity + impactForce;
        Debug.Log($"召回撞击！力度: {impactForce.magnitude}");
        
        // 可以在这里添加特效、音效等
    }
}