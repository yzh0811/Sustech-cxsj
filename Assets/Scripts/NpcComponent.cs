using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcComponent : MonoBehaviour
{
    public NPC npcData;

    private void Start()
    {
        // ��ʼ�� NPC ����
        npcData = new NPC(
            name: "NPC_" + Random.Range(1, 100),
            personality: Random.value > 0.5f ? "�Ѻ�" : "����",
            interests: new List<string> { "����", "����", "ð��" }
        );
    }
}

