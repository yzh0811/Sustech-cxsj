using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    //��ȡ��Cinemachine
    private CinemachineVirtualCamera cine;
    //�л�followʱ��
    private float totalTime;
    void Start()
    {
        ChangeFollow();
    }

    // Update is called once per frame
    void Update()
    {
        //
        totalTime += Time.deltaTime;
        if (totalTime>=8)
        {
            ChangeBack();
        }
    }

    //
    public void ChangeFollow()
    {//�õ�cinemachine
        cine = GameObject.FindGameObjectWithTag("cineMachine").GetComponent<CinemachineVirtualCamera>();
        //�õ������ͷ������
        GameObject changeCameraTest = GameObject.Find("changeCameraTest").gameObject;
        cine.Follow = changeCameraTest.GetComponent<Transform>();
        //ʧ���ɫ�ű�
        PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerMovement.enabled = false;
    }

    //�ָ�ԭ״
    public void ChangeBack()
    {
        cine = GameObject.FindGameObjectWithTag("cineMachine").GetComponent<CinemachineVirtualCamera>();
        //�õ�����
        GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
        cine.Follow = player.transform;
        //�����鲥�Ž���������ű�
        if (totalTime >=7) {
            PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            playerMovement.enabled = true;
        }
    }
}
