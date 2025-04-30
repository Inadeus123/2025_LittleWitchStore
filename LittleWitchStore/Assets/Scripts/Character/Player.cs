using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
   [field: Header("References")]
   [field: SerializeField] public PlayerSO PlayerData { get; private set; }
   
   public Camera mainCamera;
   
   private CharacterController playerCharacterController;
   Vector3 velocity;

   private void Awake()
   {
      playerCharacterController = GetComponent<CharacterController>();
   }

   private void Start()
   {
      //PlayerMovementStateMachine.ChangeState();
   }
}
