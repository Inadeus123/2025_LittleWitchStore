using UnityEngine;

/// <summary>
/// 运行时配置管理器 - 管理可调整的参数
/// </summary>
public class RuntimeConfigurationManager : MonoBehaviour
{
    [Header("配置引用")]
    [SerializeField] private PoleVaultConfiguration config;
    
    [Header("运行时调试")]
    [SerializeField] private bool enableRuntimeTuning = true;    // 启用运行时调试
    [SerializeField] private KeyCode configToggleKey = KeyCode.F1;  // 配置切换键
    
    private BendablePole[] poles;
    private JoystickController joystickController;
    private bool showConfigUI = false;
    
    void Start()
    {
        // 获取场景中的所有杆子
        poles = FindObjectsOfType<BendablePole>();
        joystickController = FindObjectOfType<JoystickController>();
        
        // 应用配置
        ApplyConfiguration();
    }
    
    void Update()
    {
        // 切换配置UI
        if (enableRuntimeTuning && Input.GetKeyDown(configToggleKey))
        {
            showConfigUI = !showConfigUI;
        }
    }
    
    /// <summary>
    /// 应用配置到所有对象
    /// </summary>
    public void ApplyConfiguration()
    {
        if (config == null) return;
        
        // 应用到杆子
        foreach (var pole in poles)
        {
            ApplyConfigurationToPole(pole);
        }
        
        // 应用到输入控制器
        if (joystickController != null)
        {
            ApplyConfigurationToInput(joystickController);
        }
    }
    
    /// <summary>
    /// 应用配置到杆子
    /// </summary>
    private void ApplyConfigurationToPole(BendablePole pole)
    {
        // 这里需要根据BendablePole的具体实现来应用配置
        // 例如：pole.SetConfiguration(config);
    }
    
    /// <summary>
    /// 应用配置到输入控制器
    /// </summary>
    private void ApplyConfigurationToInput(JoystickController controller)
    {
        // 这里需要根据JoystickController的具体实现来应用配置
        // 例如：controller.SetConfiguration(config);
    }
    
    void OnGUI()
    {
        if (!enableRuntimeTuning || !showConfigUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));
        GUILayout.BeginVertical(GUI.skin.box);
        
        GUILayout.Label("撑杆跳配置", GUI.skin.label);
        
        // 发射力度滑条
        GUILayout.Label($"发射力度: {config.launchForce:F1}");
        config.launchForce = GUILayout.HorizontalSlider(config.launchForce, 5f, 30f);
        
        // 弯曲灵敏度滑条
        GUILayout.Label($"弯曲灵敏度: {config.bendSensitivity:F1}");
        config.bendSensitivity = GUILayout.HorizontalSlider(config.bendSensitivity, 0.5f, 3f);
        
        // 输入死区滑条
        GUILayout.Label($"输入死区: {config.inputDeadzone:F2}");
        config.inputDeadzone = GUILayout.HorizontalSlider(config.inputDeadzone, 0f, 0.5f);
        
        // 应用按钮
        if (GUILayout.Button("应用配置"))
        {
            ApplyConfiguration();
        }
        
        // 重置按钮
        if (GUILayout.Button("重置默认值"))
        {
            ResetToDefaults();
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// 重置为默认值
    /// </summary>
    private void ResetToDefaults()
    {
        config.launchForce = 15f;
        config.bendSensitivity = 2f;
        config.inputDeadzone = 0.1f;
        ApplyConfiguration();
    }
}