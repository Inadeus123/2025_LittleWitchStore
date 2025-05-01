using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
   [field: Header("References")]
   [field: SerializeField] public PlayerSO PlayerData { get; private set; }
   
   public Camera mainCamera;
   public PlayerInput playerInput;
   private CharacterController playerCharacterController;
   Vector3 velocity;
   
   private PlayerMovementStateMachine playerMovementStateMachine;

   private void Awake()
   {
      playerCharacterController = GetComponent<CharacterController>();
      playerMovementStateMachine = new PlayerMovementStateMachine(this);
      playerInput = GetComponent<PlayerInput>();
   }

   private void Start()
   {
      playerMovementStateMachine.ChangeState(playerMovementStateMachine.IdlingState);
   }
   
   private void Update()
   {
      // Handle player input and movement here
      playerMovementStateMachine.HandleInput();
   }
}
