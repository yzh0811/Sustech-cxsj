using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using TMPro.Examples;
public class NPC2 : MonoBehaviour
{
    private bool isCollisionHandled = false;
    private bool isInDialogue = false; // ��ӱ�־λ
    private bool isProcessingState = false;
    //ai����
    private string apiKey = "deb0870790e59c7f8d4593e5459b5b59.dn2EGqYLtrmEmhkt";
    //�Ի��򲿷�
    public TMP_InputField userInputField;  // ��Ҫ��Inspector�а�
    //�Ի�����ʾ
    public bool isInputFieldActive = false;
    //task
    public TaskManager taskManager;
    //�ٶ�
    public float speed = 120;
    //��ȡ����
    public Rigidbody2D rigidbody;
    //������
    public Animator animator;
    //����
    public Vector2 vector2;
    //״̬ 0 �������� 1 ������ײ 2 ��ײ������ 3 �Ի� 4 ���� 5 ��� 6 npc��ײ
    public int npcState = 0;
    //�೤ʱ��任npc���ƶ�����
    public float changeDirectionTime = 0;
    //���ʱ��
    public float randomTime = 1;
    //���� x -1 �� 0 ���� 1 �� Input.GetAxisRaw("Horizontal");
    public float[] xDirection = { -1, 0, 1 };
    //y -1 ���� 0 ���� 1 ���� Input.GetAxisRaw("Veritical")��
    public float[] yDirection = { -1, 0, 1 };
    //���x��y
    public int xIndex = 0;
    public int yIndex = 0;
    //���յ�����
    public float x = 0;
    public float y = 0;
    //ǰһ��λ�Ƶ�����
    public float beforeX = 0;
    public float beforeY = 0;

    //-------��ײ
    //������ײʱ��
    public float stayingTime = 0;
    //������ײ�������ִ��ʱ��
    public float touchNormalTime = 0;
    //������ײ�������ִ��ʱ��
    public float touchStayTime = 0;
    //����������ײ�������
    public int changDirectionIndex = 0;
    //-------��ײ

    //-------��ײ���
    //��ײ���flag
    public bool touchPlayer = false;
    //��ײ��ҵ�ʱ��
    public float touchPlayerTime = 0;
    //-------��ײ���

    //-------��ײnpc
    //��ײnpc��flag
    public bool touchNpc = false;
    //��ײnpc��ʱ��
    public float touchNpcTime = 0;
    //-------��ײnpc


    //-------�Ի� 
    public bool firstTalk = true;
    public string npcName;
    public string dynasty;
    //��ǰ�øж�
    public int npcFavorLevel = 0;
    //���øж�
    public int maxNpcFavorLevel = 5;
    //��ǰ�øж�
    public int currentFavor = 0;
    //�����øж���Ҫ����ֵ
    public int maxFavor = 0;
    //�ж�npc�Ƿ��ڵȴ��Ի���Ӧ
    public bool isWaitingForResponse = false;
    //����������һ�ֶԻ�������
    public string previousMessage = "";
    //-------�Ի�
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (npcState != 2)
        {
            //�������
            ChangeAnim();
        }
        else
        {
            animator.SetBool("lefting", false);
            animator.SetBool("righting", false);
            animator.SetBool("uping", false);
            animator.SetBool("downing", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
            animator.SetBool("up", false);
            animator.SetBool("down", true);
        }
        //�øж�������ϵ
        FavorUpdateLevel();
        if (Input.GetKeyDown(KeyCode.Return))  // ���»س���
        {
            userInputField.gameObject.SetActive(!isInputFieldActive);
            isInputFieldActive = !isInputFieldActive;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            isCollisionHandled = false;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (InputHandler.isGeneratingGraph)
            {
                InputHandler.isGeneratingGraph = false;
                userInputField.placeholder.GetComponent<TMP_Text>().text = "Enter your message here...";
            }
            else
            {
                InputHandler.isGeneratingGraph = true;
                userInputField.placeholder.GetComponent<TMP_Text>().text = "Generating Graph...";
            }
        }
    }

    private void FixedUpdate()
    {
        //������
        NpcController();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCollisionHandled) return; // �����ײ�Ѿ��������ֱ�ӷ���

        if (collision.gameObject.tag == "Player")
        {
            npcState = 2;
            rigidbody.bodyType = RigidbodyType2D.Static;
            touchPlayer = true;
        }
        else if (collision.gameObject.CompareTag("NPC"))
        {
            touchWithNpc(collision.gameObject);
            npcState = 6;
        }
        else
        {
            npcState = 1;
        }

