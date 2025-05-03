using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
   protected IState currentState;
   
   public void ChangeState(IState newState)
   {
      currentState?.Exit();

      currentState = newState;

      currentState.Enter();
   }

   //监听当前状态的输入
   public void HandleInput()
   {
      currentState?.HandleInput();
   }

   
   //执行当前状态的更新
   public void Update()
   {
      currentState?.Update();
   }

   //执行当前状态的物理更新（运动相关）
   public void PhysicsUpdate()
   {
      currentState?.PhysicsUpdate();
   }

   public void OnTriggerEnter(Collider collider)
   {
      currentState?.OnTriggerEnter(collider);
   }

   public void OnTriggerExit(Collider collider)
   {
      currentState?.OnTriggerExit(collider);
   }

   public void OnAnimationEnterEvent()
   {
      currentState?.OnAnimationEnterEvent();
   }

   public void OnAnimationExitEvent()
   {
      currentState?.OnAnimationExitEvent();
   }

   public void OnAnimationTransitionEvent()
   {
      currentState?.OnAnimationTransitionEvent();
   }
}
