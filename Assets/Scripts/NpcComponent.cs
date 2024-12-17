using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcComponent : MonoBehaviour
{
    public NPC npcData;

    private void Start()
    {
        // 初始化 NPC 数据
        npcData = new NPC(
            name: "NPC_" + Random.Range(1, 100),
            personality: Random.value > 0.5f ? "友好" : "谨慎",
            interests: new List<string> { "钓鱼", "种田", "冒险" }
        );
    }
}

