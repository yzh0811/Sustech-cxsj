using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using TMPro;
using static System.Net.WebRequestMethods;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Net.Sockets;
using System.Net;
using System.Collections;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
public class InputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public static bool isGeneratingGraph = false;
    public GameObject portalPrefab; // 传送门预制体
    public Vector3 portalOffset = new Vector3(2, 0, 0); // 传送门相对于玩家的位置偏移
    public string targetSceneName; // 目标场景名称
    public string NPCRole;// 将身份暴露为可配置的字段
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";
    private string graphUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/text2image/sd_xl?access_token=24.db73b730d2bbe318a9c5a42c48a7f2f6.2592000.1736338739.282335-116499673";
    private string poem = "";
    private string flaskUrl = "http://localhost:5000/api/memory";
    private string previousMemory = "";

    // Start is called before the first frame update  
    void Start()
    {
        // 通过场景名称设置 NPC 身份（可选）
        if (string.IsNullOrEmpty(NPCRole))
        {
            NPCRole = GetNPCRoleForCurrentScene();
        }
        inputField.onEndEdit.AddListener(OnInputFieldSubmit);
        
    }

    private void OnInputFieldSubmit(string input)
    {
        // 调用异步方法  
        if (!isGeneratingGraph)
        {
            SendRequestToAI(input);
          
        }
        else
        {
            GenerateGraph(input);
        }
        inputField.text = ""; // 清空输入框  
    }

 
    private string GetNPCRoleForCurrentScene()
    {
        // 根据当前场景名称返回 NPC 身份
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Song dynasty":
                return "宋代词人苏轼";
            case "Tang dynasty":
                return "唐代诗人李白";
            // 添加更多场景配置
            default:
                return "未知身份的诗人";
        }
    }

    private async void SendRequestToAI(string userMessage)
    {
        GameObject dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
        GameObject dialogPrefab = Resources.Load<GameObject>("Dialog/Dialog");
        if (!dialog)
        {
            Instantiate(dialogPrefab);
        }
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        var systemMemoryId = NPCRole == "唐代诗人李白" ? "sm-pmt3q5ia1whqybdb" :
                "sm-2j77vi5yh3rjqkkb";
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = userMessage },
                },
                system = "你在和玩家对话。你现在是AI模拟" + NPCRole +
                ", 请按照史实和系统记忆将自己当作他，模拟他的行为。并且注意从记忆中获取情感数值，以作出不同的行为。",
                enable_system_memory = true,
                system_memory_id = NPCRole == "唐代诗人李白" ? "sm-pmt3q5ia1whqybdb" :
                "sm-2j77vi5yh3rjqkkb",
               
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
                        dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        dialog.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
                        InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = aiResponse.result;
                        //更新记忆
                        StartUpdatingMemory(userMessage, aiResponse.result, systemMemoryId);
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
        var getFlaskUrl = flaskUrl + (NPCRole == "唐代诗人李白" ? "/LB" :
                "/SS");
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

   



    private async void GenerateGraph(string userMessage)
    {
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = $"以“{userMessage}”为题，作一首短诗歌，并将其翻译成英文。只需要返回英文内容。"},
                }
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
                        if (isGeneratingGraph)
                        {
                            poem = aiResponse.result;
                        }
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
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                prompt = "A Flat view 2D map for Unity, 1024x1024 resolution, DPM++ SDE Karras sampler, Pixel Art style, tile map style: rectangle, suitable for character to move, " +
                         "Style is to describe the following poem in ancient china style: " + poem,
                size = "1024x1024",
                negative_prompt = "isometric view",
                step = 50,
                n = 1,
                sampler_index = "DPM++ SDE Karras",
                style = "Pixel Art"
            };

            string jsonData = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            Debug.Log("Sending request to AI: " + graphUrl);
            Debug.Log("Request Data: " + jsonData);

            try
            {
                // 发送 POST 请求  
                HttpResponseMessage response = await client.PostAsync(graphUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    GraphResponse aiResponse = JsonConvert.DeserializeObject<GraphResponse>(jsonResponse);
                    if (aiResponse?.data != null && aiResponse.data.Count > 0)
                    {
                        Debug.Log("AI成功生成图片");
                        string b64Image = aiResponse.data[0].b64_image;

                        // 转换base64到纹理  
                        SaveBase64Image(b64Image);
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        Vector3 spawnPosition = player.transform.position + portalOffset;
                        GameObject portalInstance = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
                        SceneExit script = portalInstance.GetComponent<SceneExit>();
                        if(script != null)
                        {
                            script.newSceneName = targetSceneName;
                        }
                        //// 切换场景  
                        //SceneManager.LoadScene("Level_Generation");
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
    public class GraphResponse
    {
        public List<ImageData> data { get; set; }
    }

    [System.Serializable]
    public class ImageData
    {
        public string b64_image { get; set; }
    }

    private void SaveBase64Image(string base64Image)
    {
        // 移除base64前缀（如果有）  
        if (base64Image.Contains(","))
        {
            base64Image = base64Image.Split(',')[1];
        }

        // 转换为字节数组  
        byte[] imageBytes = Convert.FromBase64String(base64Image);

        // 确定保存路径  
        string folderPath = Path.Combine(Application.dataPath, "Images");

        // 确保文件夹存在  
        Directory.CreateDirectory(folderPath);

        // 生成唯一文件名  
        string fileName = $"generated_image.png";
        string savedImagePath = Path.Combine(folderPath, fileName);

        // 保存图像  
        System.IO.File.WriteAllBytes(savedImagePath, imageBytes);

        Debug.Log($"图像已保存到: {savedImagePath}");
    }

    [System.Serializable]
    public class MemoryUpdate
    {
        public string user;
        public string ai;
        public string memoryId;
        public string memoryContent;
    }

}

