using TMPro;
using UnityEngine;

public class DialogueComponent : MonoBehaviour
{
    private TextMeshProUGUI dialogueText;
    public RectTransform npcTransform; // NPC��λ��
    private void Awake()
    {
   
        // �� NPC ����������Ѱ�� TextMeshPro ���
        dialogueText = GetComponentInChildren<TextMeshProUGUI>();
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
            Debug.Log("������ҵ�");
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
            // ��Text����NPC
            dialogueText.rectTransform.position = npcTransform.position + new Vector3(0, 2, 0); // ���Ը�����Ҫ����ƫ��
        }
    }

    public void ShowDialogue(string text)
    {
        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.gameObject.SetActive(true);
            Debug.Log("����ʾ��");
        }
    }

    public void HideDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.gameObject.SetActive(false);
            Debug.Log("������");
        }
    }
}
