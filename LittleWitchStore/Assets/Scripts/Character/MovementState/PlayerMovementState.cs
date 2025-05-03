using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementState : IState
{
    protected PlayerMovementStateMachine stateMachine;
    protected readonly PlayerWithOutCarryingItemMovementData playerWithOutCarryingItemMovementData;
    protected readonly PlayerWithCarryingItemMovementData playerWithCarryingItemMovementData;
    
    
    public PlayerMovementState(PlayerMovementStateMachine playerMovementStateMachine)
    {
        stateMachine = playerMovementStateMachine;
        playerWithOutCarryingItemMovementData = playerMovementStateMachine.Player.PlayerData.PlayerWithOutCarryingItemMovementData;
        playerWithCarryingItemMovementData = playerMovementStateMachine.Player.PlayerData.PlayerWithCarryingItemMovementData;
        
        InitializeData();
    }
    
    private void InitializeData()
    {
        // Initialize data here
    }

    public virtual void Enter()
    {
        
    }

    public virtual void Exit()
    {
        
    }

    public virtual void HandleInput()
    {
        ReadMovementInput();
    }

    public virtual void Update()
    {
        
    }

    public virtual void PhysicsUpdate()
    {
        Move();
    }

    public virtual void OnTriggerEnter(Collider collider)
    {
        
    }

    public virtual void OnTriggerExit(Collider collider)
    {
        
    }

    public virtual void OnAnimationEnterEvent()
    {
        
    }

    public virtual void OnAnimationExitEvent()
    {
        
    }

    public virtual void OnAnimationTransitionEvent()
    {
        
    }
    
     
    private void ReadMovementInput()
    {
        // Read movement input here
        stateMachine.PlayerReusableData.MovementInput = stateMachine.Player.playerInput.PlayerNormalInputActions.Movement.ReadValue<Vector2>();
        //Debug.Log("X: " + stateMachine.PlayerReusableData.MovementInput.x + "Y: " + stateMachine.PlayerReusableData.MovementInput.y);

        
    }

    private void Move()
    {
        // Move the player here
        //Vector3 moveDirection = new Vector3(stateMachine.PlayerReusableData.MovementInput.x, 0, stateMachine.PlayerReusableData.MovementInput.y);
        //Debug.Log(stateMachine.Player.mainCamera.transform.forward);
        //获取相机投射到地图平面的方向
        Vector3 camForward = Vector3.Scale(stateMachine.Player.mainCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = stateMachine.Player.mainCamera.transform.right;
        Vector3 moveDirection = (stateMachine.PlayerReusableData.MovementInput.x * camRight +
                                 stateMachine.PlayerReusableData.MovementInput.y * camForward).normalized;
        //Debug.Log("PlayerMoveDirectionX:" + moveDirection.x + " PlayerMoveDirectionY:" + moveDirection.y + " PlayerMoveDirectionZ:" + moveDirection.z);
        HandleWalk(moveDirection);
    }

    private void HandleWalk(Vector3 moveDirection)
    {
        //Debug.Log("PlayerMoveDirectionX:" + moveDirection.x + " PlayerMoveDirectionY:" + moveDirection.y + " PlayerMoveDirectionZ:" + moveDirection.z);

        //角色移动
        float playerSpeed = moveDirection != Vector3.zero ? playerWithOutCarryingItemMovementData.walkSpeed : 0f; //输入不为0则速度为walkSpeed，否则为0
        Vector3 desiredVelocity = moveDirection * playerSpeed;
        //Debug.Log("PlayerSpeed" + playerSpeed);
        float accelRate = (stateMachine.Player.velocity.magnitude < desiredVelocity.magnitude) ? playerWithOutCarryingItemMovementData.walkAccel : playerWithOutCarryingItemMovementData.walkDecel;
        stateMachine.Player.velocity = Vector3.MoveTowards(stateMachine.Player.velocity, desiredVelocity, accelRate* Time.deltaTime);
        //Debug.Log(stateMachine.Player.velocity);
        //旋转
        if (moveDirection != Vector3.zero)
        {
            var targetRotation = Quaternion.LookRotation(moveDirection);
            stateMachine.Player.transform.rotation = Quaternion.RotateTowards(stateMachine.Player.transform.rotation, targetRotation, playerWithOutCarryingItemMovementData.turnSpeed * Time.deltaTime);
        }
        
        //是否启用重力
        if (playerWithOutCarryingItemMovementData.useGravity)
            stateMachine.Player.velocity.y += playerWithOutCarryingItemMovementData.gravity * Time.deltaTime;
        else
            stateMachine.Player.velocity.y = 0;
        
        stateMachine.Player.playerCharacterController.Move(stateMachine.Player.velocity * Time.deltaTime);
    }
}
