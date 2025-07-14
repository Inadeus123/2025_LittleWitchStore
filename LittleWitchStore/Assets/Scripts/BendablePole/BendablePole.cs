using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可弯曲杆子控制器 - 使用连接的胶囊碰撞器创建杆子
/// </summary>
public class BendablePole : MonoBehaviour
{
    [Header("杆子配置")]
    [SerializeField] private int segmentCount = 10;        // 杆子段数
    [SerializeField] private float segmentLength = 1f;     // 每段长度
    [SerializeField] private float segmentRadius = 0.1f;   // 杆子半径
    [SerializeField] private GameObject segmentPrefab;     // 杆子段预制体
    
    [Header("物理参数")]
    [SerializeField] private float segmentMass = 1f;       // 每段质量
    [SerializeField] private float maxBendAngle = 30f;     // 最大弯曲角度
    [SerializeField] private float springForce = 100f;     // 弹簧力度
    [SerializeField] private float damping = 10f;          // 阻尼系数
    
    [Header("材质设置")]
    [SerializeField] private PhysicMaterial poleMaterial;  // 杆子物理材质
    [SerializeField] private Material visualMaterial;      // 视觉材质
    
    // 内部变量
    private List<GameObject> segments = new List<GameObject>();
    private List<ConfigurableJoint> joints = new List<ConfigurableJoint>();
    private Transform poleTop;                             // 杆子顶部引用
    
    void Start()
    {
        CreatePoleSegments();
        ConnectSegments();
        SetupTopReference();
    }
    
    /// <summary>
    /// 创建杆子段
    /// </summary>
    void CreatePoleSegments()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            // 计算段位置
            Vector3 segmentPosition = transform.position + Vector3.up * (i * segmentLength);
            
            // 创建段对象
            GameObject segment = segmentPrefab != null ? 
                Instantiate(segmentPrefab, segmentPosition, Quaternion.identity, transform) :
                CreateSegmentFromScratch(segmentPosition);
                
            segment.name = $"PoleSegment_{i}";
            
            // 设置胶囊碰撞器
            CapsuleCollider capsule = segment.GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = segment.AddComponent<CapsuleCollider>();
                
            capsule.height = segmentLength;
            capsule.radius = segmentRadius;
            capsule.material = poleMaterial;
            
            // 设置刚体
            Rigidbody rb = segment.GetComponent<Rigidbody>();
            if (rb == null)
                rb = segment.AddComponent<Rigidbody>();
                
            rb.mass = segmentMass;
            rb.drag = 0.1f;
            rb.angularDrag = 0.1f;
            
            // 底部段固定
            if (i == 0)
            {
                rb.isKinematic = true;
            }
            
            // 设置视觉材质
            Renderer renderer = segment.GetComponent<Renderer>();
            if (renderer != null && visualMaterial != null)
            {
                renderer.material = visualMaterial;
            }
            
            segments.Add(segment);
        }
    }
    
    /// <summary>
    /// 从头创建段对象
    /// </summary>
    GameObject CreateSegmentFromScratch(Vector3 position)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        segment.transform.position = position;
        segment.transform.SetParent(transform);
        return segment;
    }
    
    /// <summary>
    /// 连接杆子段
    /// </summary>
    void ConnectSegments()
    {
        for (int i = 1; i < segments.Count; i++)
        {
            ConfigurableJoint joint = segments[i].AddComponent<ConfigurableJoint>();
            joint.connectedBody = segments[i - 1].GetComponent<Rigidbody>();
            
            // 设置连接锚点
            joint.anchor = Vector3.down * (segmentLength * 0.5f);
            joint.connectedAnchor = Vector3.up * (segmentLength * 0.5f);
            
            // 锁定位置移动
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            // 设置旋转限制
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
            
            // 设置角度限制
            SoftJointLimit lowAngularLimit = new SoftJointLimit();
            lowAngularLimit.limit = -maxBendAngle;
            
            SoftJointLimit highAngularLimit = new SoftJointLimit();
            highAngularLimit.limit = maxBendAngle;
            
            joint.lowAngularXLimit = lowAngularLimit;
            joint.highAngularXLimit = highAngularLimit;
            
            SoftJointLimit angularYLimit = new SoftJointLimit();
            angularYLimit.limit = maxBendAngle;
            
            SoftJointLimit angularZLimit = new SoftJointLimit();
            angularZLimit.limit = maxBendAngle;
            
            joint.angularYLimit = angularYLimit;
            joint.angularZLimit = angularZLimit;
            
            // 设置弹簧驱动
            JointDrive angularXDrive = new JointDrive();
            angularXDrive.positionSpring = springForce;
            angularXDrive.positionDamper = damping;
            angularXDrive.maximumForce = Mathf.Infinity;
            
            JointDrive angularYZDrive = new JointDrive();
            angularYZDrive.positionSpring = springForce;
            angularYZDrive.positionDamper = damping;
            angularYZDrive.maximumForce = Mathf.Infinity;
            
            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
            
            // 性能优化设置
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            joint.breakForce = 1000f;
            joint.breakTorque = 1000f;
            
            joints.Add(joint);
        }
    }
    
    /// <summary>
    /// 设置顶部引用
    /// </summary>
    void SetupTopReference()
    {
        if (segments.Count > 0)
        {
            poleTop = segments[segments.Count - 1].transform;
        }
    }
    
    /// <summary>
    /// 获取杆子顶部位置
    /// </summary>
    public Vector3 GetTopPosition()
    {
        return poleTop != null ? poleTop.position : transform.position;
    }
    
    /// <summary>
    /// 应用弯曲力
    /// </summary>
    public void ApplyBendingForce(Vector3 direction, float intensity)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            // 计算每段的弯曲权重（顶部更容易弯曲）
            float bendWeight = (float)(i + 1) / joints.Count;
            
            // 计算目标旋转
            Vector3 targetRotation = direction * intensity * maxBendAngle * bendWeight;
            
            // 应用到关节
            joints[i].targetRotation = Quaternion.Euler(targetRotation);
            
            // 动态调整弹簧强度
            JointDrive angularXDrive = new JointDrive();
            angularXDrive.positionSpring = springForce * (1f + intensity);
            angularXDrive.positionDamper = damping;
            angularXDrive.maximumForce = Mathf.Infinity;
            
            JointDrive angularYZDrive = new JointDrive();
            angularYZDrive.positionSpring = springForce * (1f + intensity);
            angularYZDrive.positionDamper = damping;
            angularYZDrive.maximumForce = Mathf.Infinity;
            
            joints[i].angularXDrive = angularXDrive;
            joints[i].angularYZDrive = angularYZDrive;
        }
    }
    
    /// <summary>
    /// 释放弯曲
    /// </summary>
    public void ReleaseBending()
    {
        foreach (var joint in joints)
        {
            joint.targetRotation = Quaternion.identity;
            
            // 恢复默认弹簧强度
            JointDrive angularXDrive = new JointDrive();
            angularXDrive.positionSpring = springForce;
            angularXDrive.positionDamper = damping;
            angularXDrive.maximumForce = Mathf.Infinity;
            
            JointDrive angularYZDrive = new JointDrive();
            angularYZDrive.positionSpring = springForce;
            angularYZDrive.positionDamper = damping;
            angularYZDrive.maximumForce = Mathf.Infinity;
            
            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
        }
    }
}