 using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class DiologTest : MonoBehaviour
{
    //获取到文本框control
    private Transform DiologControll;
    //文本
    private string[] dialog = {"总算到了","我记得会有人接我","......","这里有路标","" };
    //多长时间变换一次文本
    private float dialogChangeTime = 0;
    //下标
    private int index = -1;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        dialogChangeTime += Time.deltaTime;
        if (dialogChangeTime >=1)
        {
            ChangeText();
            dialogChangeTime = 0;
        }
    }

    //变换文本
    public void ChangeText()
    {
        //赋值
        DiologControll = GetComponent<Transform>();
        //取激活文本框
        if (index == -1)
        {
            DiologControll.GetChild(0).gameObject.SetActive(true);
            //index为文本最后一个
        }else if (index == dialog.Length)
        {
            DiologControll.GetChild(0).gameObject.SetActive(false);
            DiologControll.GetComponent<DiologTest>().enabled = false;
        }
        else
        {
            DiologControll.GetChild(0).GetChild(0).GetComponent<Text>().text = dialog[index];
        }
        index++;
    }
}
