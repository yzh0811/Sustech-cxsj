 using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class DiologTest : MonoBehaviour
{
    //��ȡ���ı���control
    private Transform DiologControll;
    //�ı�
    private string[] dialog = {"���㵽��","�Ҽǵû����˽���","......","������·��","" };
    //�೤ʱ��任һ���ı�
    private float dialogChangeTime = 0;
    //�±�
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

    //�任�ı�
    public void ChangeText()
    {
        //��ֵ
        DiologControll = GetComponent<Transform>();
        //ȡ�����ı���
        if (index == -1)
        {
            DiologControll.GetChild(0).gameObject.SetActive(true);
            //indexΪ�ı����һ��
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
