using UnityEngine;  
using TMPro;  
using UnityEngine.Networking;  
using UnityEngine.UI;  
using System.Collections;  
using System.Text;  
using Newtonsoft.Json;  

public class NPCInteraction : MonoBehaviour  
{  
    public GameObject player;
    public float interactionDistance = 2f;
    public TaskManager taskManager;  
    public GameObject speechBubble;  
    public TMP_InputField userInputField;  
    public ScrollRect scrollRect;  
    public GameObject messagePrefab;  

    private string apiUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";  
    private string apiKey = "deb0870790e59c7f8d4593e5459b5b59.dn2EGqYLtrmEmhkt";  
    private bool isInteracting = false; // 控制交互状态

    private void Start()  
    {  
        speechBubble.SetActive(false);  
        userInputField.gameObject.SetActive(false);  
        scrollRect.gameObject.SetActive(false);
    }

    private void Update()  
    {  
        if (isInteracting)  
        {  
            Vector3 bubblePosition = transform.position + new Vector3(0,2.0f,0);  
            speechBubble.transform.position = bubblePosition;  
            userInputField.transform.position = bubblePosition - new Vector3(0,1.0f,0);
        }
        if (Vector3.Distance(player.transform.position, transform.position) <= interactionDistance)  
        {  
            if (Input.GetKeyDown(KeyCode.Return))  
            {  
                if (!isInteracting)  
                {  
                    StartInteraction();  
                }  
                else  
                {  
                    SendUserMessage(userInputField.text);  
                }  
            }  
        }  else  
        {  
            if (isInteracting)  
            {  
                EndInteraction();  
            }  
        }
    }  

    private void StartInteraction()  
    {  
        isInteracting = true;  
        speechBubble.SetActive(true);  
        userInputField.gameObject.SetActive(true);  
        scrollRect.gameObject.SetActive(true);  
        userInputField.ActivateInputField();  
    }

    private void SendUserMessage(string message)  
    {  
        if (!string.IsNullOrEmpty(message))  
        {
            AddMessageToChat("你: " + message);
            userInputField.text = "";  
            StartCoroutine(GetResponseFromAI(message)); 

            // 与NPC对话次数加1
            taskManager.IncrementConversationCount(); 
        }
    }

    private IEnumerator GetResponseFromAI(string userMessage)  
    {  
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");  
        string NPCRole = "宋代词人苏轼";
        var requestBody = new  
        {  
            model = "glm-4",  
            messages = new[]  
            {  
                new { role = "user", content = "你现在是" + NPCRole},
                new { role = "assistant", content = "当然。" },
                new { role = "user", content = userMessage },  
            }
        };  

        string jsonData = JsonConvert.SerializeObject(requestBody);  
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);  
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);  
        request.downloadHandler = new DownloadHandlerBuffer();  
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");  
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");  

        yield return request.SendWebRequest();  

        if (request.result == UnityWebRequest.Result.Success)  
        {  
            var jsonResponse = JsonConvert.DeserializeObject<AIResponse>(request.downloadHandler.text);  
            if (jsonResponse != null && jsonResponse.choices.Length > 0)  
            {  
                string aiResponse = jsonResponse.choices[0].message.content;
                AddMessageToChat(NPCRole + ": " + aiResponse);
            }  
            else  
            {  
                Debug.LogError("Invalid response format.");  
            }  
        }  
        else  
        {  
            Debug.LogError("Error: " + request.error + request.downloadHandler.text);
        }  
    }

    private void AddMessageToChat(string message)  
    {  
        GameObject messageObject = Instantiate(messagePrefab, scrollRect.content);  
        messageObject.GetComponent<TextMeshProUGUI>().text = message;  
        messageObject.transform.SetAsLastSibling(); 
        Canvas.ForceUpdateCanvases();  
        scrollRect.verticalNormalizedPosition = 0;  
    }  

    private void EndInteraction()  
    {  
        isInteracting = false;  
        speechBubble.SetActive(false);  
        userInputField.gameObject.SetActive(false); 
        scrollRect.gameObject.SetActive(false); 
    }  
}  

[System.Serializable]  
public class AIResponse  
{  
    public Choice[] choices;  
}  

[System.Serializable]  
public class Choice  
{  
    public Message message;  
}  

[System.Serializable]  
public class Message  
{  
    public string content;  
}  
