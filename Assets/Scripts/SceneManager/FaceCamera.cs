using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamrea : MonoBehaviour
{
    Transform[] childs = null;
    // Start is called before the first frame update
    void Start()
    {
        childs = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            childs[i] = transform.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int j = 0; j < childs.Length; j++)
        {
            // 获取摄像机的旋转角度
            Vector3 eulerRotation = Camera.main.transform.rotation.eulerAngles;

            // 创建一个新的旋转，保持摄像机的 y 轴旋转，并且修改物体的 x 轴旋转
            Quaternion targetRotation = Quaternion.Euler(eulerRotation.x, childs[j].rotation.eulerAngles.y, childs[j].rotation.eulerAngles.z);

            // 获取 Rigidbody 组件
            Rigidbody rb = childs[j].GetComponent<Rigidbody>();
            if (rb != null)
            {
                // 使用 Rigidbody 的 MoveRotation 来旋转
                rb.MoveRotation(targetRotation);
            }
            else
            {
                // 如果没有 Rigidbody，就直接修改 Transform 的 rotation
                childs[j].rotation = targetRotation;
            }
        }
    }

}

