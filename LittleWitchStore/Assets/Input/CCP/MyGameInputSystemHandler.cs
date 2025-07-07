using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Lightbug.CharacterControllerPro.Implementation;

public class InputSystemHandler : InputHandler
{
    [SerializeField] InputActionAsset inputActionsAsset = null;
    [SerializeField] bool filterByActionMap = false;
    [SerializeField] string gameplayActionMap = "Gameplay";
    [SerializeField] bool filterByControlScheme = false;
    [SerializeField] string controlSchemeName = "Keyboard&Mouse";
    


    // 用字典将动作名映射到 InputAction
    Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();

    void Awake()
    {
        if (inputActionsAsset == null)
        {
            Debug.LogError("未设置 InputActionAsset！");
            return;
        }
        inputActionsAsset.Enable();
        // 可选：按控制方案过滤
        /*if(filterByControlScheme)
        {
            var scheme = inputActionsAsset.FindControlScheme(controlSchemeName);
            inputActionsAsset.bindingMask = InputBinding.MaskByGroup(scheme.bindingGroup);
        }*/
        // 将指定 Action Map 内的所有动作加入字典
        if(filterByActionMap)
        {
            var map = inputActionsAsset.FindActionMap(gameplayActionMap);
            foreach(var act in map.actions)
                inputActions.Add(act.name, act);
        }
        else
        {
            // 不过滤则添加资产中所有动作
            foreach(var map in inputActionsAsset.actionMaps)
                foreach(var act in map.actions)
                    inputActions.Add(act.name, act);
        }
    }

    public override bool GetBool(string actionName)
    {
        if(!inputActions.TryGetValue(actionName, out InputAction action))
            return false;
        // 按钮类动作：值大于默认按压阈值则为 true
        return action.ReadValue<float>() >= InputSystem.settings.defaultButtonPressPoint;
    }
    public override float GetFloat(string actionName)
    {
        if(!inputActions.TryGetValue(actionName, out InputAction action))
            return 0f;
        return action.ReadValue<float>();
    }
    public override Vector2 GetVector2(string actionName)
    {
        if (!inputActions.TryGetValue(actionName, out InputAction action))
        {
            return Vector2.zero;
            Debug.Log("没有输出动作：" + actionName);
        }
        // 测试
        Debug.Log(actionName+" >> X输入" + action.ReadValue<Vector2>().x);
        Debug.Log(actionName+" >> Y输入" + action.ReadValue<Vector2>().y);
        return action.ReadValue<Vector2>();
    }
}
