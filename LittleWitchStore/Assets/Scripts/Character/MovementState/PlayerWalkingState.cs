using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkingState :PlayerWithOutCarryingItemMovementState
{
    public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.WalkingParameterHash); 
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.WalkingParameterHash);
    }

    //检查是否还有输入，没有输入则跳转到StoppingState
    public override void Update()
    {
        base.Update();
        CheckStopWalking();
    }

    private void CheckStopWalking()
    {
        if (stateMachine.PlayerReusableData.MovementInput == Vector2.zero)
        {
            stateMachine.ChangeState(stateMachine.StoppingState);
        }
    }
    
}
