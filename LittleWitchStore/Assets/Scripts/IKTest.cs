using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKTest : MonoBehaviour
{
   public Transform HandTarget;
   private Animator animator;
   
   private void Start()
   {
      animator = GetComponent<Animator>();
   }
   
   private void OnAnimatorIK(int layerIndex)
   {
      //Debug.Log("IK 正在执行");
      if (animator)
      {
         
         animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
         animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
         animator.SetIKPosition(AvatarIKGoal.LeftHand, HandTarget.position);
         animator.SetIKRotation(AvatarIKGoal.LeftHand, HandTarget.rotation);
      }
   }
}
