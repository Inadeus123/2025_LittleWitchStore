using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using UnityEngine.InputSystem;

public class PoleBender : MonoBehaviour
{
    public Transform topCapsule;
    public float bendForce = 20f;

    private Vector2 stickInput;

    void Update()
    {
        stickInput = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        
        Debug.Log(stickInput.x);
        Debug.Log(stickInput.y);
        if (stickInput.sqrMagnitude > 0.01f)
        {
            // 根据摇杆方向，对杆顶施加一个力
            Vector3 forceDir = new Vector3(stickInput.x, 0, stickInput.y);
            topCapsule.GetComponentInChildren<Rigidbody>().AddForce(forceDir * bendForce, ForceMode.Force);
        }
    }
}