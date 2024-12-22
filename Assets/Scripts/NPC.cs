using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using static System.Net.WebRequestMethods;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Net.Sockets;
using System.Net;

[System.Serializable]
public class NPC : MonoBehaviour
{
    public string Name;
    public string Personality; // 性格，比如：友好、谨慎、冷漠
    public List<string> Interests; // 兴趣爱好
    public List<string> DialogueHistory; // 对话历史
    public string lastSentence;
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";
    private string flaskUrl = "http://localhost:5000/api/memory";
    private string previousMemory = "";
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
            string topic = Interests[UnityEngine.Random.Range(0, Interests.Count)];
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
                enable_system_memory = true,
                system_memory_id = NPCRole == "李白" ? "sm-nxdgvcs8fpn4ncmy" :
                "sm-mzw1xv7r6eh3wtej",
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
                enable_system_memory = true,
                system_memory_id = NPCRole == "李白" ? "sm-nxdgvcs8fpn4ncmy" :
                "sm-mzw1xv7r6eh3wtej",
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

    [System.Serializable]
    public class MemoryUpdate
    {
        public string user;
        public string ai;
        public string memoryId;
        public string memoryContent;
    }

    public void StartUpdatingMemory(string userMessage, string aiResponse, string systemMemoryId)
    {

        StartCoroutine(UpdateMemory(userMessage, aiResponse, systemMemoryId));
    }

    public IEnumerator UpdateMemory(string userMessage, string aiResponse, string systemMemoryId)
    {
        // 获取内存信息  
        yield return StartCoroutine(GetMemoryInfo()); // 等待 GetMemoryInfo 完成  
        Debug.Log("传入参数：" + userMessage + aiResponse + systemMemoryId + previousMemory);
        StartCoroutine(SendUpdateInfo(userMessage, aiResponse, systemMemoryId, previousMemory));
    }

    private IEnumerator SendUpdateInfo(string userMessage, string aiResponse, string systemMemoryId, string previousMemory)
    {
        MemoryUpdate memoryUpdate = new MemoryUpdate
        {
            user = userMessage,
            ai = aiResponse,
            memoryId = systemMemoryId,
            memoryContent = previousMemory
        };

        // 将对象转换为 JSON  
        var json = JsonUtility.ToJson(memoryUpdate);

        Debug.Log("生成的 JSON: " + json); // 打印生成的 JSON 以进行调试  


        using (UnityWebRequest webRequest = UnityWebRequest.Put(flaskUrl, json))
        {
            // 设置请求头为 JSON  
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // 发送请求并等待响应  
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Memory updated successfully: " + webRequest.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetMemoryInfo()
    {
        var getFlaskUrl = flaskUrl + (Name == "李白" ? "/LB-NPC" :
                "/SS-NPC");
        using (UnityWebRequest webRequest = UnityWebRequest.Get(flaskUrl))
        {
            // Send the request and wait for a response  
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Handle the response  
                string jsonResponse = webRequest.downloadHandler.text;
                string decodedResponse = Regex.Unescape(jsonResponse);
                Debug.Log(decodedResponse);
                previousMemory = decodedResponse;
                Debug.Log(previousMemory);
            }
        }
    }
}
