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
    public string Personality; // �Ը񣬱��磺�Ѻá���������Į
    public List<string> Interests; // ��Ȥ����
    public List<string> DialogueHistory; // �Ի���ʷ
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

    // �����Ը����Ȥ���ɶԻ�
    public string GenerateDialogue(string otherName, string input = null)
    {
        if (input == null)
        {
            // ������ɻ���
            string topic = Interests[UnityEngine.Random.Range(0, Interests.Count)];
            invokeAIDialog(topic, Name, otherName);
            return lastSentence;
        }
        else
        {
            // �����������ɻ�Ӧ
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
                    new { role = "user", content = "���ݶԷ�����һ�仰" + input + "����һ����ʵĻظ����ӡ�" },
                },
                system = "���ں�" + OtherNPCRole + "�Ի�����������AIģ��" + NPCRole +
                ", �밴��ʷʵ��ϵͳ���佫�Լ���������ģ��������Ϊ��",
                enable_system_memory = true,
                system_memory_id = NPCRole == "���" ? "sm-nxdgvcs8fpn4ncmy" :
                "sm-mzw1xv7r6eh3wtej",
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.Log("Sending request to AI: " + apiUrl);
            Debug.Log("Request Data: " + jsonData);

            try
            {
                // ���� POST ����  
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                    if (aiResponse != null)
                    {
                        Debug.Log("����AI�Ļظ���" + aiResponse.result);
                        lastSentence = aiResponse.result;
                    }
                    else
                    {
                        Debug.LogError("Invalid response format.");
                    }
                }
                else
                {
                    Debug.LogError($"����ʧ��: {response.StatusCode}\n��Ӧ��: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"�����쳣: {e.Message}");
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
                    new { role = "user", content = "��������" + topic + "����һ����ʵĿ����Ի��ľ��ӡ�" },
                },
                system = "���ں�" + OtherNPCRole + "�Ի�����������AIģ��" + NPCRole + 
                ", �밴��ʷʵ��ϵͳ���佫�Լ���������ģ��������Ϊ��",
                enable_system_memory = true,
                system_memory_id = NPCRole == "���" ? "sm-nxdgvcs8fpn4ncmy" :
                "sm-mzw1xv7r6eh3wtej",
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.Log("Sending request to AI: " + apiUrl);
            Debug.Log("Request Data: " + jsonData);

            try
            {
                // ���� POST ����  
                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = JsonConvert.DeserializeObject<AIResponse>(jsonResponse);
                    if (aiResponse != null)
                    {
                        Debug.Log("����AI�Ļظ���" + aiResponse.result);
                        lastSentence = aiResponse.result;
                    }
                    else
                    {
                        Debug.LogError("Invalid response format.");
                    }
                }
                else
                {
                    Debug.LogError($"����ʧ��: {response.StatusCode}\n��Ӧ��: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"�����쳣: {e.Message}");
            }
        }
    }

    private class AIResponse
    {
        public string result; // ������ result �ֶ�  
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
        // ��ȡ�ڴ���Ϣ  
        yield return StartCoroutine(GetMemoryInfo()); // �ȴ� GetMemoryInfo ���  
        Debug.Log("���������" + userMessage + aiResponse + systemMemoryId + previousMemory);
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

        // ������ת��Ϊ JSON  
        var json = JsonUtility.ToJson(memoryUpdate);

        Debug.Log("���ɵ� JSON: " + json); // ��ӡ���ɵ� JSON �Խ��е���  


        using (UnityWebRequest webRequest = UnityWebRequest.Put(flaskUrl, json))
        {
            // ��������ͷΪ JSON  
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // �������󲢵ȴ���Ӧ  
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
        var getFlaskUrl = flaskUrl + (Name == "���" ? "/LB-NPC" :
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
