using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    //获取到Cinemachine
    private CinemachineVirtualCamera cine;
    //切换follow时间
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
    {//拿到cinemachine
        cine = GameObject.FindGameObjectWithTag("cineMachine").GetComponent<CinemachineVirtualCamera>();
        //拿到变更镜头的物体
        GameObject changeCameraTest = GameObject.Find("changeCameraTest").gameObject;
        cine.Follow = changeCameraTest.GetComponent<Transform>();
        //失活角色脚本
        PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        playerMovement.enabled = false;
    }

    //恢复原状
    public void ChangeBack()
    {
        cine = GameObject.FindGameObjectWithTag("cineMachine").GetComponent<CinemachineVirtualCamera>();
        //拿到主角
        GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
        cine.Follow = player.transform;
        //经剧情播放结束，激活脚本
        if (totalTime >=7) {
            PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            playerMovement.enabled = true;
        }
    }
}
