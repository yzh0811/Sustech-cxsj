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
            name: npcData.Name,
            personality: npcData.Personality,
            interests: name == "���" ? new List<string> { "����", "����ɽˮ", "��ʫ" } : 
            new List<string> {"����", "�鷨" , "��ʳ"}
        );
    }
}

