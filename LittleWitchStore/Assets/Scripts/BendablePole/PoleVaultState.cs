using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;

/// <summary>
/// 撑杆跳状态 - 处理撑杆跳逻辑
/// </summary>
public class PoleVaultState : CharacterState
{
    [Header("撑杆跳设置")]
    [SerializeField] private float vaultDetectionDistance = 2f;   // 杆子检测距离
    [SerializeField] private float vaultDuration = 0.8f;          // 撑杆跳持续时间
    [SerializeField] private float launchForce = 15f;             // 发射力度
    [SerializeField] private LayerMask poleLayerMask;             // 杆子层掩码
    
    [Header("弯曲控制")]
    [SerializeField] private float maxBendIntensity = 1f;         // 最大弯曲强度
    [SerializeField] private float bendSensitivity = 2f;          // 弯曲灵敏度
    
    private BendablePole currentPole;
    private ProceduralPoleEnhancer poleEnhancer;
    private JoystickController joystickController;
    private float vaultTimer;
    private bool isBending;
    private Vector3 launchDirection;
    
    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        // 初始化组件引用
        joystickController = FindObjectOfType<JoystickController>();
        
        // 检测杆子
        if (DetectPole())
        {
            // 传送到杆子顶部
            TeleportToPolTop();
            
            // 订阅输入事件
            joystickController.OnRightStickMove += HandleBendingInput;
            joystickController.OnRightStickRelease += HandleLaunch;
            
            // 初始化状态
            vaultTimer = vaultDuration;
            isBending = false;
            
            // 禁用重力
            //CharacterActor.UseGravity = false;
            CharacterActor.Velocity = Vector3.zero;
        }
        else
        {
            // 没有检测到杆子，退出状态
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }
    
    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        // 取消订阅输入事件
        if (joystickController != null)
        {
            joystickController.OnRightStickMove -= HandleBendingInput;
            joystickController.OnRightStickRelease -= HandleLaunch;
        }
        
        // 释放杆子弯曲
        if (currentPole != null)
        {
            currentPole.ReleaseBending();
        }
        
        if (poleEnhancer != null)
        {
            poleEnhancer.ResetDeformation();
        }
        
        // 恢复重力
        //CharacterActor.UseGravity = true;
    }
    
    public override void UpdateBehaviour(float dt)
    {
        vaultTimer -= dt;
        
        // 检查超时
        if (vaultTimer <= 0f)
        {
            LaunchPlayer(Vector3.up * launchForce);
        }
        
        // 保持在杆子顶部
        if (currentPole != null && !isBending)
        {
            CharacterActor.Position = currentPole.GetTopPosition();
        }
    }
    
    public override void CheckExitTransition()
    {
        // 检查是否应该退出状态
        if (CharacterActor.IsGrounded && vaultTimer < vaultDuration * 0.5f)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }
    }
    
    public override bool CheckEnterTransition(CharacterState fromState)
    {
        // 只允许从正常移动状态进入
        return fromState is NormalMovement && DetectPole();
    }
    
    /// <summary>
    /// 检测杆子
    /// </summary>
    private bool DetectPole()
    {
        RaycastHit hit;
        Vector3 rayOrigin = CharacterActor.Position + Vector3.up * 0.5f;
        
        if (Physics.Raycast(rayOrigin, CharacterActor.Forward, out hit, vaultDetectionDistance, poleLayerMask))
        {
            currentPole = hit.collider.GetComponentInParent<BendablePole>();
            poleEnhancer = hit.collider.GetComponentInParent<ProceduralPoleEnhancer>();
            return currentPole != null;
        }
        
        return false;
    }
    
    /// <summary>
    /// 传送到杆子顶部
    /// </summary>
    private void TeleportToPolTop()
    {
        if (currentPole != null)
        {
            Vector3 topPosition = currentPole.GetTopPosition();
            CharacterActor.Teleport(topPosition, CharacterActor.Rotation);
        }
    }
    
    /// <summary>
    /// 处理弯曲输入
    /// </summary>
    private void HandleBendingInput(Vector2 input)
    {
        if (currentPole == null) return;
        
        isBending = input.magnitude > 0.1f;
        
        if (isBending)
        {
            // 计算弯曲强度
            float bendIntensity = Mathf.Clamp01(input.magnitude * bendSensitivity);
            
            // 计算弯曲方向（世界空间）
            Vector3 bendDirection = new Vector3(input.x, 0, input.y);
            bendDirection = CharacterActor.transform.TransformDirection(bendDirection);
            
            // 应用弯曲
            currentPole.ApplyBendingForce(bendDirection, bendIntensity);
            
            // 应用程序化增强
            if (poleEnhancer != null)
            {
                poleEnhancer.ApplyProceduralDeformation(input);
            }
            
            // 计算发射方向
            launchDirection = CalculateLaunchDirection(input, bendIntensity);
        }
        else
        {
            // 释放弯曲
            currentPole.ReleaseBending();
            
            if (poleEnhancer != null)
            {
                poleEnhancer.ResetDeformation();
            }
        }
    }
    
    /// <summary>
    /// 计算发射方向
    /// </summary>
    private Vector3 CalculateLaunchDirection(Vector2 input, float intensity)
    {
        // 基础向上方向
        Vector3 baseDirection = Vector3.up;
        
        // 根据输入添加水平分量
        Vector3 horizontalComponent = new Vector3(input.x, 0, input.y);
        horizontalComponent = CharacterActor.transform.TransformDirection(horizontalComponent);
        
        // 混合方向
        Vector3 finalDirection = (baseDirection + horizontalComponent * intensity).normalized;
        
        return finalDirection;
    }
    
    /// <summary>
    /// 处理发射
    /// </summary>
    private void HandleLaunch()
    {
        if (isBending && launchDirection != Vector3.zero)
        {
            LaunchPlayer(launchDirection * launchForce);
        }
        else
        {
            LaunchPlayer(Vector3.up * launchForce);
        }
    }
    
    /// <summary>
    /// 发射玩家
    /// </summary>
    private void LaunchPlayer(Vector3 velocity)
    {
        CharacterActor.Velocity = velocity;
        CharacterActor.ForceNotGrounded();
        CharacterStateController.EnqueueTransition<NormalMovement>();
    }
}