        isCollisionHandled = true; // �����ײ�Ѵ���
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            npcState = 2;
            rigidbody.bodyType = RigidbodyType2D.Static;

        }else if(collision.gameObject.tag == "NPC")
        {
            rigidbody.bodyType = RigidbodyType2D.Static;
            npcState = 6;
        }
        else
        {
            npcState = 1;
        }

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (isProcessingState)
        {
            return;
        }
        npcState = 0;
        stayingTime = 0;
        touchStayTime = 0;
        touchNormalTime = 0;
        //��ײ��Һ�npc
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        touchPlayerTime = 0;
        touchNpcTime = 0;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        touchPlayer = false;
    }
    //npc������
    public void NpcController()
    {
        if (isProcessingState) return;

        switch (npcState)
        {
            case 0:
                NormalMove();
                break;
            case 1:
                TouchMove();
                break;
            case 2:
                touchWithPlayer();
                break;
            case 6:
                break;
        }
    }

    //�������ߵ�ʱ�򣬻���Ҫִ��ʲô
    public void NormalMove()
    {
        changeDirectionTime += Time.fixedDeltaTime;
        vector2.x = x;
        vector2.y = y;
        vector2.Normalize();
        rigidbody.velocity = speed * vector2 * Time.fixedDeltaTime;

        if (changeDirectionTime >= randomTime)
        {
            beforeX = x;
            beforeY = y;
            System.Random random = new System.Random();
            randomTime = random.Next(1, 5);
            xIndex = random.Next(0, 3);
            yIndex = random.Next(0, 3);
            //���ǵ�xyͬʱ�ƶ����������
            while (xIndex != 1 && yIndex != 1)
            {
                xIndex = random.Next(0, 3);
                yIndex = random.Next(0, 3);
            }
            x = xDirection[xIndex];
            y = yDirection[yIndex];
            changeDirectionTime = 0;
        }
    }
    //�������
    public void ChangeAnim()
    {
        //����
        if (x == -1 && y == 0)
        {
            animator.SetBool("lefting", true);
            animator.SetBool("righting", false);
            animator.SetBool("uping", false);
            animator.SetBool("downing", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
        }//����
        else if (x == 1 && y == 0)
        {
            animator.SetBool("lefting", false);
            animator.SetBool("righting", true);
            animator.SetBool("uping", false);
            animator.SetBool("downing", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
        }//����
        else if (x == 0 && y == 1)
        {
            animator.SetBool("lefting", false);
            animator.SetBool("righting", false);
            animator.SetBool("uping", true);
            animator.SetBool("downing", false);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
        }//����
        else if (x == 0 && y == -1)
        {
            animator.SetBool("lefting", false);
            animator.SetBool("righting", false);
            animator.SetBool("uping", false);
            animator.SetBool("downing", true);
            animator.SetBool("left", false);
            animator.SetBool("right", false);
            animator.SetBool("up", false);
            animator.SetBool("down", false);
        }
        else if (x == 0 && y == 0)
        {//ֹͣ
            if (beforeX == 0 && beforeY == 0)
            {
                animator.SetBool("lefting", false);
                animator.SetBool("righting", false);
                animator.SetBool("uping", false);
                animator.SetBool("downing", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("up", false);
                animator.SetBool("down", true);
            }//������
            else if (beforeX == -1 && beforeY == 0)
            {
                animator.SetBool("lefting", false);
                animator.SetBool("righting", false);
                animator.SetBool("uping", false);
                animator.SetBool("downing", false);
                animator.SetBool("left", true);
                animator.SetBool("right", false);
                animator.SetBool("up", false);
                animator.SetBool("down", false);
            }//����
            else if (beforeX == 1 && beforeY == 0)
            {
                animator.SetBool("lefting", false);
                animator.SetBool("righting", false);
                animator.SetBool("uping", false);
                animator.SetBool("downing", false);
                animator.SetBool("left", false);
                animator.SetBool("right", true);
                animator.SetBool("up", false);
                animator.SetBool("down", false);
            }//������
            else if (beforeX == 0 && beforeY == 1)
            {
                animator.SetBool("lefting", false);
                animator.SetBool("righting", false);
                animator.SetBool("uping", false);
                animator.SetBool("downing", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("up", true);
                animator.SetBool("down", false);
            }//������
            else if (beforeX == 0 & beforeY == -1)
            {
                animator.SetBool("lefting", false);
                animator.SetBool("righting", false);
                animator.SetBool("uping", false);
                animator.SetBool("downing", false);
                animator.SetBool("left", false);
                animator.SetBool("right", false);
                animator.SetBool("up", false);
                animator.SetBool("down", true);
            }
        }
    }

    //��ײ
    public void TouchMove()
    {
        stayingTime += Time.fixedDeltaTime;
        touchNormalTime += Time.fixedDeltaTime;
        touchStayTime += Time.fixedDeltaTime;
        //������
        if (stayingTime <= 3)
        {
            if (touchNormalTime >= 0.6)
            {
                beforeX = x;
                beforeY = y;
                switch (x)
                {
                    case -1:
                        x = 1;
                        break;
                    case 1:
                        x = -1;
                        break;
                }
                switch (y)
                {
                    case -1:
                        y = 1;
                        break;
                    case 1:
                        y = -1;
                        break;
                }
                if (x == 0 && y == 0)
                {
                    while (xIndex != 1 && yIndex != 1)
                    {
                        //��д�����
                        System.Random random = new System.Random();
                        xIndex = random.Next(0, 3);
                        yIndex = random.Next(0, 3);
                    }
                    x = xDirection[xIndex];
                    y = yDirection[yIndex];
                }
                touchNormalTime = 0;
            }


        }
        else
        {
            //�������
            if (touchStayTime >= 0.6)
            {
                beforeX = x;
                beforeY = y;
                switch (changDirectionIndex)
                {
                    case 0:
                        x = -1;
                        y = 0;
                        changDirectionIndex = 1;
                        break;
                    case 1:
                        x = 1;
                        y = 0;
                        changDirectionIndex = 2;
                        break;
                    case 2:
                        x = 0;
                        y = -1;
                        changDirectionIndex = 3;
                        break;
                    case 3:
                        x = 0;
                        y = 1;
                        changDirectionIndex = 0;
                        break;
                 
                }
                touchStayTime = 0;
            }
        }
        npcState = 0;
    }

    //�������
    public void touchWithPlayer()
    {
        touchPlayerTime += Time.fixedDeltaTime;
        if (touchPlayerTime >= 5)
        {
            rigidbody.bodyType = RigidbodyType2D.Static;
            npcState = 0;
            touchPlayerTime = 0;
        }
        else
        {
            //�ڼ���û�а���E��
            if (touchPlayer == true && Input.GetKeyDown(KeyCode.E))
            {
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                var playerPos = Camera.main.WorldToScreenPoint(player.position);
                gameObject.transform.GetChild(0).GetChild(0).GetChild(0).position = new Vector3(playerPos.x, playerPos.y, 0);
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
    public void touchWithNpc(GameObject otherNpc)
    {
        isProcessingState = true;
        if (touchNpc) return;
        touchNpc = true;
        if (isInDialogue) return; // ��ֹ�ظ������Ի�
        isInDialogue = true;
        // ֹͣ���ߵ�����ƶ�
        this.rigidbody.bodyType = RigidbodyType2D.Static;
        otherNpc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Debug.Log("npc�໥����");
        // ��ʼ�Ի�Э��
        StartCoroutine(StartDialogueWithAI(otherNpc));
    }
   
    private IEnumerator StartDialogueWithAI(GameObject otherNpc)
    {
        // ��ȡ��λ NPC �� AI ����
        NPC thisNpcAI = GetComponent<NpcComponent>().npcData;
        NPC otherNpcAI = otherNpc.GetComponent<NpcComponent>().npcData;

        int dialogueRounds = Random.Range(2, 5); // �Ի�����
        for (int i = 0; i < dialogueRounds; i++)
        {
            // ��ǰ NPC ˵��
            string thisDialogue = thisNpcAI.GenerateDialogue(otherNpcAI.Name);
            if(thisDialogue != null)
            {
                Debug.Log("�Ի����ݲ�Ϊ��");
            }
            thisNpcAI.DialogueHistory.Add(thisDialogue);
            GetComponent<DialogueComponent>().ShowDialogue(thisDialogue);
            yield return new WaitForSeconds(2);

            // ��һ�� NPC ��Ӧ
            string otherDialogue = otherNpcAI.GenerateDialogue(thisNpcAI.Name, thisDialogue);
            otherNpcAI.DialogueHistory.Add(otherDialogue);
            otherNpc.GetComponent<DialogueComponent>().ShowDialogue(otherDialogue);
            yield return new WaitForSeconds(2);
        }

        // �Ի�����������ı�
        GetComponent<DialogueComponent>().HideDialogue();
        otherNpc.GetComponent<DialogueComponent>().HideDialogue();
        Debug.Log("�Ի�����");
        // �ָ�����ƶ�
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        otherNpc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        isProcessingState = false;
        isInDialogue = false;
        touchNpc = false;
    }

    //�Ի�
    public void Talk()
    {
        if (firstTalk == true)
        {
            GameObject dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
            GameObject dialogPrefab = Resources.Load<GameObject>("Dialog/Dialog");
            if (!dialog)
            {
                Instantiate(dialogPrefab);
            }
            // ��ʾ�״ζԻ���Ϣ
            dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
            dialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().message = new string[] { "���", "û����������", "ʲô��������δ��", "������������", "�ҽ�" + npcName, "��������" };
            dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            dialog.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
            currentFavor++;
        }
        else
        {
            // ������AI�ĶԻ�
            StartCoroutine(InitiateAIConversation());
        }
        npcState = 3;
    }

    // ʹ��AI��NPC�ĶԻ�
    public IEnumerator InitiateAIConversation()
    {
        string userMessage = userInputField.text; // ��ȡ������е�����

        GameObject dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
        GameObject dialogPrefab = Resources.Load<GameObject>("Dialog/Dialog");

        if (!dialog)
        {
            Instantiate(dialogPrefab);
        }

        string npcName = this.npcName;


        UnityWebRequest request = new UnityWebRequest("https://open.bigmodel.cn/api/paas/v4/chat/completions", "POST");
        var requestBody = new
        {
            model = "glm-4",
            messages = new[]
            {
                new { role = "user", content = $"{npcName}�������жԻ���" },
                new { role = "assistant", content = "�����ˡ�" },
                new { role = "user", content = userMessage },
                new { role = "assistant", content = previousMessage }
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

                // ���¶Ի���չʾAI��Ӧ
                dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
                string[] aiMessage = new string[] { aiResponse };
                dialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().message = aiMessage;
                dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

                previousMessage = aiResponse;

                taskManager.IncrementConversationCount();
                isWaitingForResponse = true;
            }
            else
            {
                Debug.LogError("��Ч��AI��Ӧ��ʽ");
            }
        }
        else
        {
            Debug.LogError("����ʧ�ܣ�" + request.error + request.downloadHandler.text);
        }

        // ���������е�����
        userInputField.text = "";
    }
    //ʹ��AI֮��ĶԻ�
    public IEnumerator InitiateAIConversation(string userMessage, NPC2 targetNpc)
    {
        GameObject dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
        GameObject dialogPrefab = Resources.Load<GameObject>("Dialog/Dialog");

        if (!dialog)
        {
            Instantiate(dialogPrefab);
        }

        string npcName = this.npcName;
        string targetNpcName = targetNpc.npcName;

        // ��������ĶԻ����ݣ�AI��Ҫ�������������ɻ�Ӧ
        UnityWebRequest request = new UnityWebRequest("https://open.bigmodel.cn/api/paas/v4/chat/completions", "POST");
        var requestBody = new
        {
            model = "glm-4",
            messages = new[]
            {
                new { role = "user", content = $"{npcName}��{targetNpcName}�����жԻ���" },
                new { role = "assistant", content = "�����ˡ�" },
                new { role = "user", content = userMessage }, // ����Ƿ���ĶԻ�����
                new { role = "assistant", content = previousMessage } // ����һ�ֶԻ����ݸ�AI��Ϊ������
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

                // ���¶Ի���չʾAI��Ӧ
                dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
                string[] aiMessage = new string[] { aiResponse };
                dialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().message = aiMessage;
                dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

                // ��AI�Ļ�Ӧ������Ϊ��һ�ֶԻ���������
                previousMessage = aiResponse;

                // ������NPC�Ļ�������
                taskManager.IncrementConversationCount();

                // ����NPCΪ�ȴ���Ӧ״̬��׼������һ�ֵĶԻ�
                isWaitingForResponse = true;
            }
            else
            {
                Debug.LogError("��Ч��AI��Ӧ��ʽ");
            }
        }
        else
        {
            Debug.LogError("����ʧ�ܣ�" + request.error + request.downloadHandler.text);
        }
    }



    // NPC�ظ���һNPC����Ϣ
    public void RespondToNpc(NPC2 targetNpc)
    {
        if (isWaitingForResponse)
        {
            // �ڵȴ�NPC��Ӧ
            string responseMessage = "�����˽�������������顣";

            // ������Ŀ��NPC�Ի�
            StartCoroutine(InitiateAIConversation(responseMessage, targetNpc));
            isWaitingForResponse = false; // ���õȴ�״̬
        }
    }

    //�øж�������ϵ
    public void FavorUpdateLevel()
    {
        //���øж���ֵ�䶯
        maxFavor = 10 + npcFavorLevel * (npcFavorLevel + 1) * 5;
        //����
        if (currentFavor >= maxFavor)
        {//�жϵ�ǰ�����Ƿ���������
            if (currentFavor > maxFavor)
            {//��ǰ������ڵ�ǰ�����ȥ�����
                if (npcFavorLevel < maxNpcFavorLevel)
                {
                    currentFavor = currentFavor - maxFavor;
                    npcFavorLevel++;
                }
                else
                {
                    currentFavor = maxFavor;
                }

            }
        }
        else
        {
            //�ж��Ƿ�ﵽ���ȼ�
            if (npcFavorLevel < maxNpcFavorLevel)
            {
                currentFavor = 0;
                npcFavorLevel++;
            }
            else
            {
                currentFavor = maxFavor;
            }
        }
    }
}


