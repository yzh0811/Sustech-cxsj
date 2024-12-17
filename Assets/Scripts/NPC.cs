using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

[System.Serializable]
public class NPC
{
    public string Name;
    public string Personality; // 性格，比如：友好、谨慎、冷漠
    public List<string> Interests; // 兴趣爱好
    public List<string> DialogueHistory; // 对话历史
    public string lastSentence;
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";

    public NPC(string name, string personality, List<string> interests)
    {
        Name = name;
        Personality = personality;
        Interests = interests;
        DialogueHistory = new List<string>();
    }

    // 根据性格和兴趣生成对话
    public string GenerateDialogue(string otherName, string input = null)
    {
        if (input == null)
        {
            // 随机生成话题
            string topic = Interests[Random.Range(0, Interests.Count)];
            invokeAIDialog(topic, Name, otherName);
            return lastSentence;
        }
        else
        {
            // 根据输入生成回应
            continueAIDialog(input, Name, otherName);
            return lastSentence;
        }
    }


    public async void continueAIDialog(string input, string NPCRole, string OtherNPCRole)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = "根据对方的上一句话" + input + "生成一句合适的回复句子。" },
                },
                system = "你在和" + OtherNPCRole + "对话。你现在是AI模拟" + NPCRole +
                ", 请按照史实和系统记忆将自己当作他，模拟他的行为。",
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.Log("Sending request to AI: " + apiUrl);
            Debug.Log("Request Data: " + jsonData);

            try
            {
                // 发送 POST 请求  
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                    if (aiResponse != null)
                    {
                        Debug.Log("来自AI的回复：" + aiResponse.result);
                        lastSentence = aiResponse.result;
                    }
                    else
                    {
                        Debug.LogError("Invalid response format.");
                    }
                }
                else
                {
                    Debug.LogError($"请求失败: {response.StatusCode}\n响应体: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"请求异常: {e.Message}");
            }
        }
    }

    public async void invokeAIDialog(string topic, string NPCRole, string OtherNPCRole)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = "根据主题" + topic + "生成一句合适的开启对话的句子。" },
                },
                system = "你在和" + OtherNPCRole + "对话。你现在是AI模拟" + NPCRole + 
                ", 请按照史实和系统记忆将自己当作他，模拟他的行为。",
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.Log("Sending request to AI: " + apiUrl);
            Debug.Log("Request Data: " + jsonData);

            try
            {
                // 发送 POST 请求  
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                    if (aiResponse != null)
                    {
                        Debug.Log("来自AI的回复：" + aiResponse.result);
                        lastSentence = aiResponse.result;
                    }
                    else
                    {
                        Debug.LogError("Invalid response format.");
                    }
                }
                else
                {
                    Debug.LogError($"请求失败: {response.StatusCode}\n响应体: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"请求异常: {e.Message}");
            }
        }
    }

    private class AIResponse
    {
        public string result; // 仅保持 result 字段  
    }
}
