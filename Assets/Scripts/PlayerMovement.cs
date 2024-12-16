using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    public float moveSpeed = 5f; // 玩家移动速度

    private Rigidbody2D rb;
    private Animator animator; // 动画控制器
    private Vector2 movement;

    //需要获取预制体
    private GameObject UITestPrefab;
    //获取加载后的预制体
    private GameObject UITest;
    //控制ui显示
    private bool uiControl = false;
    void Start()
    {
        Instance = this;
        // 获取 Rigidbody2D 和 Animator 组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        //获取UI预制体
        UITestPrefab = Resources.Load<GameObject>("ui/UITest");
        
        Instantiate(UITestPrefab);
        UITest = GameObject.FindGameObjectWithTag("uiSystem");
        
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        // 获取 WASD 输入
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 更新动画参数
        UpdateAnimation();
        //ui
        UIcontrol();
    }

    void FixedUpdate()
    {
        // 移动玩家
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // 更新动画状态的方法
    void UpdateAnimation()
    {
        // 设置水平和垂直移动参数
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);

        // 根据玩家是否在移动设置 Speed 参数
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
    //控制ui系统的显示与关闭
    void UIcontrol()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiControl = !uiControl;
            UITest.transform.GetChild(0).gameObject.SetActive(uiControl);
        }
    }
}
