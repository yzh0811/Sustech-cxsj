using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class fishingDialogTest : MonoBehaviour
{
    private bool showDialog = false;
    //判断我们是否有触碰到路标
    private bool EnterRoadMark = false;
    // 钓鱼湖路标文本框显示和隐藏
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EnterRoadMark == true)
        {
            ControlDialog();
        }
        SetDialogActive();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            EnterRoadMark = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        EnterRoadMark = false; 
    }

    //控制我们的文本框显示和隐藏
    public void ControlDialog()
    {
        //获取预制体
        GameObject DialogTest = Resources.Load<GameObject>("Dialog/Dialog");
        //获取实例化物体
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        //
        if (!InitDialog)
        {
            Instantiate(DialogTest);
        }
        //赋值
        InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        if (Input.GetKeyDown(KeyCode.E))
        {
            showDialog = !showDialog;
            InitDialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(showDialog);
            InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = "左上方前往钓鱼湖";
        }
    }

    //控制文本框本身脚本的激活和失效
    public void SetDialogActive()
    {
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        if (EnterRoadMark == true)
        {
            InitDialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().enabled = false;
        }
        else
        {
            InitDialog.transform.GetChild(0).GetComponent<dialogPrefabTest>().enabled = true;
        }
    }
}
