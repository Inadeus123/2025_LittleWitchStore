using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

/// <summary>
/// 摆荡物理系统 - 处理绳索摆荡的物理计算
/// </summary>
public class SwingPhysics : MonoBehaviour
{
    [Header("= 摆荡参数 =")]
    [SerializeField] private float ropeLength = 5f;           // 绳索长度
    [SerializeField] private float gravity = 20f;             // 重力加速度
    [SerializeField] private float damping = 0.98f;           // 阻尼系数（能量衰减）
    [SerializeField] private float tensionMultiplier = 1.2f;  // 张力倍数
    
    [Header("= 释放参数 =")]
    [SerializeField] private float releaseVelocityBoost = 1.1f; // 释放时的速度增益
    
    // 物理状态
    private Vector3 velocity;
    private Vector3 position;
    private Vector3 anchorPoint;
    private bool isSwinging;
    
    // 组件引用
    //private CharacterController characterController;
    private CharacterActor characterActor;
    private Rigidbody rb;
    
    // 公共属性
    public Vector3 CurrentVelocity => velocity;
    public bool IsSwinging => isSwinging;
    
    void Awake()
    {
        characterActor = GetComponent<CharacterActor>();
        rb = GetComponent<Rigidbody>();
    }
    
    /// <summary>
    /// 开始摆荡
    /// </summary>
    public void StartSwing(Vector3 anchor, Vector3 initialVelocity)
    {
        anchorPoint = anchor;
        position = transform.position;
        velocity = initialVelocity;
        isSwinging = true;
        
        // 计算实际绳索长度
        ropeLength = Vector3.Distance(position, anchorPoint);
        
        Debug.Log($"开始摆荡 - 锚点: {anchorPoint}, 绳长: {ropeLength}");
    }
    
    /// <summary>
    /// 更新摆荡物理
    /// </summary>
    public void UpdateSwing(float deltaTime)
    {
        if (!isSwinging) return;
        
        // 1. 应用重力
        velocity += Vector3.down * gravity * deltaTime;
        
        // 2. 预测新位置
        Vector3 nextPosition = position + velocity * deltaTime;
        
        // 3. 约束到绳索长度（投影到圆弧上）
        Vector3 toAnchor = nextPosition - anchorPoint;
        float currentDistance = toAnchor.magnitude;
        
        if (currentDistance > ropeLength)
        {
            // 将位置约束到绳索长度
            nextPosition = anchorPoint + toAnchor.normalized * ropeLength;
            
            // 计算径向和切向分量
            Vector3 radialDirection = (position - anchorPoint).normalized;
            float radialVelocity = Vector3.Dot(velocity, radialDirection);
            
            // 移除径向速度分量（绳索张力）
            if (radialVelocity > 0)
            {
                velocity -= radialDirection * radialVelocity * tensionMultiplier;
            }
        }
        
        // 4. 更新位置
        Vector3 deltaPos = nextPosition - position;
        position = nextPosition;
        
        // 5. 应用阻尼
        velocity *= damping;
        
        // 6. 应用到角色
        if (characterActor != null && characterActor.enabled)
        {
            characterActor.Move(deltaPos);
            
        }
        else if (rb != null)
        {
            rb.MovePosition(position);
        }
        else
        {
            transform.position = position;
        }
        
        // 7. 更新实际速度（用于动画等）
        velocity = deltaPos / deltaTime;
    }
    
    /// <summary>
    /// 释放摆荡
    /// </summary>
    public Vector3 ReleaseSwing()
    {
        if (!isSwinging) return Vector3.zero;
        
        isSwinging = false;
        
        // 计算释放速度（添加少许增益让动作更爽快）
        Vector3 releaseVelocity = velocity * releaseVelocityBoost;
        
        Debug.Log($"释放摆荡 - 速度: {releaseVelocity.magnitude}");
        
        return releaseVelocity;
    }
    
    /// <summary>
    /// 停止摆荡
    /// </summary>
    public void StopSwing()
    {
        isSwinging = false;
        velocity = Vector3.zero;
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    void OnDrawGizmos()
    {
        if (!isSwinging) return;
        
        // 绘制绳索
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, anchorPoint);
        
        // 绘制锚点
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(anchorPoint, 0.2f);
        
        // 绘制速度方向
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, velocity.normalized * 2f);
    }
}