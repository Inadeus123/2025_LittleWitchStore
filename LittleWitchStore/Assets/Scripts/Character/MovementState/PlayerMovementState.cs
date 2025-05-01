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

    public void Enter()
    {
        
    }

    public void Exit()
    {
        
    }

    public void HandleInput()
    {
        ReadMovementInput();
    }

    public void Update()
    {
        
    }

    public void PhysicsUpdate()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        
    }

    public void OnTriggerExit(Collider collider)
    {
        
    }

    public void OnAnimationEnterEvent()
    {
        
    }

    public void OnAnimationExitEvent()
    {
        
    }

    public void OnAnimationTransitionEvent()
    {
        
    }
    
     
    private void ReadMovementInput()
    {
        // Read movement input here
        stateMachine.PlayerReusableData.MovementInput = stateMachine.Player.playerInput.PlayerNormalInputActions.Movement.ReadValue<Vector2>();
        Debug.Log("X: " + stateMachine.PlayerReusableData.MovementInput.x + "Y: " + stateMachine.PlayerReusableData.MovementInput.y);
    }
}
