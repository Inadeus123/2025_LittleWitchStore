using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class PlayerHead : MonoBehaviour
{
    /// <summary>
    /// Gets the CharacterActor component of the gameObject.
    /// </summary>
    public CharacterActor characterActor { get; private set; }

    /// <summary>
    /// Gets the CharacterBrain component of the gameObject.
    /// </summary>
    // public CharacterBrain CharacterBrain{ get; private set; }
    CharacterBrain characterBrain = null;

    /// <summary>
    /// Gets the current brain actions CharacterBrain component of the gameObject.
    /// </summary>
    private CharacterActions characterActions;
    
    private CharacterStateController controller;
    public InputSystemHandler inputSystemHandler;
    public Camera3D camera;
    public GameObject headGraphic;
    
    public float headRadius = 0.35f;
    
    //----------------------长身体的部分-------------------------
    public GameObject playerBody;
    public GameObject playerMainCharacter; //角色
    public float playerMainCharacterOffset;
    public float playerHeadThrowVelocity;
    
    [SerializeField] private float launchSpeed = 10f;    // 弹射力度，可在Inspector调整
    [SerializeField] private float launchAngle = 45f;    // 弹射向上角度（度），可调整
    private void Start()
    {
        controller = GetComponentInChildren<CharacterStateController>();
        characterActor = GetComponentInChildren<CharacterActor>();
        characterBrain = GetComponentInChildren<CharacterBrain>();
        characterActions = characterBrain.CharacterActions;
        camera = Camera.main.GetComponent<Camera3D>();
        
        if (controller == null || characterActor == null || characterBrain == null || camera == null)
        {
            Debug.LogError("No CharacterStateController/characterActor/characterBrain/Camera3D not found");
            return;
        }
    }
    void Update()
    {
        float speed = characterActor.Velocity.magnitude;                     // m/s

        if (speed < 0.01f) return;                            // 几乎静止，不转

        //--------------------------计算旋转-------------------------------- 
        
        Vector3 up = Vector3.up;
        Vector3 spinAxis = Vector3.Cross(characterActor.Velocity.normalized, up);

        // ω(rad/s) = v / r  → Δ角度 = ω * dt (再转成度)
        float angularSpeed = speed / headRadius;             // rad/s
        float deltaAngleDeg = -angularSpeed * Mathf.Rad2Deg * Time.deltaTime;

        headGraphic.transform.Rotate(spinAxis, deltaAngleDeg, Space.World);
        //Debug.Log("CurrentVelocity: " + characterActor.Velocity);
        
        if (inputSystemHandler.GetButtonDown("ThrowHead"))
        {
            //Debug.Log("Press RB, Ready for throwhead");
            GrowBody();
        }
    }

    void GrowBody()
    {
        if (playerBody != null)
        {
            Destroy(playerBody);
        }

        GameObject cameraReference = GameObject.Find("Camera reference");
        Transform prevCameraTransform =cameraReference.transform;
        //----------------setactive头部，传送头部到位------------------------
        if (!playerMainCharacter.activeSelf)
        {
            playerMainCharacter.transform.position = transform.position + new Vector3(0, playerMainCharacterOffset, 0);
            playerMainCharacter.SetActive(true);
        }
        //Debug.Log("Player head position: " + playerHead.transform.position);
        CharacterActor playerMainCharacterActor = playerMainCharacter.GetComponent<CharacterActor>();
        //playerMainCharacterActor.Teleport(transform.position + new Vector3(0, playerMainCharacterOffset, 0));
        //playerMainCharacterActor.Rotation = characterActor.Rotation;
        
        
        //--------------------------------相机控制权给playerHead--------------------------------
        camera.targetTransform = playerMainCharacter.transform;
        camera.bodyObject = playerMainCharacter;
        camera.inputHandlerSettings.InputHandler = playerMainCharacter.GetComponentInChildren<InputSystemHandler>();
        //cameraReference.transform.rotation = prevCameraTransform.rotation;
        //--------------------------------本体设置为inactive--------------------------------
        this.gameObject.SetActive(false);
        
        //--------------------------------PlayerHead加速--------------------------------
        LaunchBody();
    }
    
    void LaunchBody()
    {
        // 计算弹射方向
        Vector3 forward = transform.forward;         // 物体当前面朝方向
        Vector3 upward = Vector3.up;                 // 世界坐标的向上向量
        float angleRad = launchAngle * Mathf.Deg2Rad; // 转换为弧度
        
        Vector3 launchDirection = Quaternion.AngleAxis(launchAngle, Vector3.Cross(forward, upward)) * forward;

        CharacterActor playerMainCharacterActor = playerMainCharacter.GetComponent<CharacterActor>();
        playerMainCharacterActor.Velocity = launchDirection * launchSpeed;
    }
}
