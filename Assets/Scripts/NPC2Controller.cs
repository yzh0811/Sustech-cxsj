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
    private bool isInDialogue = false; // 添加标志位
    private bool isProcessingState = false;
    //ai部分
    private string apiKey = "deb0870790e59c7f8d4593e5459b5b59.dn2EGqYLtrmEmhkt";
    //对话框部分
    public TMP_InputField userInputField;  // 需要在Inspector中绑定
    //对话框显示
    public bool isInputFieldActive = false;
    //task
    public TaskManager taskManager;
    //速度
    public float speed = 120;
    //获取刚体
    public Rigidbody2D rigidbody;
    //动画器
    public Animator animator;
    //向量
    public Vector2 vector2;
    //状态 0 正常行走 1 发生碰撞 2 碰撞到主角 3 对话 4 送礼 5 组队 6 npc碰撞
    public int npcState = 0;
    //多长时间变换npc的移动方向
    public float changeDirectionTime = 0;
    //随机时间
    public float randomTime = 1;
    //方向 x -1 左 0 不动 1 右 Input.GetAxisRaw("Horizontal");
    public float[] xDirection = { -1, 0, 1 };
    //y -1 向下 0 不动 1 往上 Input.GetAxisRaw("Veritical")；
    public float[] yDirection = { -1, 0, 1 };
    //随机x和y
    public int xIndex = 0;
    public int yIndex = 0;
    //最终的坐标
    public float x = 0;
    public float y = 0;
    //前一个位移的坐标
    public float beforeX = 0;
    public float beforeY = 0;

    //-------碰撞
    //持续碰撞时间
    public float stayingTime = 0;
    //正常碰撞变更方向执行时间
    public float touchNormalTime = 0;
    //保持碰撞变更方向执行时间
    public float touchStayTime = 0;
    //发生持续碰撞随机方向
    public int changDirectionIndex = 0;
    //-------碰撞

    //-------碰撞玩家
    //碰撞玩家flag
    public bool touchPlayer = false;
    //碰撞玩家的时间
    public float touchPlayerTime = 0;
    //-------碰撞玩家

    //-------碰撞npc
    //碰撞npc的flag
    public bool touchNpc = false;
    //碰撞npc的时间
    public float touchNpcTime = 0;
    //-------碰撞npc


    //-------对话 
    public bool firstTalk = true;
    public string npcName;
    public string dynasty;
    //当前好感度
    public int npcFavorLevel = 0;
    //最大好感度
    public int maxNpcFavorLevel = 5;
    //当前好感度
    public int currentFavor = 0;
    //升级好感度需要的数值
    public int maxFavor = 0;
    //判断npc是否在等待对话回应
    public bool isWaitingForResponse = false;
    //用来保存上一轮对话的内容
    public string previousMessage = "";
    //-------对话
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
            //变更动画
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
        //好感度升级体系
        FavorUpdateLevel();
        if (Input.GetKeyDown(KeyCode.Return))  // 按下回车键
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
        //控制器
        NpcController();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCollisionHandled) return; // 如果碰撞已经处理过，直接返回

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

        isCollisionHandled = true; // 标记碰撞已处理
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
        //碰撞玩家和npc
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        touchPlayerTime = 0;
        touchNpcTime = 0;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        touchPlayer = false;
    }
    //npc控制器
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

    //正常行走的时候，会需要执行什么
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
            //我们的xy同时移动，重新随机
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
    //变更动画
    public void ChangeAnim()
    {
        //左走
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
        }//向右
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
        }//向上
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
        }//向下
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
        {//停止
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
            }//面向左
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
            }//向右
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
            }//面向上
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
            }//面向下
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

    //碰撞
    public void TouchMove()
    {
        stayingTime += Time.fixedDeltaTime;
        touchNormalTime += Time.fixedDeltaTime;
        touchStayTime += Time.fixedDeltaTime;
        //反方向
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
                        //编写随机数
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
            //随机方向
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

    //触碰玩家
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
            //期间有没有按下E键
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
        if (isInDialogue) return; // 防止重复触发对话
        isInDialogue = true;
        // 停止两者的随机移动
        this.rigidbody.bodyType = RigidbodyType2D.Static;
        otherNpc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Debug.Log("npc相互触碰");
        // 开始对话协程
        StartCoroutine(StartDialogueWithAI(otherNpc));
    }
   
    private IEnumerator StartDialogueWithAI(GameObject otherNpc)
    {
        // 获取两位 NPC 的 AI 数据
        NPC thisNpcAI = GetComponent<NpcComponent>().npcData;
        NPC otherNpcAI = otherNpc.GetComponent<NpcComponent>().npcData;

        int dialogueRounds = Random.Range(2, 5); // 对话轮数
        for (int i = 0; i < dialogueRounds; i++)
        {
            // 当前 NPC 说话
            string thisDialogue = thisNpcAI.GenerateDialogue(otherNpcAI.Name);
            if(thisDialogue != null)
            {
                Debug.Log("对话内容不为空");
            }
            thisNpcAI.DialogueHistory.Add(thisDialogue);
            GetComponent<DialogueComponent>().ShowDialogue(thisDialogue);
            yield return new WaitForSeconds(2);

            // 另一个 NPC 回应
            string otherDialogue = otherNpcAI.GenerateDialogue(thisNpcAI.Name, thisDialogue);
            otherNpcAI.DialogueHistory.Add(otherDialogue);
            otherNpc.GetComponent<DialogueComponent>().ShowDialogue(otherDialogue);
            yield return new WaitForSeconds(2);
        }

        // 对话结束，清除文本
        GetComponent<DialogueComponent>().HideDialogue();
        otherNpc.GetComponent<DialogueComponent>().HideDialogue();
        Debug.Log("对话结束");
        // 恢复随机移动
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        otherNpc.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        isProcessingState = false;
        isInDialogue = false;
        touchNpc = false;
    }

    //对话
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
            // 显示首次对话信息
            dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
            dialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().message = new string[] { "你好", "没见过的人呢", "什么，你来自未来", "看来就是你了", "我叫" + npcName, "跟我来吧" };
            dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
            dialog.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(true);
            currentFavor++;
        }
        else
        {
            // 启动与AI的对话
            StartCoroutine(InitiateAIConversation());
        }
        npcState = 3;
    }

    // 使用AI与NPC的对话
    public IEnumerator InitiateAIConversation()
    {
        string userMessage = userInputField.text; // 获取输入框中的内容

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
                new { role = "user", content = $"{npcName}与正进行对话。" },
                new { role = "assistant", content = "明白了。" },
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

                // 更新对话框并展示AI回应
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
                Debug.LogError("无效的AI回应格式");
            }
        }
        else
        {
            Debug.LogError("请求失败：" + request.error + request.downloadHandler.text);
        }

        // 清空输入框中的内容
        userInputField.text = "";
    }
    //使用AI之间的对话
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

        // 构造请求的对话内容，AI需要根据上下文生成回应
        UnityWebRequest request = new UnityWebRequest("https://open.bigmodel.cn/api/paas/v4/chat/completions", "POST");
        var requestBody = new
        {
            model = "glm-4",
            messages = new[]
            {
                new { role = "user", content = $"{npcName}与{targetNpcName}正进行对话。" },
                new { role = "assistant", content = "明白了。" },
                new { role = "user", content = userMessage }, // 这就是发起的对话内容
                new { role = "assistant", content = previousMessage } // 将上一轮对话传递给AI作为上下文
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

                // 更新对话框并展示AI回应
                dialog = GameObject.FindGameObjectWithTag("DialogPrefab").gameObject;
                string[] aiMessage = new string[] { aiResponse };
                dialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().message = aiMessage;
                dialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);

                // 将AI的回应保存作为下一轮对话的上下文
                previousMessage = aiResponse;

                // 增加与NPC的互动次数
                taskManager.IncrementConversationCount();

                // 设置NPC为等待回应状态，准备好下一轮的对话
                isWaitingForResponse = true;
            }
            else
            {
                Debug.LogError("无效的AI回应格式");
            }
        }
        else
        {
            Debug.LogError("请求失败：" + request.error + request.downloadHandler.text);
        }
    }



    // NPC回复另一NPC的消息
    public void RespondToNpc(NPC2 targetNpc)
    {
        if (isWaitingForResponse)
        {
            // 在等待NPC回应
            string responseMessage = "我想了解更多关于你的事情。";

            // 继续与目标NPC对话
            StartCoroutine(InitiateAIConversation(responseMessage, targetNpc));
            isWaitingForResponse = false; // 重置等待状态
        }
    }

    //好感度升级体系
    public void FavorUpdateLevel()
    {
        //最大好感度数值变动
        maxFavor = 10 + npcFavorLevel * (npcFavorLevel + 1) * 5;
        //升级
        if (currentFavor >= maxFavor)
        {//判断当前经验是否大于最大经验
            if (currentFavor > maxFavor)
            {//当前经验等于当前经验减去最大经验
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
            //判断是否达到最大等级
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


