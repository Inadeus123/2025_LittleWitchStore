using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;


public class Player : MonoBehaviour
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
    
    //----------------------丢脑袋的部分-------------------------
    public GameObject playerHead; //丢出去的头
    public float playerHeadOffset;
    
    //----------------------生成PlayerBody的部分-------------------------
    public GameObject playerBody;
    
    [SerializeField] private float launchSpeed = 10f;    // 弹射力度，可在Inspector调整
    [SerializeField] private float launchAngle = 45f;    // 弹射向上角度（度），可调整
    public string prefabPath = "Prefab";

    [Header("附加冲量 (可选)")]
    public Vector3 extraImpulse = Vector3.zero;   // 例如 new Vector3(0,3,0)
    Rigidbody playerRb;
    
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
        //Debug.Log("Player update");
        if (Input.GetKeyDown(KeyCode.Y))
        {
            controller.EnqueueTransition<RunFastState>();
        }
        
        // 检查是否按下了摆荡
        if (inputSystemHandler.GetButtonDown("Swing"))
        {
            //Debug.Log("Press B, Ready for swing");
            controller.EnqueueTransition<FamiliarSwingState>();
        }

        if (inputSystemHandler.GetButtonDown("ThrowHead"))
        {
            //Debug.Log("Press RB, Ready for throwhead");
            ThrowHead();
        }
        //Debug.Log("CurrentVelocity: " + playerHeadCharacterActor.Velocity);
    }


    void ThrowHead()
    {
        //----------------setactive头部，传送头部到位------------------------
        if (!playerHead.activeSelf)
        {
            playerHead.transform.position = transform.position + new Vector3(0, playerHeadOffset, 0);
            playerHead.SetActive(true);
        }
        //Debug.Log("Player head position: " + playerHead.transform.position);
        CharacterActor playerHeadCharacterActor = playerHead.GetComponent<CharacterActor>();
        playerHeadCharacterActor.Teleport(transform.position + new Vector3(0, playerHeadOffset, 0));
        
        //--------------------------------相机控制权给playerHead--------------------------------
        camera.targetTransform = playerHead.transform;
        camera.bodyObject = playerHead;
        camera.inputHandlerSettings.InputHandler = playerHead.GetComponentInChildren<InputSystemHandler>();
        //--------------------------------本体设置为inactive--------------------------------
        
        this.gameObject.SetActive(false);
        //生成playerbody
        /*if (!playerBody.activeSelf)
        {
            playerBody.transform.position = transform.position;
            playerBody.SetActive(true);
        }*/
        SpawnCharacterBody();
        //--------------------------------PlayerHead加速--------------------------------
        LaunchHead();
    }

    void LaunchHead()
    {
        // 计算弹射方向
        Vector3 forward = transform.forward;         // 物体当前面朝方向
        Vector3 upward = Vector3.up;                 // 世界坐标的向上向量
        float angleRad = launchAngle * Mathf.Deg2Rad; // 转换为弧度
        
        Vector3 launchDirection = Quaternion.AngleAxis(launchAngle, Vector3.Cross(forward, upward)) * forward;

        CharacterActor playerHeadCharacterActor = playerHead.GetComponent<CharacterActor>();
        playerHeadCharacterActor.Velocity = launchDirection * launchSpeed;
    }

    void SpawnCharacterBody()
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"SpawnOnKey: 未找到预制体 {prefabPath}");
            return;
        }
        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        playerHead.GetComponent<PlayerHead>().playerBody = go;
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null)
            rb = go.AddComponent<Rigidbody>();
        Vector3 playerVelocity = Vector3.zero;
        if (characterActor != null)
            playerVelocity = characterActor.Velocity;
        rb.velocity = playerVelocity;        // 直接带初速，效果最“炸弹”
        if (extraImpulse != Vector3.zero)
            rb.AddForce(extraImpulse, ForceMode.Impulse);
    }
}
