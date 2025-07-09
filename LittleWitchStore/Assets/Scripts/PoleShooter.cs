using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PoleShooter : MonoBehaviour
{
    public Transform topCapsule;
    public GameObject player;
    public float launchForce = 10f;

    private Vector2 prevStick;
    private bool isAttached = true;

    void Update()
    {
        var currentStick = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;

        if (isAttached)
        {
            // 检测从有方向 → 归零 的瞬间
            if (prevStick.sqrMagnitude > 0.1f && currentStick.sqrMagnitude < 0.01f)
            {
                Vector3 shootDir = new Vector3(prevStick.x, 0.5f, prevStick.y).normalized;
                DetachAndLaunch(shootDir);
            }

            prevStick = currentStick;
        }
    }

    void DetachAndLaunch(Vector3 dir)
    {
        isAttached = false;

        player.transform.SetParent(null);  // 脱离杆体
        var rb = player.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(dir * launchForce, ForceMode.Impulse);
        // 重新启用 CCP 控制器等
    }
}
