using TMPro;
using UnityEngine;

public class DialogueComponent : MonoBehaviour
{
    private TextMeshProUGUI dialogueText;
    public RectTransform npcTransform; // NPC的位置
    private void Awake()
    {
   
        // 在 NPC 的子物体中寻找 TextMeshPro 组件
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
            Debug.Log("组件已找到");
        }
    }
    void Start()
    {
        npcTransform = (RectTransform)transform;
    }

    private void Update()
    {
        if (dialogueText.gameObject.activeSelf && npcTransform != null)
        {
            // 让Text跟随NPC
            dialogueText.rectTransform.position = npcTransform.position + new Vector3(0, 2, 0); // 可以根据需要调整偏移
        }
    }

    public void ShowDialogue(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.gameObject.SetActive(true);
            Debug.Log("已显示！");
        }
    }

    public void HideDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
            Debug.Log("已隐藏");
        }
    }
}
