using UnityEngine;

/// <summary>
/// 撑杆跳配置 - 可调整的参数设置
/// </summary>
[CreateAssetMenu(fileName = "PoleVaultConfig", menuName = "Game/Pole Vault Configuration")]
public class PoleVaultConfiguration : ScriptableObject
{
    [Header("杆子物理参数")]
    [Range(5, 20)]
    public int segmentCount = 10;                    // 杆子段数
    
    [Range(0.5f, 2f)]
    public float segmentLength = 1f;                 // 段长度
    
    [Range(0.05f, 0.3f)]
    public float segmentRadius = 0.1f;               // 段半径
    
    [Range(0.1f, 5f)]
    public float segmentMass = 1f;                   // 段质量
    
    [Range(10f, 90f)]
    public float maxBendAngle = 30f;                 // 最大弯曲角度
    
    [Range(50f, 500f)]
    public float springForce = 100f;                 // 弹簧力度
    
    [Range(1f, 50f)]
    public float damping = 10f;                      // 阻尼系数
    
    [Header("输入设置")]
    [Range(0f, 0.5f)]
    public float inputDeadzone = 0.1f;               // 输入死区
    
    [Range(0.1f, 5f)]
    public float inputSensitivity = 1f;              // 输入灵敏度
    
    [Range(0.5f, 3f)]
    public float bendSensitivity = 2f;               // 弯曲灵敏度
    
    public bool invertYAxis = false;                 // Y轴反转
    
    [Header("发射参数")]
    [Range(5f, 30f)]
    public float launchForce = 15f;                  // 发射力度
    
    [Range(0.1f, 2f)]
    public float vaultDuration = 0.8f;               // 撑杆跳持续时间
    
    [Range(0.5f, 5f)]
    public float detectionDistance = 2f;             // 检测距离
    
    [Header("动画设置")]
    [Range(0.1f, 1f)]
    public float bendAnimationSpeed = 0.2f;          // 弯曲动画速度
    
    [Range(0.1f, 1f)]
    public float returnAnimationSpeed = 0.5f;        // 返回动画速度
    
    [Range(0.1f, 1f)]
    public float teleportAnimationSpeed = 0.3f;      // 传送动画速度
    
    [Header("视觉效果")]
    [Range(0f, 2f)]
    public float deformationIntensity = 0.5f;        // 变形强度
    
    public AnimationCurve deformationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("音效设置")]
    [Range(0f, 1f)]
    public float bendSoundVolume = 0.5f;             // 弯曲音效音量
    
    [Range(0f, 1f)]
    public float launchSoundVolume = 0.8f;           // 发射音效音量
    
    [Header("粒子效果")]
    public bool enableParticles = true;              // 启用粒子效果
    
    [Range(0f, 1f)]
    public float particleIntensity = 0.7f;           // 粒子强度
}