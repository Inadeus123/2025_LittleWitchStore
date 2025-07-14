using Lightbug.CharacterControllerPro.Core;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;

/// <summary>
/// 撑杆跳主控制器 - 整合所有系统
/// </summary>
public class PoleVaultController : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private CharacterActor characterActor;
    [SerializeField] private CharacterStateController stateController;
    [SerializeField] private JoystickController joystickController;
    [SerializeField] private PoleDetector poleDetector;
    [SerializeField] private SmoothAnimationController animationController;
    
    [Header("配置")]
    [SerializeField] private PoleVaultConfiguration configuration;
    
    [Header("调试")]
    [SerializeField] private bool enableDebugGizmos = true;
    [SerializeField] private bool enableDebugLogs = false;
    
    private BendablePole currentPole;
    private bool isVaulting = false;
    private Vector2 lastBendInput;
    
    void Start()
    {
        InitializeComponents();
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// 初始化组件
    /// </summary>
    private void InitializeComponents()
    {
        if (characterActor == null)
            characterActor = GetComponent<CharacterActor>();
            
        if (stateController == null)
            stateController = GetComponent<CharacterStateController>();
            
        if (joystickController == null)
            joystickController = FindObjectOfType<JoystickController>();
            
        if (poleDetector == null)
            poleDetector = GetComponent<PoleDetector>();
            
        if (animationController == null)
            animationController = GetComponent<SmoothAnimationController>();
    }
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (poleDetector != null)
        {
            poleDetector.OnPoleDetected += OnPoleDetected;
            poleDetector.OnPoleLeft += OnPoleLeft;
        }
        
        if (joystickController != null)
        {
            joystickController.OnRightStickMove += OnRightStickMove;
            joystickController.OnRightStickRelease += OnRightStickRelease;
        }
    }
    
    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (poleDetector != null)
        {
            poleDetector.OnPoleDetected -= OnPoleDetected;
            poleDetector.OnPoleLeft -= OnPoleLeft;
        }
        
        if (joystickController != null)
        {
            joystickController.OnRightStickMove -= OnRightStickMove;
            joystickController.OnRightStickRelease -= OnRightStickRelease;
        }
    }
    
    /// <summary>
    /// 检测到杆子
    /// </summary>
    private void OnPoleDetected(BendablePole pole)
    {
        currentPole = pole;
        
        if (enableDebugLogs)
            Debug.Log("检测到杆子: " + pole.name);
        
        // 可以在这里触发状态转换到撑杆跳状态
        if (stateController != null)
        {
            stateController.EnqueueTransition<PoleVaultState>();
        }
    }
    
    /// <summary>
    /// 离开杆子
    /// </summary>
    private void OnPoleLeft()
    {
        currentPole = null;
        isVaulting = false;
        
        if (enableDebugLogs)
            Debug.Log("离开杆子");
    }
    
    /// <summary>
    /// 右摇杆移动
    /// </summary>
    private void OnRightStickMove(Vector2 input)
    {
        lastBendInput = input;
        
        if (currentPole != null && isVaulting)
        {
            // 计算弯曲方向和强度
            Vector3 bendDirection = new Vector3(input.x, 0, input.y);
            float bendIntensity = Mathf.Clamp01(input.magnitude * configuration.bendSensitivity);
            
            // 应用弯曲
            currentPole.ApplyBendingForce(bendDirection, bendIntensity);
            
            // 播放弯曲动画
            if (animationController != null)
            {
                animationController.AnimateBending(currentPole, bendDirection, bendIntensity);
            }
        }
    }
    
    /// <summary>
    /// 右摇杆释放
    /// </summary>
    private void OnRightStickRelease()
    {
        if (currentPole != null && isVaulting)
        {
            // 计算发射方向
            Vector3 launchDirection = CalculateLaunchDirection();
            
            // 发射玩家
            LaunchPlayer(launchDirection);
            
            // 播放返回动画
            if (animationController != null)
            {
                animationController.AnimateReturn();
            }
        }
    }
    
    /// <summary>
    /// 计算发射方向
    /// </summary>
    private Vector3 CalculateLaunchDirection()
    {
        Vector3 baseDirection = Vector3.up;
        
        if (lastBendInput.magnitude > 0.1f)
        {
            // 根据最后的弯曲输入计算水平分量
            Vector3 horizontalComponent = new Vector3(lastBendInput.x, 0, lastBendInput.y);
            horizontalComponent = transform.TransformDirection(horizontalComponent);
            
            // 混合方向
            baseDirection = (baseDirection + horizontalComponent * 0.5f).normalized;
        }
        
        return baseDirection;
    }
    
    /// <summary>
    /// 发射玩家
    /// </summary>
    private void LaunchPlayer(Vector3 direction)
    {
        if (characterActor != null)
        {
            Vector3 launchVelocity = direction * configuration.launchForce;
            characterActor.Velocity = launchVelocity;
            characterActor.ForceNotGrounded();
        }
        
        isVaulting = false;
        
        if (enableDebugLogs)
            Debug.Log("发射玩家: " + direction * configuration.launchForce);
    }
    
    /// <summary>
    /// 设置撑杆跳状态
    /// </summary>
    public void SetVaultingState(bool vaulting)
    {
        isVaulting = vaulting;
    }
    
    /// <summary>
    /// 获取当前杆子
    /// </summary>
    public BendablePole GetCurrentPole()
    {
        return currentPole;
    }
    
    void OnDrawGizmos()
    {
        if (!enableDebugGizmos) return;
        
        // 绘制发射方向
        if (isVaulting && lastBendInput.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Vector3 launchDirection = CalculateLaunchDirection();
            Gizmos.DrawRay(transform.position, launchDirection * 3f);
        }
        
        // 绘制检测范围
        if (poleDetector != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, configuration.detectionDistance);
        }
    }
}