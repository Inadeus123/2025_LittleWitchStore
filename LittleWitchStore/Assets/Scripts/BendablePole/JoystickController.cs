using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 摇杆控制器 - 处理右摇杆输入
/// </summary>
public class JoystickController : MonoBehaviour
{
    [Header("输入配置")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("摇杆设置")]
    [SerializeField] private float deadzone = 0.1f;           // 死区
    [SerializeField] private float sensitivity = 1.0f;        // 灵敏度
    [SerializeField] private bool invertY = false;            // Y轴反转
    
    // 输入动作引用
    private InputAction rightStickAction;
    private InputAction rightStickPressAction;
    private InputAction rightStickReleaseAction;
    
    // 当前输入状态
    private Vector2 currentStickInput;
    private bool isStickPressed;
    
    // 事件委托
    public System.Action<Vector2> OnRightStickMove;
    public System.Action OnRightStickPress;
    public System.Action OnRightStickRelease;
    
    void Awake()
    {
        // 获取输入动作引用
        rightStickAction = inputActions.FindAction("Player/RightStick");
        rightStickPressAction = inputActions.FindAction("Player/RightStickPress");
        rightStickReleaseAction = inputActions.FindAction("Player/RightStickRelease");
    }
    
    void OnEnable()
    {
        // 启用输入动作
        rightStickAction.Enable();
        rightStickPressAction.Enable();
        rightStickReleaseAction.Enable();
        
        // 订阅输入事件
        rightStickAction.performed += OnRightStickInput;
        rightStickAction.canceled += OnRightStickCanceled;
        
        rightStickPressAction.performed += OnRightStickPressed;
        rightStickReleaseAction.performed += OnRightStickReleased;
    }
    
    void OnDisable()
    {
        // 取消订阅输入事件
        rightStickAction.performed -= OnRightStickInput;
        rightStickAction.canceled -= OnRightStickCanceled;
        
        rightStickPressAction.performed -= OnRightStickPressed;
        rightStickReleaseAction.performed -= OnRightStickReleased;
        
        // 禁用输入动作
        rightStickAction.Disable();
        rightStickPressAction.Disable();
        rightStickReleaseAction.Disable();
    }
    
    /// <summary>
    /// 处理摇杆输入
    /// </summary>
    private void OnRightStickInput(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        
        // 应用死区
        if (rawInput.magnitude < deadzone)
        {
            currentStickInput = Vector2.zero;
        }
        else
        {
            // 死区外的输入归一化
            currentStickInput = rawInput.normalized * 
                Mathf.InverseLerp(deadzone, 1f, rawInput.magnitude);
        }
        
        // 应用灵敏度和Y轴反转
        currentStickInput.x *= sensitivity;
        currentStickInput.y *= invertY ? -sensitivity : sensitivity;
        
        // 触发事件
        OnRightStickMove?.Invoke(currentStickInput);
    }
    
    /// <summary>
    /// 摇杆输入取消
    /// </summary>
    private void OnRightStickCanceled(InputAction.CallbackContext context)
    {
        currentStickInput = Vector2.zero;
        OnRightStickMove?.Invoke(currentStickInput);
    }
    
    /// <summary>
    /// 摇杆按下
    /// </summary>
    private void OnRightStickPressed(InputAction.CallbackContext context)
    {
        isStickPressed = true;
        OnRightStickPress?.Invoke();
    }
    
    /// <summary>
    /// 摇杆释放
    /// </summary>
    private void OnRightStickReleased(InputAction.CallbackContext context)
    {
        isStickPressed = false;
        OnRightStickRelease?.Invoke();
    }
    
    // 公共访问方法
    public Vector2 GetCurrentStickInput() => currentStickInput;
    public bool IsStickPressed() => isStickPressed;
}