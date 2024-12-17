using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPC
{
    public string Name;
    public string Personality; // 性格，比如：友好、谨慎、冷漠
    public List<string> Interests; // 兴趣爱好
    public List<string> DialogueHistory; // 对话历史

    public NPC(string name, string personality, List<string> interests)
    {
        Name = name;
        Personality = personality;
        Interests = interests;
        DialogueHistory = new List<string>();
    }

    // 根据性格和兴趣生成对话
    public string GenerateDialogue(string input = null)
    {
        if (input == null)
        {
            // 随机生成话题
            return $"我最近在研究{Interests[Random.Range(0, Interests.Count)]}！";
        }
        else
        {
            // 根据输入生成回应
            if (input.Contains("天气"))
            {
                return Personality == "友好" ? "今天的天气真的不错！" : "嗯，天气无所谓。";
            }
            else
            {
                return Personality == "谨慎" ? "这个话题听起来有点复杂。" : "这很有趣！";
            }
        }
    }
}
