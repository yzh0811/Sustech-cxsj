using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPC
{
    public string Name;
    public string Personality; // �Ը񣬱��磺�Ѻá���������Į
    public List<string> Interests; // ��Ȥ����
    public List<string> DialogueHistory; // �Ի���ʷ

    public NPC(string name, string personality, List<string> interests)
    {
        Name = name;
        Personality = personality;
        Interests = interests;
        DialogueHistory = new List<string>();
    }

    // �����Ը����Ȥ���ɶԻ�
    public string GenerateDialogue(string input = null)
    {
        if (input == null)
        {
            // ������ɻ���
            return $"��������о�{Interests[Random.Range(0, Interests.Count)]}��";
        }
        else
        {
            // �����������ɻ�Ӧ
            if (input.Contains("����"))
            {
                return Personality == "�Ѻ�" ? "�����������Ĳ���" : "�ţ���������ν��";
            }
            else
            {
                return Personality == "����" ? "��������������е㸴�ӡ�" : "�����Ȥ��";
            }
        }
    }
}
