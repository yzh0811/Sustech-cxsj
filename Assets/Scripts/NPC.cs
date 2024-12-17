using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

[System.Serializable]
public class NPC
{
    public string Name;
    public string Personality; // �Ը񣬱��磺�Ѻá���������Į
    public List<string> Interests; // ��Ȥ����
    public List<string> DialogueHistory; // �Ի���ʷ
    public string lastSentence;
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";

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
            string topic = Interests[Random.Range(0, Interests.Count)];
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
}
