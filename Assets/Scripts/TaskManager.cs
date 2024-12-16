using UnityEngine;
using TMPro;

public class TaskManager : MonoBehaviour
{
    public int conversationCount = 0; // 当前与NPC对话次数
    public int requiredConversations = 5; // 完成任务所需对话次数
    public bool taskCompleted = false; // 任务完成标志

    public TextMeshProUGUI taskText; // 引用任务文本UI

    private void Start()
    {
        UpdateTaskText(); // 初始化UI文本
    }

    public void IncrementConversationCount()
    {
        if (!taskCompleted)
        {
            conversationCount++;
            CheckTaskCompletion();
            UpdateTaskText(); // 更新UI文本
        }
    }

    private void CheckTaskCompletion()
    {
        if (conversationCount >= requiredConversations)
        {
            taskCompleted = true;
            Debug.Log("任务完成！");
            UpdateTaskText(); // 更新UI文本
        }
    }

    private void UpdateTaskText()
    {
        // 设置任务列表标题
        string taskListHeader = "任务列表\n";
        // 更新对话次数和完成提示
        string taskProgress = $"与NPC对话次数: {conversationCount}/{requiredConversations}";

        if (taskCompleted)
        {
            taskProgress += "            -- 任务完成！";
        }

        // 更新文本
        taskText.text = taskListHeader + taskProgress;
    }
}
