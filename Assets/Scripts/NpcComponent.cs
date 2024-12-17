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
            name: npcData.Name,
            personality: npcData.Personality,
            interests: name == "李白" ? new List<string> { "饮酒", "游历山水", "作诗" } : 
            new List<string> {"抚琴", "书法" , "美食"}
        );
    }
}

