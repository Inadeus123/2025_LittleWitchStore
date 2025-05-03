using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
   [field: Header("References")]
   [field: SerializeField] public PlayerSO PlayerData { get; private set; }
   
   public Camera mainCamera;
   public PlayerInput playerInput;
   public CharacterController playerCharacterController;
   [ReadOnly] public Vector3 velocity;
   
   private PlayerMovementStateMachine playerMovementStateMachine;

   private void Awake()
   {
      playerCharacterController = GetComponent<CharacterController>();
      playerMovementStateMachine = new PlayerMovementStateMachine(this);
      playerInput = GetComponent<PlayerInput>();
      
      mainCamera = Camera.main;
   }

   private void Start()
   {
      playerMovementStateMachine.ChangeState(playerMovementStateMachine.IdlingState);
   }
   
   private void Update()
   {
      playerMovementStateMachine.HandleInput();
      playerMovementStateMachine.Update();
   }

   private void FixedUpdate()
   {
      playerMovementStateMachine.PhysicsUpdate();
   }
   
   private void OnTriggerEnter(Collider other)
   {
      playerMovementStateMachine.OnTriggerEnter(other);
   }
   
   private void OnTriggerExit(Collider other)
   {
      playerMovementStateMachine.OnTriggerExit(other);
   }
   
   public void OnMovementStateAnimationEnterEvent()
   {
      playerMovementStateMachine.OnAnimationEnterEvent();
   }
   
   public void OnMovementStateAnimationExitEvent()
   {
      playerMovementStateMachine.OnAnimationExitEvent();
   }
   
   public void OnMovementStateAnimationTransitionEvent()
   {
      playerMovementStateMachine.OnAnimationTransitionEvent();
   }
}
