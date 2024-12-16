using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;

public class fishingDialogTest : MonoBehaviour
{
    private bool showDialog = false;
    //�ж������Ƿ��д�����·��
    private bool EnterRoadMark = false;
    // �����·���ı�����ʾ������
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

    //�������ǵ��ı�����ʾ������
    public void ControlDialog()
    {
        //��ȡԤ����
        GameObject DialogTest = Resources.Load<GameObject>("Dialog/Dialog");
        //��ȡʵ��������
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        //
        if (!InitDialog)
        {
            Instantiate(DialogTest);
        }
        //��ֵ
        InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        if (Input.GetKeyDown(KeyCode.E))
        {
            showDialog = !showDialog;
            InitDialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(showDialog);
            InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = "���Ϸ�ǰ�������";
        }
    }

    //�����ı�����ű��ļ����ʧЧ
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
