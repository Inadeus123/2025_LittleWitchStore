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
    
    [Header("= 基础设置 =")]
    [SerializeField] private GameObject familiarPrefab;       // 使魔预制体
    [SerializeField] private float anchorHeight = 5f;         // 挂点高度（相对于角色）
    [SerializeField] private float anchorForwardOffset = 2f;  // 挂点前方偏移
    
    [SerializeField] float ropeLength = 5f;      // 绳长
    [SerializeField] float swingGravity = 35f;   // 与 NormalMovement 的重力保持一致
    [SerializeField] float airControl   = 3f;    // 空中对摆荡速度的微调程度
    [SerializeField] float damping = 0.98f;   // 阻尼系数
    [SerializeField] float tensionMultiplier = 1.2f;   // 绳张力
    [SerializeField] float minLaunchSpeed = 6f;     // 小于此值就帮玩家补足
    [SerializeField] float maxLaunchSpeed = 18f;    // 大于此值就裁掉
    
    [Header("= 释放参数 =")]
    [SerializeField] private float releaseVelocityBoost = 1.1f; // 释放时的速度增益
    
    [Header("= 召回设置 =")]
    [SerializeField] private float recallSpeed = 30f;         // 召回飞行速度
    [SerializeField] private float recallImpactForce = 15f;  // 撞击力度
    [SerializeField] private float recallImpactRadius = 1f;  // 撞击检测半径
    
    // 组件引用
    private GameObject currentFamiliar;
    private Transform playerTransform;
    //private LineRenderer chainRenderer;
    
    // 状态
    public bool IsDeployed { get; private set; }
    public bool IsRecalling { get; private set; }
    public Vector3 AnchorPosition { get; private set; }
    
    [Header("= 状态设置 =")]
    [SerializeField] private float minSwingHeight = 2f;       // 最小摆荡高度
    [SerializeField] private bool allowGroundExit = true;     // 是否允许着地时退出
    
    public InputSystemHandler inputHandler;
    
    // 状态变量
    private bool wantToRelease = false;
    private bool wantToRecall = false;

    // 物理状态
    private Vector3 velocity;
    private Vector3 position;
    private Vector3 anchorPoint;
    private bool isSwinging;
    
    [Header("Swinging")] 
    private float maxSwingDistance = 3f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    //private Transform player;
    
    protected override void Awake()
    {
        base.Awake();
        
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
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
        
        /*// 主动释放退出
        if (wantToRelease && !swingPhysics.IsSwinging)
        {
            CharacterStateController.EnqueueTransition<NormalMovement>();
            return;
        }*/
        
    }
    
    public override void EnterBehaviour(float dt, CharacterState fromState)
    {
        Debug.Log("进入使魔摆荡状态");
        DeployFamiliar();
        anchorPoint = AnchorPosition;            // 由你的 DeployFamiliar 算好的挂点
        // 把当前速度投影到绳切线，作为初始摆速
        /*Vector3 toAnchor = anchorPoint - CharacterActor.Position;
        Vector3 tangent  = Vector3.Cross(toAnchor, CharacterActor.Right).normalized;
        CharacterActor.Velocity = Vector3.Project(CharacterActor.Velocity, tangent);*/
        Vector3 toAnchor = anchorPoint - CharacterActor.Position;
        Vector3 tangent  = Vector3.Cross(toAnchor, CharacterActor.Right).normalized;

        Vector3 tangentialVel = Vector3.Project(CharacterActor.Velocity, tangent);

        // ----------- 新增：速度裁剪 -----------
        float speed = tangentialVel.magnitude;
        if (speed < minLaunchSpeed)        // 不够快 → 补到下限方向不变
            tangentialVel = tangent * minLaunchSpeed;
        else if (speed > maxLaunchSpeed)   // 太快   → 裁到上限
            tangentialVel = tangentialVel.normalized * maxLaunchSpeed;
        // --------------------------------------

        CharacterActor.Velocity = tangentialVel;


        CharacterActor.alwaysNotGrounded = true; // 不要被判定 grounded
        
    }
    
    /// <summary>
    /// 开始摆荡
    /// </summary>
    public void StartSwing()
    {
        
    }
    
    public override void ExitBehaviour(float dt, CharacterState toState)
    {
        Debug.Log("退出使魔摆荡状态");
        
        
    }
    
    public override void UpdateBehaviour(float dt)
    {
        // 处理输入
        HandleInput();
        // 1) 手动加重力
        CharacterActor.VerticalVelocity += -CharacterActor.Up * swingGravity * dt;

        //输入微调
        Vector2 inputVelocity = CharacterActions.movement.value;
        Vector3 camRight  = CharacterStateController.MovementReferenceRight;
        Vector3 camForward= CharacterStateController.MovementReferenceForward;
        //CharacterActor.PlanarVelocity += (camRight * inputVelocity.x + camForward * inputVelocity.y) * airControl;
        CharacterActor.PlanarVelocity += (camForward * inputVelocity.y) * airControl;
        Debug.Log("Current Velocity " + CharacterActor.PlanarVelocity);
        
        //绳约束 —— 把角色拉回半径 = ropeLength 的球面
        Vector3 toAnchor = CharacterActor.Position - anchorPoint;
        float dist = toAnchor.magnitude;

        /*if (dist > ropeLength)
        {
            Vector3 dir = toAnchor / dist;

            // 3‑a 位置修正
            CharacterActor.Position = anchorPoint + dir * ropeLength;

            // 3‑b 速度分解：去掉指向 anchor 的分量
            CharacterActor.Velocity = Vector3.ProjectOnPlane(CharacterActor.Velocity, dir);
        }*/
        if (dist > ropeLength)
        {
            Vector3 dir = toAnchor / dist;

            // ① 位置回到圆面
            CharacterActor.Position = anchorPoint + dir * ropeLength;

            // ② 把“指向锚点”的分量拿掉，并乘 tensionMultiplier
            float vRadial = Vector3.Dot(CharacterActor.Velocity, dir);
            if (vRadial > 0f)           // 只处理向外拉长的分量
                CharacterActor.Velocity -= dir * vRadial * tensionMultiplier;
        }

        
        //阻尼
        CharacterActor.Velocity *= damping;
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        /*// 释放摆荡（松开F键）
        if (Input.GetKeyUp(KeyCode.F))
        {
            wantToRelease = true;
        }
        
        // 召回使魔（按R键）
        if (Input.GetKeyDown(KeyCode.R))
        {
            wantToRecall = true;
        }*/
        
        if (inputHandler.GetButtonDown("Swing"))
        {
            //Swing
            //Debug.Log("Swing");
            
        }else if (inputHandler.GetBool("Swing"))
        {
            //Debug.Log("In Swinging");
        }
        else if (inputHandler.GetButtonUp("Swing"))
        {
            //Debug.Log("Relase Swing");
            wantToRelease = true;
            PerformRelease();
        }
    }
    
    /// <summary>
    /// 使魔部署完成
    /// </summary>
    private void OnFamiliarDeployed(Vector3 anchorPosition)
    {
        // 开始摆荡
        Debug.Log("OnFamiliarDeployed");
        //swingPhysics.StartSwing(anchorPosition, CharacterActor.Velocity);
    }
    
    /// <summary>
    /// 执行释放
    /// </summary>
    private void PerformRelease()
    {
        /*if (wantToRelease)
        {
            CharacterActor.alwaysNotGrounded = false;   // 让重力/落地逻辑恢复正常
            CharacterStateController.EnqueueTransition<NormalMovement>();
        }*/
        
        if (!wantToRelease) return;

        Vector3 v = CharacterActor.Velocity * releaseVelocityBoost;
        CharacterActor.Velocity = v;

        CharacterActor.alwaysNotGrounded = false;
        ReleaseFamiliar();
        CharacterStateController.EnqueueTransition<NormalMovement>();
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
    
    /// <summary>
    /// 部署使魔到挂点
    /// </summary>
    public void DeployFamiliar()
    {
        if (IsDeployed) return;
        
        // 计算挂点位置
        Vector3 deployDirection = playerTransform.forward;
        deployDirection.y = 0.5f; // 稍微向上
        deployDirection.Normalize();
        
        AnchorPosition = playerTransform.position + 
                         Vector3.up * anchorHeight + 
                         deployDirection * anchorForwardOffset;
        
        // 创建使魔
        if (familiarPrefab != null)
        {
            currentFamiliar = Instantiate(familiarPrefab, AnchorPosition, Quaternion.identity);
        }
        else
        {
            // 如果没有预制体，创建默认球体
            currentFamiliar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentFamiliar.transform.position = AnchorPosition;
            currentFamiliar.transform.localScale = Vector3.one * 0.5f;
            
            // 设置为触发器
            Collider col = currentFamiliar.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
        
        currentFamiliar.name = "Familiar_Anchor";
        
        // 启用锁链
        //chainRenderer.enabled = true;
        
        IsDeployed = true;
        //OnFamiliarDeployed?.Invoke(AnchorPosition);
        //StartSwing();
        Debug.Log($"使魔已部署到: {AnchorPosition}");
    }
    
    /// <summary>
    /// 释放使魔
    /// </summary>
    public void ReleaseFamiliar()
    {
        if (currentFamiliar != null)
        {
            Destroy(currentFamiliar);
            currentFamiliar = null;
        }
        
        //chainRenderer.enabled = false;
        IsDeployed = false;
        IsRecalling = false;
        AnchorPosition = Vector3.zero;
    }
    
    /// <summary>
    /// 绘制调试信息
    /// </summary>
    void OnDrawGizmos()
    {
        if (!isSwinging) return;
        
        // 绘制绳索
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerTransform.position, anchorPoint);
        
        // 绘制锚点
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(anchorPoint, 0.2f);
        
        // 绘制速度方向
        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(playerTransform.position, velocity.normalized * 2f);
    }
}