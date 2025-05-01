using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    public Player Player { get; private set; }
    public PlayerReusableData PlayerReusableData { get;}
    public PlayerIdlingState IdlingState { get; private set; }
    public PlayerWalkingState WalkingState { get; private set; }
    public PlayerStoppingState StoppingState { get; private set; }
    
    //Player被击中的几种State
    public PlayerStunnedState StunnedState { get; private set; }
    public PlayerHitReactState HitReactState { get; private set; }
    
    public PlayerMovementStateMachine(Player player)
    {
        Player = player;
        PlayerReusableData = new PlayerReusableData();
        
        //行动State
        IdlingState = new PlayerIdlingState(this);
        WalkingState = new PlayerWalkingState(this);
        StoppingState = new PlayerStoppingState(this);
        
        //受击State
        StunnedState = new PlayerStunnedState(this);
        HitReactState = new PlayerHitReactState(this);
    }
}
