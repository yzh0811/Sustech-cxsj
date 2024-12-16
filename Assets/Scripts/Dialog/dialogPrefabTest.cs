using System.Collections;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
public class dialogPrefabTest : MonoBehaviour
{
    //�����ȡ���ı��±�
    public int index = 0;
    //�����ȡ�ı�
    public string[] message = null;
    //public string[] message = new string {"11","222","333"}
    //���ƹر��ı���
    public bool closeDialog = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CloseDialog();
        DialogControl();
    }


    //�ر��ı���
    private void CloseDialog()
    {
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
        if(Input.GetKeyDown(KeyCode.E) && InitDialog.transform.GetChild(0).GetChild(0).gameObject.activeSelf == true)
        {
            InitDialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        }
    }
    public void DialogControl()
    {
        if(message.Length == 0)
        {
            return;
        }
        GameObject InitDialog = GameObject.FindGameObjectWithTag("DialogPrefab");
       
        
        if(index < message.Length)
        {
            if(Input.GetKeyDown(KeyCode.E) && closeDialog == false)
            {
                InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = message[index];
                index++;
                if(index == message.Length)
                {
                    closeDialog = true;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E) && closeDialog == true)
            {
                InitDialog.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
                InitDialog.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = "";
                index = 0;
                message = new string[0];
                closeDialog = false;
            }
        }


    }
}
