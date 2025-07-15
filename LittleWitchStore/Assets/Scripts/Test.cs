using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine.InputSystem;

    public class Test : CharacterState
    {
       [Header("= 基础设置 =")] [SerializeField] protected GameObject capsulePrefab; // 胶囊体预制体
       [SerializeField] protected InputSystemHandler inputHandler; // 输入处理器

       [SerializeField] protected Vector3 capsuleOffset = new Vector3(0, -0.5f, 0); // 胶囊体偏移

       [Header("= 移动参数 =")] [Min(0f)] [SerializeField]
       protected float normalMoveSpeed = 5f; // 普通移动速度

       [Min(0f)] [SerializeField] protected float acceleration = 10f; // 加速度

       [Min(0f)] [SerializeField] protected float deceleration = 15f; // 减速度

       [Header("= 冲刺参数 =")] [Min(0f)] [SerializeField]
       protected float minChargeTime = 0.1f; // 最小蓄力时间

       [Min(0f)] [SerializeField] protected float maxChargeTime = 2f; // 最大蓄力时间

       [Min(0f)] [SerializeField] protected float minDashSpeed = 20f; // 最小冲刺速度

       [Min(0f)] [SerializeField] protected float maxDashSpeed = 35f; // 最大冲刺速度

       [Min(0f)] [SerializeField] protected float dashDuration = 0.5f; // 冲刺持续时间

       [SerializeField] protected AnimationCurve dashSpeedCurve = AnimationCurve.Linear(0, 1, 1, 0); // 冲刺速度曲线

       [Range(0f, 1f)] [SerializeField] protected float dashControlAmount = 0.3f; // 冲刺时的方向控制量

       [SerializeField] protected bool forceNotGrounded = true; // 冲刺时强制离地

       [SerializeField] protected bool cancelDashOnWallHit = true; // 撞墙时取消冲刺

       [Header("= 下砸参数 =")] [Min(0f)] [SerializeField]
       protected float slamSpeed = 30f; // 下砸速度

       [Min(0f)] [SerializeField] protected float slamRadius = 3f; // 下砸影响半径

       [Min(0f)] [SerializeField] protected float slamForce = 10f; // 下砸冲击力

       [Header("= 视觉效果 =")] [SerializeField] protected GameObject chargeEffectPrefab; // 蓄力特效预制体

       [SerializeField] protected GameObject dashEffectPrefab; // 冲刺特效预制体

       [SerializeField] protected GameObject slamEffectPrefab; // 下砸特效预制体

       [SerializeField] protected AnimationCurve compressionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 压缩曲线

       [Range(0.5f, 1f)] [SerializeField] protected float maxCompressionScale = 0.8f; // 最大压缩比例

       // ─────────────────────────────────────────────────────────────────────────────────────────────
       // 内部变量
       // ─────────────────────────────────────────────────────────────────────────────────────────────

       protected GameObject currentCapsule; // 当前胶囊体实例

       //protected MaterialController materialController;        // 材质控制器

       // 状态标志
       protected bool isCharging = false; // 是否正在蓄力
       protected bool isDashing = false; // 是否正在冲刺
       protected bool isSlammingDown = false; // 是否正在下砸
       protected bool isDone = false; // 状态是否完成

       // 计时器
       protected float chargeTimer = 0f; // 蓄力计时器
       protected float dashTimer = 0f; // 冲刺计时器
       protected float currentChargeLevel = 0f; // 当前蓄力等级(0-1)

       // 运动变量
       protected Vector3 dashDirection; // 冲刺方向
       protected Vector3 originalScale; // 原始缩放
       protected float currentSpeedMultiplier = 1f; // 速度倍数

       // 特效实例
       protected GameObject chargeEffect;
       protected GameObject dashEffect;
       protected GameObject slamEffect;

       #region 事件

       /// <summary>
       /// 进入跑快快状态时触发
       /// </summary>
       public event System.Action OnRunFastEnter;

       /// <summary>
       /// 退出跑快快状态时触发
       /// </summary>
       public event System.Action OnRunFastExit;

       /// <summary>
       /// 开始冲刺时触发
       /// </summary>
       public event System.Action<Vector3> OnDashStart;

       /// <summary>
       /// 冲刺结束时触发
       /// </summary>
       public event System.Action<Vector3> OnDashEnd;

       /// <summary>
       /// 下砸着陆时触发
       /// </summary>
       public event System.Action OnSlamImpact;

       #endregion

       // Write your initialization code here
       protected override void Awake()
       {
          base.Awake();
          originalScale = transform.localScale;
       }

       // Write your transitions here
       public override bool CheckEnterTransition(CharacterState fromState)
       {
          Debug.Log("Enter CheckRunFastStateEnterTransition");
          //获取输入处理器
          if (inputHandler == null) return false;
          
          Debug.Log("CheckRunFastStateEnterTransition");
          return fromState is NormalMovement && CharacterActor.IsGrounded; // 只能从NormalMovement状态进入，且必须在地面
          
       }


       // Write your transitions here
       public override void CheckExitTransition()
       {
          //Debug.Log("Enter CheckRunFastStateExitTransition");
          // 按T键退出
          if (Input.GetKeyDown(KeyCode.P))
          {
             Debug.Log("CheckRunFastStateExitTransition");
             CharacterStateController.EnqueueTransition<NormalMovement>();
             return;
          }

          // 如果某个动作完成也可以退出（可选）
          if (isDone)
          {
             CharacterStateController.EnqueueTransition<NormalMovement>();
          }
       }


       // Write your update code here
       public override void UpdateBehaviour(float dt)
       {
          //Debug.Log("UpdateRunFastBehaviour");
          // 更新输入
          HandleInput(dt);
        
          // 根据不同状态更新移动
          if (isDashing)
          {
             UpdateDash(dt);
          }
          else if (isSlammingDown)
          {
             UpdateSlam(dt);
          }
          else
          {
             UpdateNormalMovement(dt);
          }
        
          // 更新视觉效果
          UpdateVisuals(dt);
       }

       public override void PostUpdateBehaviour(float dt)
       {
          // 检查碰撞
          /*if (cancelDashOnWallHit && isDashing)
          {
             if (CharacterActor.WallContact)
             {
                Debug.Log("撞墙，停止冲刺");
                EndDash();
             }
          }*/
       }

       public override void EnterBehaviour(float dt, CharacterState fromState)
       {
          Debug.Log($"进入跑快快状态，从 {fromState.GetType().Name} 状态转换而来");
          
          // 重置所有状态
          ResetState();
        
          // 生成胶囊体
          SpawnCapsule();
        
          // 触发进入事件
          OnRunFastEnter?.Invoke();
        
          // 设置速度倍数
          /*if (!CharacterActor.IsGrounded && materialController != null)
          {
             currentSpeedMultiplier = materialController.CurrentVolume.speedMultiplier;
          }
          else if (materialController != null)
          {
             currentSpeedMultiplier = materialController.CurrentSurface.speedMultiplier * 
                                      materialController.CurrentVolume.speedMultiplier;
          }*/
       }

       public override void ExitBehaviour(float dt, CharacterState toState)
       {
          Debug.Log($"退出跑快快状态，转换到 {toState.GetType().Name} 状态");
        
          // 清理胶囊体
          DestroyCapsule();
        
          // 清理特效
          CleanupEffects();
        
          // 恢复设置
          if (forceNotGrounded)
          {
             CharacterActor.alwaysNotGrounded = false;
          }
        
          // 恢复缩放
          transform.localScale = originalScale;
        
          // 触发退出事件
          OnRunFastExit?.Invoke();
       }
       
       // ─────────────────────────────────────────────────────────────────────────────────────────────
       // 输入处理
       // ─────────────────────────────────────────────────────────────────────────────────────────────
    
       protected virtual void HandleInput(float dt)
       {
          if (inputHandler == null) return;
        
          // 地面状态
          if (CharacterActor.IsGrounded && !isDashing)
          {
             // RB键按下 - 开始蓄力
             if (inputHandler.GetButtonDown("FamiliarAct"))
             {
                StartCharging();
             }
             // RB键持续按住 - 继续蓄力
             else if (inputHandler.GetBool("FamiliarAct") && isCharging)
             {
                UpdateCharging(dt);
             }
             // RB键释放 - 执行冲刺
             else if (inputHandler.GetButtonUp("FamiliarAct") && isCharging)
             {
                ExecuteDash();
             }
          }
          // 空中状态
          else if (!CharacterActor.IsGrounded && !isSlammingDown)
          {
             // RB键按下 - 瞬发下砸
             if (inputHandler.GetButtonDown("FamiliarAct"))
             {
                ExecuteSlam();
             }
          }
       }
       
       // ─────────────────────────────────────────────────────────────────────────────────────────────
       // 普通移动
       // ─────────────────────────────────────────────────────────────────────────────────────────────
    
       protected virtual void UpdateNormalMovement(float dt)
       {
          // 获取移动输入
          Vector2 inputAxes = CharacterActions.movement.value;
        
          if (inputAxes.magnitude > 0.1f)
          {
             // 计算移动方向
             Vector3 movementDirection = new Vector3(inputAxes.x, 0f, inputAxes.y);
             //movementDirection = CharacterActor.TransformVectorToWorld(movementDirection);
             
             movementDirection = CharacterActor.transform.TransformDirection(movementDirection);
             movementDirection.Normalize();
            
             // 应用移动
             float targetSpeed = normalMoveSpeed * currentSpeedMultiplier;
             
             //设置平面速度
             //CharacterActor.SetPlanarVelocity(movementDirection * targetSpeed);
             var v = CharacterActor.Velocity;
             Vector3 planar = new Vector3((movementDirection * targetSpeed).x, 0f, (movementDirection * targetSpeed).y);
             CharacterActor.Velocity = planar + Vector3.up * v.y;

             
          }
          else
          {
             // 减速停止
             Vector3 currentVelocity = CharacterActor.PlanarVelocity;
             currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * dt);
             //CharacterActor.SetPlanarVelocity(currentVelocity);
             
             //设置平面速度
             var v = CharacterActor.Velocity;
             Vector3 planar = new Vector3(currentVelocity.x, 0f, currentVelocity.y);
             CharacterActor.Velocity = planar + Vector3.up * v.y;
          }
       }
       
       // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 蓄力系统
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    protected virtual void StartCharging()
    {
        if (isCharging) return;
        
        Debug.Log("开始蓄力");
        isCharging = true;
        chargeTimer = 0f;
        currentChargeLevel = 0f;
        
        // 生成蓄力特效
        if (chargeEffectPrefab != null)
        {
            chargeEffect = Instantiate(chargeEffectPrefab, transform.position, Quaternion.identity);
            chargeEffect.transform.SetParent(transform);
        }
    }
    
    protected virtual void UpdateCharging(float dt)
    {
        chargeTimer += dt;
        currentChargeLevel = Mathf.Clamp01(chargeTimer / maxChargeTime);
        
        // 更新蓄力特效
        if (chargeEffect != null)
        {
            // 可以根据蓄力等级调整特效
            var particleSystem = chargeEffect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.rateOverTime = currentChargeLevel * 50f;
            }
        }
        
        // 达到最大蓄力时间自动释放
        if (chargeTimer >= maxChargeTime)
        {
            ExecuteDash();
        }
    }
    
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 冲刺系统
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    protected virtual void ExecuteDash()
    {
        if (!isCharging || chargeTimer < minChargeTime) return;
        
        Debug.Log($"执行冲刺！蓄力等级: {currentChargeLevel:F2}");
        
        // 停止蓄力
        isCharging = false;
        if (chargeEffect != null)
        {
            Destroy(chargeEffect);
            chargeEffect = null;
        }
        
        // 计算冲刺方向
        Vector2 inputAxes = CharacterActions.movement.value;
        if (inputAxes.magnitude > 0.1f)
        {
            dashDirection = new Vector3(inputAxes.x, 0f, inputAxes.y);
            //dashDirection = CharacterActor.TransformVectorToWorld(dashDirection);
            dashDirection = CharacterActor.transform.TransformDirection(dashDirection);
            dashDirection.Normalize();
        }
        else
        {
            dashDirection = CharacterActor.Forward;
        }
        
        // 开始冲刺
        isDashing = true;
        dashTimer = 0f;
        
        // 强制离地
        if (forceNotGrounded)
        {
            CharacterActor.alwaysNotGrounded = true;
            CharacterActor.ForceNotGrounded();
        }
        
        // 生成冲刺特效
        if (dashEffectPrefab != null)
        {
            dashEffect = Instantiate(dashEffectPrefab, transform.position, Quaternion.LookRotation(dashDirection));
            dashEffect.transform.SetParent(transform);
        }
        
        // 触发冲刺事件
        OnDashStart?.Invoke(dashDirection);
    }
    
    protected virtual void UpdateDash(float dt)
    {
        dashTimer += dt;
        float dashProgress = dashTimer / dashDuration;
        
        if (dashProgress >= 1f)
        {
            EndDash();
            return;
        }
        
        // 计算冲刺速度
        float dashSpeed = Mathf.Lerp(minDashSpeed, maxDashSpeed, currentChargeLevel);
        dashSpeed *= dashSpeedCurve.Evaluate(dashProgress);
        dashSpeed *= currentSpeedMultiplier;
        
        // 允许微调方向
        Vector2 inputAxes = CharacterActions.movement.value;
        if (inputAxes.magnitude > 0.1f && dashControlAmount > 0f)
        {
            Vector3 inputDirection = new Vector3(inputAxes.x, 0f, inputAxes.y);
            //inputDirection = CharacterActor.TransformVectorToWorld(inputDirection);
            inputDirection = CharacterActor.transform.TransformDirection(inputDirection);
            inputDirection.Normalize();
            
            // 混合输入方向和冲刺方向
            dashDirection = Vector3.Slerp(dashDirection, inputDirection, dashControlAmount * dt);
            dashDirection.Normalize();
        }
        
        // 应用速度
        //CharacterActor.SetVelocity(dashDirection * dashSpeed);
        CharacterActor.Velocity = dashDirection * dashSpeed;
    }
    
    protected virtual void EndDash()
    {
        Debug.Log("冲刺结束");
        
        isDashing = false;
        dashTimer = 0f;
        currentChargeLevel = 0f;
        
        // 恢复设置
        if (forceNotGrounded)
        {
            CharacterActor.alwaysNotGrounded = false;
        }
        
        // 清理特效
        if (dashEffect != null)
        {
            Destroy(dashEffect);
            dashEffect = null;
        }
        
        // 触发结束事件
        OnDashEnd?.Invoke(dashDirection);
    }
    
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 下砸系统
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    protected virtual void ExecuteSlam()
    {
        if (isSlammingDown) return;
        
        Debug.Log("执行下砸！");
        
        isSlammingDown = true;
        
        // 设置下砸速度
        CharacterActor.Velocity = Vector3.down * slamSpeed;
        
        // 生成下砸特效
        if (slamEffectPrefab != null)
        {
            slamEffect = Instantiate(slamEffectPrefab, transform.position, Quaternion.identity);
            slamEffect.transform.SetParent(transform);
        }
    }
    
    protected virtual void UpdateSlam(float dt)
    {
        // 保持下砸速度
        var v = CharacterActor.Velocity;
        v.y = -slamSpeed;
        CharacterActor.Velocity = v;
        
        // 检查是否着陆
        if (CharacterActor.IsGrounded)
        {
            OnSlamLanded();
        }
    }
    
    protected virtual void OnSlamLanded()
    {
        Debug.Log("下砸着陆！");
        
        isSlammingDown = false;
        
        // 清理下砸特效
        if (slamEffect != null)
        {
            Destroy(slamEffect);
            slamEffect = null;
        }
        
        // 创建冲击波效果
        CreateSlamImpact();
        
        // 触发着陆事件
        OnSlamImpact?.Invoke();
    }
    
    protected virtual void CreateSlamImpact()
    {
        // 获取周围的对象
        Collider[] colliders = Physics.OverlapSphere(transform.position, slamRadius);
        
        foreach (Collider col in colliders)
        {
            if (col.gameObject == gameObject) continue;
            
            // 应用冲击力
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (col.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(col.transform.position, transform.position);
                float forceMagnitude = slamForce * (1f - distance / slamRadius);
                
                rb.AddForce(direction * forceMagnitude + Vector3.up * forceMagnitude * 0.5f, ForceMode.Impulse);
            }
            
            // 可以在这里添加伤害逻辑
            // IDamageable damageable = col.GetComponent<IDamageable>();
            // if (damageable != null) { ... }
        }
    }
    
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 视觉效果
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    protected virtual void UpdateVisuals(float dt)
    {
        // 蓄力压缩效果
        if (isCharging)
        {
            float compressionValue = compressionCurve.Evaluate(currentChargeLevel);
            float yScale = Mathf.Lerp(1f, maxCompressionScale, compressionValue);
            float xzScale = Mathf.Lerp(1f, 1f / maxCompressionScale, compressionValue * 0.5f);
            
            transform.localScale = new Vector3(
                originalScale.x * xzScale,
                originalScale.y * yScale,
                originalScale.z * xzScale
            );
        }
        else if (!isDashing && !isSlammingDown)
        {
            // 恢复原始缩放
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, 10f * dt);
        }
        
        // 更新胶囊体位置
        if (currentCapsule != null)
        {
            currentCapsule.transform.localPosition = capsuleOffset;
        }
    }
    
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 辅助方法
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    protected virtual void SpawnCapsule()
    {
        if (capsulePrefab != null)
        {
            currentCapsule = Instantiate(capsulePrefab, transform.position + capsuleOffset, Quaternion.identity);
            currentCapsule.transform.SetParent(transform);
            currentCapsule.transform.localPosition = capsuleOffset;
        }
        else
        {
            // 创建默认胶囊体
            currentCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            currentCapsule.transform.SetParent(transform);
            currentCapsule.transform.localPosition = capsuleOffset;
            currentCapsule.transform.localScale = new Vector3(1.5f, 0.8f, 1.5f);
            
            // 移除碰撞器
            Collider col = currentCapsule.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    protected virtual void DestroyCapsule()
    {
        if (currentCapsule != null)
        {
            Destroy(currentCapsule);
            currentCapsule = null;
        }
    }
    
    protected virtual void CleanupEffects()
    {
        if (chargeEffect != null) Destroy(chargeEffect);
        if (dashEffect != null) Destroy(dashEffect);
        if (slamEffect != null) Destroy(slamEffect);
    }
    
    protected virtual void ResetState()
    {
        isCharging = false;
        isDashing = false;
        isSlammingDown = false;
        isDone = false;
        chargeTimer = 0f;
        dashTimer = 0f;
        currentChargeLevel = 0f;
        dashDirection = Vector3.forward;
    }
    
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // 调试
    // ─────────────────────────────────────────────────────────────────────────────────────────────
    
    void OnDrawGizmosSelected()
    {
        // 绘制下砸范围
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, slamRadius);
    }
    }