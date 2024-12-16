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
public class InputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public static bool isGeneratingGraph = false;
    public GameObject portalPrefab; // ������Ԥ����
    public Vector3 portalOffset = new Vector3(2, 0, 0); // �������������ҵ�λ��ƫ��
    public string targetSceneName; // Ŀ�곡������
    public string NPCRole;// �����ݱ�¶Ϊ�����õ��ֶ�
    private string apiUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/chat/ernie-4.0-turbo-128k?access_token=24.0b8b17ad51d7a8c5e8c04417c2bcf5c6.2592000.1736602936.282335-116499673";
    private string graphUrl = "https://aip.baidubce.com/rpc/2.0/ai_custom/v1/wenxinworkshop/text2image/sd_xl?access_token=24.db73b730d2bbe318a9c5a42c48a7f2f6.2592000.1736338739.282335-116499673";
    private string poem = "";
    // Start is called before the first frame update  
    void Start()
    {
        // ͨ�������������� NPC ���ݣ���ѡ��
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
        // ���ݵ�ǰ�������Ʒ��� NPC ����
        string sceneName = SceneManager.GetActiveScene().name;

        switch (sceneName)
        {
            case "Song dynasty":
                return "�δ���������";
            case "Tang dynasty":
                return "�ƴ�ʫ�����";
            // ���Ӹ��ೡ������
            default:
                return "δ֪���ݵ�ʫ��";
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
        using (HttpClient client = new HttpClient())
        {
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = "��������" + NPCRole },
                    new { role = "assistant", content = "��Ȼ��" },
                    new { role = "user", content = userMessage },
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
                        dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                        dialog.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
                        InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = aiResponse.result;
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
}