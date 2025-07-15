using UnityEngine;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using Lightbug.CharacterControllerPro.Implementation;

    public class RunFastState : CharacterState
    {
        public CharacterStateController MyCharacterStateController { get; private set; }
        [Space(10)]

        public PlanarMovementParameters planarMovementParameters = new PlanarMovementParameters();

        public VerticalMovementParameters verticalMovementParameters = new VerticalMovementParameters();

        public CrouchParameters crouchParameters = new CrouchParameters();

        public LookingDirectionParameters lookingDirectionParameters = new LookingDirectionParameters();


        [Header("Animation")]

        [SerializeField]
        protected string groundedParameter = "Grounded";

        [SerializeField]
        protected string stableParameter = "Stable";

        [SerializeField]
        protected string verticalSpeedParameter = "VerticalSpeed";

        [SerializeField]
        protected string planarSpeedParameter = "PlanarSpeed";

        [SerializeField]
        protected string horizontalAxisParameter = "HorizontalAxis";

        [SerializeField]
        protected string verticalAxisParameter = "VerticalAxis";

        [SerializeField]
        protected string heightParameter = "Height";

        //-------------------------------------------------跑快快部分--------------------------------------------------
        [Header("跑快快能力部分")] 
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
       
    
       

        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────


        #region Events	

        /// <summary>
        /// Event triggered when the character jumps.
        /// </summary>
        public event System.Action OnJumpPerformed;

        /// <summary>
        /// Event triggered when the character jumps from the ground.
        /// </summary>
        public event System.Action<bool> OnGroundedJumpPerformed;

        /// <summary>
        /// Event triggered when the character jumps while.
        /// </summary>
        public event System.Action<int> OnNotGroundedJumpPerformed;

        #endregion


        protected MaterialController materialController = null;
        protected int notGroundedJumpsLeft = 0;
        protected bool isAllowedToCancelJump = false;
        protected bool wantToRun = false;
        protected float currentPlanarSpeedLimit = 0f;

        protected bool groundedJumpAvailable = false;
        protected Vector3 jumpDirection = default(Vector3);

        protected Vector3 targetLookingDirection = default(Vector3);
        protected float targetHeight = 1f;

        protected bool wantToCrouch = false;
        protected bool isCrouched = false;

        protected PlanarMovementParameters.PlanarMovementProperties currentMotion = new PlanarMovementParameters.PlanarMovementProperties();
        bool reducedAirControlFlag = false;
        float reducedAirControlInitialTime = 0f;
        float reductionDuration = 0.5f;

        protected override void Awake()
        {
            base.Awake();

            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;

            materialController = this.GetComponentInBranch<CharacterActor, MaterialController>();
            MyCharacterStateController = GetComponent<CharacterStateController>();
            if (MyCharacterStateController == null)
            {
                Debug.LogError("Character Controller Pro: No Character State Controller found");
                return;
            }
        }

        protected virtual void OnValidate()
        {
            verticalMovementParameters.OnValidate();
        }

        protected override void Start()
        {
            base.Start();

            targetHeight = CharacterActor.DefaultBodySize.y;

            float minCrouchHeightRatio = CharacterActor.BodySize.x / CharacterActor.BodySize.y;
            crouchParameters.heightRatio = Mathf.Max(minCrouchHeightRatio, crouchParameters.heightRatio);
        }

        protected virtual void OnEnable()
        {
            CharacterActor.OnTeleport += OnTeleport;
        }

        protected virtual void OnDisable()
        {
            CharacterActor.OnTeleport -= OnTeleport;
        }

        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            targetLookingDirection = CharacterActor.Forward;
            isAllowedToCancelJump = false;
        }

        /// <summary>
        /// Gets/Sets the useGravity toggle. Use this property to enable/disable the effect of gravity on the character.
        /// </summary>
        /// <value></value>
        public bool UseGravity
        {
            get => verticalMovementParameters.useGravity;
            set => verticalMovementParameters.useGravity = value;
        }

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
        }

        public override void ExitBehaviour(float dt, CharacterState toState)
        {
            Debug.Log("ExitRunFastState");
            reducedAirControlFlag = false;
            
            //初始化跑快快的参数
            isDashing = false;
            dashTimer = 0f;
        }



        /// <summary>
        /// Reduces the amount of acceleration and deceleration (not grounded state) until the character reaches the apex of the jump 
        /// (vertical velocity close to zero). This can be useful to prevent the character from accelerating/decelerating too quickly (e.g. right after performing a wall jump).
        /// </summary>
        public void ReduceAirControl(float reductionDuration = 0.5f)
        {
            reducedAirControlFlag = true;
            reducedAirControlInitialTime = Time.time;
            this.reductionDuration = reductionDuration;
        }

        void SetMotionValues(Vector3 targetPlanarVelocity)
        {
            float angleCurrentTargetVelocity = Vector3.Angle(CharacterActor.PlanarVelocity, targetPlanarVelocity);

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    currentMotion.acceleration = planarMovementParameters.stableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.stableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.stableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.UnstableGrounded:
                    currentMotion.acceleration = planarMovementParameters.unstableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.unstableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.unstableGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.NotGrounded:

                    if (reducedAirControlFlag)
                    {
                        float time = Time.time - reducedAirControlInitialTime;
                        if (time <= reductionDuration)
                        {
                            currentMotion.acceleration = (planarMovementParameters.notGroundedAcceleration / reductionDuration) * time;
                            currentMotion.deceleration = (planarMovementParameters.notGroundedDeceleration / reductionDuration) * time;
                        }
                        else
                        {
                            reducedAirControlFlag = false;

                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                        }

                    }
                    else
                    {
                        currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                        currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                    }

                    currentMotion.angleAccelerationMultiplier = planarMovementParameters.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;

            }


            // Material values
            if (materialController != null)
            {
                if (CharacterActor.IsGrounded)
                {
                    currentMotion.acceleration *= materialController.CurrentSurface.accelerationMultiplier * materialController.CurrentVolume.accelerationMultiplier;
                    currentMotion.deceleration *= materialController.CurrentSurface.decelerationMultiplier * materialController.CurrentVolume.decelerationMultiplier;
                }
                else
                {
                    currentMotion.acceleration *= materialController.CurrentVolume.accelerationMultiplier;
                    currentMotion.deceleration *= materialController.CurrentVolume.decelerationMultiplier;
                }
            }

        }


        /// <summary>
        /// Processes the lateral movement of the character (stable and unstable state), that is, walk, run, crouch, etc. 
        /// This movement is tied directly to the "movement" character action.
        /// </summary>
        protected virtual void ProcessPlanarMovement(float dt)
        {
            //SetMotionValues();

            float speedMultiplier = materialController != null ?
            materialController.CurrentSurface.speedMultiplier * materialController.CurrentVolume.speedMultiplier : 1f;


            bool needToAccelerate = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, currentPlanarSpeedLimit).sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;

            Vector3 targetPlanarVelocity = default;
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:

                    if (CharacterActor.WasGrounded)
                        currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.StableGrounded:


                    // Run ------------------------------------------------------------
                    if (planarMovementParameters.runInputMode == InputMode.Toggle)
                    {
                        if (CharacterActions.run.Started)
                            wantToRun = !wantToRun;
                    }
                    else
                    {
                        wantToRun = CharacterActions.run.value;
                    }

                    if (wantToCrouch || !planarMovementParameters.canRun)
                        wantToRun = false;


                    if (isCrouched)
                    {
                        currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit * crouchParameters.speedMultiplier;
                    }
                    else
                    {
                        currentPlanarSpeedLimit = wantToRun ? planarMovementParameters.boostSpeedLimit : planarMovementParameters.baseSpeedLimit;
                    }

                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.UnstableGrounded:

                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply(CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);


                    break;
            }

            SetMotionValues(targetPlanarVelocity);


            float acceleration = currentMotion.acceleration;


            if (needToAccelerate)
            {
                acceleration *= currentMotion.angleAccelerationMultiplier;
            }
            else
            {
                acceleration = currentMotion.deceleration;
            }

            CharacterActor.PlanarVelocity = Vector3.MoveTowards(
                CharacterActor.PlanarVelocity,
                targetPlanarVelocity,
                acceleration * dt
            );
        }



        protected virtual void ProcessGravity(float dt)
        {
            if (!verticalMovementParameters.useGravity)
                return;


            verticalMovementParameters.UpdateParameters();


            float gravityMultiplier = 1f;

            if (materialController != null)
                gravityMultiplier = CharacterActor.LocalVelocity.y >= 0 ?
                    materialController.CurrentVolume.gravityAscendingMultiplier :
                    materialController.CurrentVolume.gravityDescendingMultiplier;

            float gravity = gravityMultiplier * verticalMovementParameters.gravity;


            if (!CharacterActor.IsStable)
                CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);


        }


        protected bool UnstableGroundedJumpAvailable => !verticalMovementParameters.canJumpOnUnstableGround && CharacterActor.CurrentState == CharacterActorState.UnstableGrounded;



        public enum JumpResult
        {
            Invalid,
            Grounded,
            NotGrounded
        }

        JumpResult CanJump()
        {
            JumpResult jumpResult = JumpResult.Invalid;

            if (!verticalMovementParameters.canJump)
                return jumpResult;

            if (isCrouched)
                return jumpResult;

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime && groundedJumpAvailable)
                        jumpResult = JumpResult.Grounded;

                    break;
                case CharacterActorState.NotGrounded:

                    if (CharacterActions.jump.Started)
                    {
                        // First check if the "grounded jump" is available. If so, execute a "coyote jump".
                        if (CharacterActor.NotGroundedTime <= verticalMovementParameters.postGroundedJumpTime && groundedJumpAvailable)
                        {
                            jumpResult = JumpResult.Grounded;
                        }
                        else if (notGroundedJumpsLeft != 0)  // Do a 'not grounded' jump
                        {
                            jumpResult = JumpResult.NotGrounded;
                        }
                    }

                    break;
                case CharacterActorState.UnstableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime && verticalMovementParameters.canJumpOnUnstableGround)
                        jumpResult = JumpResult.Grounded;

                    break;
            }

            return jumpResult;
        }



        protected virtual void ProcessJump(float dt)
        {
            ProcessRegularJump(dt);
            ProcessJumpDown(dt);
        }

        #region JumpDown

        protected virtual bool ProcessJumpDown(float dt)
        {
            if (!verticalMovementParameters.canJumpDown)
                return false;

            if (!CharacterActor.IsStable)
                return false;

            if (!CharacterActor.IsGroundAOneWayPlatform)
                return false;

            if (verticalMovementParameters.filterByTag)
            {
                if (!CharacterActor.GroundObject.CompareTag(verticalMovementParameters.jumpDownTag))
                    return false;
            }

            if (!ProcessJumpDownAction())
                return false;

            JumpDown(dt);

            return true;
        }


        protected virtual bool ProcessJumpDownAction()
        {
            return isCrouched && CharacterActions.jump.Started;
        }


        protected virtual void JumpDown(float dt)
        {

            float groundDisplacementExtraDistance = 0f;

            Vector3 groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);

            if (!CharacterActor.IsGroundAscending)
                groundDisplacementExtraDistance = groundDisplacement.magnitude;

            CharacterActor.ForceNotGrounded();

            CharacterActor.Position -=
                CustomUtilities.Multiply(
                    CharacterActor.Up,
                    CharacterConstants.ColliderMinBottomOffset + verticalMovementParameters.jumpDownDistance + groundDisplacementExtraDistance
                );

            CharacterActor.VerticalVelocity -= CustomUtilities.Multiply(CharacterActor.Up, verticalMovementParameters.jumpDownVerticalVelocity);
        }

        #endregion

        #region Jump

        void ResetJump()
        {
            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;
            groundedJumpAvailable = true;
        }

        protected virtual void ProcessRegularJump(float dt)
        {
            
            if (CharacterActor.IsGrounded)
            {
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                {
                    ResetJump();
                }
            }

            if (isAllowedToCancelJump)
            {
                if (verticalMovementParameters.cancelJumpOnRelease)
                {
                    if (CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMaxTime || CharacterActor.IsFalling)
                    {
                        isAllowedToCancelJump = false;
                    }
                    else if (!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMinTime)
                    {
                        // Get the velocity mapped onto the current jump direction
                        Vector3 projectedJumpVelocity = Vector3.Project(CharacterActor.Velocity, jumpDirection);

                        CharacterActor.Velocity -= CustomUtilities.Multiply(projectedJumpVelocity, 1f - verticalMovementParameters.cancelJumpMultiplier);

                        isAllowedToCancelJump = false;
                    }
                }
            }
            else
            {
                JumpResult jumpResult = CanJump();

                switch (jumpResult)
                {
                    case JumpResult.Grounded:
                        groundedJumpAvailable = false;

                        break;
                    case JumpResult.NotGrounded:
                        notGroundedJumpsLeft--;

                        break;

                    case JumpResult.Invalid:
                        return;
                }

                // Events ---------------------------------------------------
                if (CharacterActor.IsGrounded)
                    OnGroundedJumpPerformed?.Invoke(true);
                else
                    OnNotGroundedJumpPerformed?.Invoke(notGroundedJumpsLeft);

                OnJumpPerformed?.Invoke();

                // Define the jump direction ---------------------------------------------------
                jumpDirection = SetJumpDirection();

                // Force "not grounded" state.     
                if (CharacterActor.IsGrounded)
                    CharacterActor.ForceNotGrounded();

                // First remove any velocity associated with the jump direction.
                CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, jumpDirection);
                CharacterActor.Velocity += CustomUtilities.Multiply(jumpDirection, verticalMovementParameters.jumpSpeed);

                if (verticalMovementParameters.cancelJumpOnRelease)
                    isAllowedToCancelJump = true;

            }


        }

        /// <summary>
        /// Returns the jump direction vector whenever the jump action is started.
        /// </summary>
        protected virtual Vector3 SetJumpDirection()
        {
            return CharacterActor.Up;
        }

        #endregion


        void ProcessVerticalMovement(float dt)
        {
            ProcessGravity(dt);
            ProcessJump(dt);
        }


        public override void EnterBehaviour(float dt, CharacterState fromState)
        {
            Debug.Log("EnterRunFastState");
            targetLookingDirection = CharacterActor.Forward;

            CharacterActor.alwaysNotGrounded = false;
            
            //初始化跑快快的参数
            isDashing = false;
            dashTimer = 0f;
            
            // Grounded jump
            groundedJumpAvailable = false;
            if (CharacterActor.IsGrounded)
            {
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                {
                    groundedJumpAvailable = true;
                }
            }

            // Wallside to NormalMovement transition
            if (fromState == CharacterStateController.GetState<WallSlide>())
            {
                // "availableNotGroundedJumps + 1" because the update code will consume one jump!
                notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps + 1;

                // Reduce the amount of air control (acceleration and deceleration) for 0.5 seconds.
                ReduceAirControl(0.5f);
            }

            currentPlanarSpeedLimit = Mathf.Max(CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

            CharacterActor.UseRootMotion = false;
        }

        protected virtual void HandleRotation(float dt)
        {
            HandleLookingDirection(dt);
        }

        void HandleLookingDirection(float dt)
        {
            if (!lookingDirectionParameters.changeLookingDirection)
                return;

            switch (lookingDirectionParameters.lookingDirectionMode)
            {
                case LookingDirectionParameters.LookingDirectionMode.Movement:

                    switch (CharacterActor.CurrentState)
                    {
                        case CharacterActorState.NotGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.notGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.StableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.stableGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.UnstableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.unstableGroundedLookingDirectionMode);

                            break;
                    }

                    break;

                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:

                    if (!CharacterActor.CharacterBody.Is2D)
                        targetLookingDirection = CharacterStateController.MovementReferenceForward;

                    break;

                case LookingDirectionParameters.LookingDirectionMode.Target:

                    targetLookingDirection = (lookingDirectionParameters.target.position - CharacterActor.Position);
                    targetLookingDirection.Normalize();

                    break;
            }

            Quaternion targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetLookingDirection);
            Quaternion currentDeltaRotation = Quaternion.Slerp(Quaternion.identity, targetDeltaRotation, lookingDirectionParameters.speed * dt);

            if (CharacterActor.CharacterBody.Is2D)
                CharacterActor.SetYaw(targetLookingDirection);
            else
                CharacterActor.SetYaw(currentDeltaRotation * CharacterActor.Forward);
        }

        void SetTargetLookingDirection(LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (CharacterStateController.InputMovementReference != Vector3.zero)
                    targetLookingDirection = CharacterStateController.InputMovementReference;
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
            else
            {
                if (CharacterActor.PlanarVelocity != Vector3.zero)
                    targetLookingDirection = Vector3.ProjectOnPlane(CharacterActor.PlanarVelocity, CharacterActor.Up);
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
        }
        
        

        public override void UpdateBehaviour(float dt)
        {
            HandleSize(dt);
            HandleVelocity(dt);
            HandleRotation(dt);
            
            //Debug.Log("UpdateRunFastBehaviour");
            //Runfast部分输入
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
            
        
            // 更新视觉效果
            UpdateVisuals(dt);
        }
        
        protected virtual void HandleInput(float dt)
        {
            Debug.Log("GetBool FamiliarAct"+inputHandler.GetBool("FamiliarAct"));
            Debug.Log("Is Charging"+isCharging);
            //Debug.Log("HandleInput");
            if (inputHandler == null) return;
        
            // 地面状态
            if (CharacterActor.IsGrounded && !isDashing)
            {
                // RB键按下 - 开始蓄力
                if (inputHandler.GetButtonDown("FamiliarAct"))
                {
                   // Debug.Log("Get Button Down FamiliarAct");
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
            Debug.Log("蓄力中");
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
            /*if (chargeTimer >= maxChargeTime)
            {
                ExecuteDash();
            }*/
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
        Debug.Log("Dashing!!!");
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


        public override void PreCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            CharacterStateController.Animator.SetBool(groundedParameter, CharacterActor.IsGrounded);
            CharacterStateController.Animator.SetBool(stableParameter, CharacterActor.IsStable);
            CharacterStateController.Animator.SetFloat(horizontalAxisParameter, CharacterActions.movement.value.x);
            CharacterStateController.Animator.SetFloat(verticalAxisParameter, CharacterActions.movement.value.y);
            CharacterStateController.Animator.SetFloat(heightParameter, CharacterActor.BodySize.y);
        }

        public override void PostCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            // Parameters associated with velocity are sent after the simulation.
            // The PostSimulationUpdate (CharacterActor) might update velocity once more (e.g. if a "bad step" has been detected).
            CharacterStateController.Animator.SetFloat(verticalSpeedParameter, CharacterActor.LocalVelocity.y);
            CharacterStateController.Animator.SetFloat(planarSpeedParameter, CharacterActor.PlanarVelocity.magnitude);
        }

        protected virtual void HandleSize(float dt)
        {
            // Get the crouch input state 
            if (crouchParameters.enableCrouch)
            {
                if (crouchParameters.inputMode == InputMode.Toggle)
                {
                    if (CharacterActions.crouch.Started)
                        wantToCrouch = !wantToCrouch;
                }
                else
                {
                    wantToCrouch = CharacterActions.crouch.value;
                }

                if (!crouchParameters.notGroundedCrouch && !CharacterActor.IsGrounded)
                    wantToCrouch = false;

                if (CharacterActor.IsGrounded && wantToRun)
                    wantToCrouch = false;
            }
            else
            {
                wantToCrouch = false;
            }

            if (wantToCrouch)
                Crouch(dt);
            else
                StandUp(dt);
        }

        void Crouch(float dt)
        {
            CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                CharacterActor.SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y * crouchParameters.heightRatio,
                crouchParameters.sizeLerpSpeed * dt, 
                sizeReferenceType);

            if (validSize)
                isCrouched = true;
        }

        void StandUp(float dt)
        {
            CharacterActor.SizeReferenceType sizeReferenceType = CharacterActor.IsGrounded ?
                CharacterActor.SizeReferenceType.Bottom : crouchParameters.notGroundedReference;

            bool validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y,
                crouchParameters.sizeLerpSpeed * dt,
                sizeReferenceType);

            if (validSize)
                isCrouched = false;
        }


        protected virtual void HandleVelocity(float dt)
        {
            ProcessVerticalMovement(dt);
            ProcessPlanarMovement(dt);
        }
    }
