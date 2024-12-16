using System.Collections.Generic;  
using System.Net.Http;  
using System.Text;  
using System.Threading.Tasks;  
using Newtonsoft.Json;  

[System.Serializable]  
public class NPCMemory  
{  
    private string apiUrl = "https://open.bigmodel.cn/api/paas/v4/chat/completions";  
    private string apiKey = "deb0870790e59c7f8d4593e5459b5b59.dn2EGqYLtrmEmhkt";
    private List<string> memories;  
    private Dictionary<string, int> emotionLevels;  

    public NPCMemory()  
    {  
        memories = new List<string>();  
        emotionLevels = new Dictionary<string, int>();  
    }  

    // 记忆  
    public void Remember(string content)  
    {  
        memories.Add(content);  
        UpdateEmotion("neutral", 1);  
    }  

    // 遗忘  
    public void ForgetMemories(int maxMemories)  
    {  
        if (memories.Count > maxMemories)  
        {  
            memories.RemoveRange(0, memories.Count - maxMemories);  
        }  
    }  

    // 更新情绪方法  
    public void UpdateEmotion(string emotionType, int level)  
    {  
        if (level < 1 || level > 5)  
        {  
            throw new System.ArgumentOutOfRangeException("等级必须在1到5之间。");  
        }  
          
        if (emotionLevels.ContainsKey(emotionType))  
        {  
            emotionLevels[emotionType] = level;  
        }  
        else  
        {  
            emotionLevels.Add(emotionType, level);  
        }  
    }  

    // 评判事件会产生的情绪  
    public async Task EvaluateEmotionAsync(string eventDescription)  
    {  
        var requestBody = new  
        {  
            model = "glm-4",  
            messages = new[]  
            {  
                new { role = "user", content = eventDescription }  
            },  
            max_tokens = 30,  
        };  

        using (var httpClient = new HttpClient())  
        {  
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);  
            var json = JsonConvert.SerializeObject(requestBody);  
            var content = new StringContent(json, Encoding.UTF8, "application/json");  

            var response = await httpClient.PostAsync(apiUrl, content);  
            if (response.IsSuccessStatusCode)  
            {  
                var responseBody = await response.Content.ReadAsStringAsync();  
                var analysisResult = JsonConvert.DeserializeObject<EmotionAnalysisResult>(responseBody);  
                UpdateEmotion(analysisResult.EmotionType, analysisResult.Level);  
            }  
            else  
            {  
                // 错误处理器  
            }  
        }  
    }  

    private class EmotionAnalysisResult  
    {  
        public string EmotionType { get; set; }  
        public int Level { get; set; }  
    }  

    // 获取记忆  
    public string RecallMemory()  
    {  
        if (memories.Count > 0)  
        {  
            return memories[memories.Count - 1];  
        }  
        return "没有记忆。";  
    }  

    // 获取情绪
    public Dictionary<string, int> GetEmotions()  
    {  
        return emotionLevels;  
    }  
}