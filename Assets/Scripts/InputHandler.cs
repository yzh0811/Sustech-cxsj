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
    public GameObject portalPrefab; // ������Ԥ����
    public Vector3 portalOffset = new Vector3(2, 0, 0); // �������������ҵ�λ��ƫ��
    public string targetSceneName; // Ŀ�곡������
    public string NPCRole;// ����ݱ�¶Ϊ�����õ��ֶ�
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";
    private string graphUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/text2image/sd_xl?access_token=24.db73b730d2bbe318a9c5a42c48a7f2f6.2592000.1736338739.282335-116499673";
    private string poem = "";
    private string flaskUrl = "http://localhost:5000/api/memory";
    private string previousMemory = "";

    // Start is called before the first frame update  
    void Start()
    {
        // ͨ�������������� NPC ��ݣ���ѡ��
        if (string.IsNullOrEmpty(NPCRole))
        {
            NPCRole = GetNPCRoleForCurrentScene();
        }
        inputField.onEndEdit.AddListener(OnInputFieldSubmit);
        
    }

    private void OnInputFieldSubmit(string input)
    {
        // �����첽����  
        if (!isGeneratingGraph)
        {
            SendRequestToAI(input);
          
        }
        else
        {
            GenerateGraph(input);
        }
        inputField.text = ""; // ��������  
    }

 
    private string GetNPCRoleForCurrentScene()
    {
        // ���ݵ�ǰ�������Ʒ��� NPC ���
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Song dynasty":
                return "�δ���������";
            case "Tang dynasty":
                return "�ƴ�ʫ�����";
            // ��Ӹ��ೡ������
            default:
                return "δ֪��ݵ�ʫ��";
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
        var systemMemoryId = NPCRole == "�ƴ�ʫ�����" ? "sm-pmt3q5ia1whqybdb" :
                "sm-2j77vi5yh3rjqkkb";
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = userMessage },
                },
                system = "���ں���ҶԻ�����������AIģ��" + NPCRole +
                ", �밴��ʷʵ��ϵͳ���佫�Լ���������ģ��������Ϊ������ע��Ӽ����л�ȡ�����ֵ����������ͬ����Ϊ��",
                enable_system_memory = true,
                system_memory_id = NPCRole == "�ƴ�ʫ�����" ? "sm-pmt3q5ia1whqybdb" :
                "sm-2j77vi5yh3rjqkkb",
               
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
                        dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        dialog.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
                        InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = aiResponse.result;
                        //���¼���
                        StartUpdatingMemory(userMessage, aiResponse.result, systemMemoryId);
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
        var getFlaskUrl = flaskUrl + (NPCRole == "�ƴ�ʫ�����" ? "/LB" :
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
                    new { role = "user", content = $"�ԡ�{userMessage}��Ϊ�⣬��һ�׶�ʫ�裬�����䷭���Ӣ�ġ�ֻ��Ҫ����Ӣ�����ݡ�"},
                }
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
                    Debug.LogError($"����ʧ��: {response.StatusCode}\n��Ӧ��: {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"�����쳣: {e.Message}");
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
                // ���� POST ����  
                HttpResponseMessage response = await client.PostAsync(graphUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    GraphResponse aiResponse = JsonConvert.DeserializeObject<GraphResponse>(jsonResponse);
                    if (aiResponse?.data != null && aiResponse.data.Count > 0)
                    {
                        Debug.Log("AI�ɹ�����ͼƬ");
                        string b64Image = aiResponse.data[0].b64_image;

                        // ת��base64������  
                        SaveBase64Image(b64Image);
                        GameObject player = GameObject.FindGameObjectWithTag("Player");
                        Vector3 spawnPosition = player.transform.position + portalOffset;
                        GameObject portalInstance = Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
                        SceneExit script = portalInstance.GetComponent<SceneExit>();
                        if(script != null)
                        {
                            script.newSceneName = targetSceneName;
                        }
                        //// �л�����  
                        //SceneManager.LoadScene("Level_Generation");
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
        // �Ƴ�base64ǰ׺������У�  
        if (base64Image.Contains(","))
        {
            base64Image = base64Image.Split(',')[1];
        }

        // ת��Ϊ�ֽ�����  
        byte[] imageBytes = Convert.FromBase64String(base64Image);

        // ȷ������·��  
        string folderPath = Path.Combine(Application.dataPath, "Images");

        // ȷ���ļ��д���  
        Directory.CreateDirectory(folderPath);

        // ����Ψһ�ļ���  
        string fileName = $"generated_image.png";
        string savedImagePath = Path.Combine(folderPath, fileName);

        // ����ͼ��  
        System.IO.File.WriteAllBytes(savedImagePath, imageBytes);

        Debug.Log($"ͼ���ѱ��浽: {savedImagePath}");
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

