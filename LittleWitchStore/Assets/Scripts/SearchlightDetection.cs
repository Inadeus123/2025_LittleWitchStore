using UnityEngine;

public class SearchlightDetection : MonoBehaviour
{
    public Light spotLight;
    public LayerMask obstacleMask; // 设置为障碍物层（排除玩家层）
    public float detectionTime = 1f; // 照到玩家多久后触发发现（防止瞬间触发）

    private float timeInLight = 0f;
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // 找到玩家（确保玩家有"Player" Tag）
        if (player == null)
        {
            Debug.LogWarning("未找到带有'Player' Tag的玩家对象！");
        }
    }

    void Update()
    {
        if (IsPlayerInLight())
        {
            timeInLight += Time.deltaTime;
            if (timeInLight >= detectionTime)
            {
                DetectPlayer();
                timeInLight = 0f; // 可选：重置时间，防止重复触发；或移除以持续触发
            }
        }
        else
        {
            timeInLight = 0f;
        }
    }

    bool IsPlayerInLight()
    {
        //Debug.Log("开始检测中");
        if (player == null) return false;

        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 检查距离是否在光程内
        if (distanceToPlayer > spotLight.range)
        {
            Debug.Log("不在光程内");
            return false;
        }

        // 使用dot product检查是否在光锥角度内
        float angleToPlayer = Vector3.Dot(transform.forward, directionToPlayer);
        float halfAngle = spotLight.spotAngle / 2f;
        if (angleToPlayer < Mathf.Cos(halfAngle * Mathf.Deg2Rad)) return false;

        // 使用raycast检查是否有障碍阻挡
        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer, obstacleMask))
        {
            if (hit.collider.tag != "Player") return false; // 被障碍挡住
        }

        return true;
    }

    void DetectPlayer()
    {
        Debug.Log("已经找到"); // 输出调试消息
        // 这里可以扩展其他逻辑：触发警报、游戏结束、敌人AI等
        // 例如：FindObjectOfType<GameManager>().PlayerDetected();
    }
}