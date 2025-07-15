using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class CustomPlatformHandler : MonoBehaviour
{
    public Transform sphereCenter; //感知的球体范围
    public float sphereRadius = 0f; //球体半径
    public LayerMask platformLayer; //平台所在的层
    public float castDistance = 0.2f; //射线检测距离
    public float footRadius = 0.5f; //SphereCast的半径
    public float snapOffset = 0.01f; //脚部位置偏移量

    public KeyCode activateKey = KeyCode.T; //与激活球体的同步
    private CharacterActor characterActor;
    private ShaderInteractorPosition shaderInteractorPosition; //Shader交互位置半径

    private void Start()
    {
        characterActor = GetComponent<CharacterActor>();
        shaderInteractorPosition = sphereCenter.gameObject.GetComponentInParent<ShaderInteractorPosition>();
        if (characterActor == null)
        {
            Debug.LogError("CharacterActor component not found");
            return;
        }
        
    }

    private void Update()
    {
        sphereRadius = shaderInteractorPosition.radius;
        //Debug.Log(sphereRadius);
    }

    private void FixedUpdate()
    {
       bool isSphereActive = Input.GetKeyDown(activateKey);
       if (isSphereActive)
       {
           HandleCustomGroundCheck();
       }
       else
       {
           //未激活
       }
    }

    private void HandleCustomGroundCheck()
    {
        float capsuleHeight = characterActor.BodySize.y;
        float capsuleRadius = characterActor.BodySize.x / 2f;
        footRadius = capsuleRadius;

        Vector3 feetPosition = transform.position;
        Debug.DrawRay(feetPosition, Vector3.down * castDistance, Color.red, 1f);  // 绘制射线，红色，持续1秒
        RaycastHit hit;
        // SphereCast：从脚部向下，方向 Vector3.down，距离 castDistance
        if (Physics.SphereCast(feetPosition, footRadius, Vector3.down, out hit, castDistance, platformLayer))
        {
            Debug.Log("Detected platform at: " + hit.point);
           float distToSphere = Vector3.Distance(hit.point, sphereCenter.position);
           if (distToSphere <= sphereRadius)
           {
               // 检测到平台且在球体范围内,模拟接地
               SimulateGrounded(hit);
           }
        }
        else
        {
            // 未检测到平台，可以添加其他逻辑，比如恢复默认状态等
        }
    }

    private void SimulateGrounded(RaycastHit hit)
    {
        // 如果插件认为未接地，snap 位置并重置垂直速度
        if (!characterActor.IsGrounded)
        {
            // Snap 到击中点 (沿法线偏移，适合坡道)
            Vector3 snapPosition = hit.point + hit.normal * (footRadius + snapOffset);
            transform.position = snapPosition;

            // 重置垂直速度，保持水平速度 (投影到地面法线)
            Vector3 planarVelocity = Vector3.ProjectOnPlane(characterActor.Velocity, hit.normal);
            characterActor.Velocity = planarVelocity;
        }

        // 可选：如果需要跳跃等，NormalMovement 会检查 IsOnGround，但由于我们 snap 了，它可能在下一帧检测到（或自定义跳跃逻辑）
    }
}
