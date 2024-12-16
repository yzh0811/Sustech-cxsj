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
            // ��ȡ���������ת�Ƕ�
            Vector3 eulerRotation = Camera.main.transform.rotation.eulerAngles;

            // ����һ���µ���ת������������� y ����ת�������޸������ x ����ת
            Quaternion targetRotation = Quaternion.Euler(eulerRotation.x, childs[j].rotation.eulerAngles.y, childs[j].rotation.eulerAngles.z);

            // ��ȡ Rigidbody ���
            Rigidbody rb = childs[j].GetComponent<Rigidbody>();
            if (rb != null)
            {
                // ʹ�� Rigidbody �� MoveRotation ����ת
                rb.MoveRotation(targetRotation);
            }
            else
            {
                // ���û�� Rigidbody����ֱ���޸� Transform �� rotation
                childs[j].rotation = targetRotation;
            }
        }
    }

}

