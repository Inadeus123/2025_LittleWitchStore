using System.Collections.Generic;
using UnityEngine;
using Lightbug.Utilities;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.CharacterControllerPro.Core;

namespace Lightbug.CharacterControllerPro.Demo
{
    

    [AddComponentMenu("Character Controller Pro/Demo/Character/States/Dash")]
    public class ThrowHead : CharacterState
    {
        

        protected override void Awake()
        {
            base.Awake();

            
        }


        public override bool CheckEnterTransition(CharacterState fromState)
        {

            return true;
        }

        public override void CheckExitTransition()
        {
            /*if (isDone)
            {
                if (OnDashEnd != null)
                    OnDashEnd(dashDirection);

                CharacterStateController.EnqueueTransition<NormalMovement>();
            }*/
        }


        public override void EnterBehaviour(float dt, CharacterState fromState)
        {
           
        }

        public override void ExitBehaviour(float dt, CharacterState toState)
        {
           
        }

        public override void UpdateBehaviour(float dt)
        {
           

        }

        public override void PostUpdateBehaviour(float dt)
        {
                    
        }

        public virtual void ResetDash()
        {
          
        }
    }

}





