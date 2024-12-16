using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;     // �������
    public Camera npcCamera;      // ���������NPC �ӽ��������

    private bool isNpcViewActive = false;

    private void Start()
    {
        // ȷ�����������������������
        SetMainCameraActive();
    }

    private void Update()
    {
        // �����л�������ӽǣ����� "C" ����
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isNpcViewActive)
            {
                SetMainCameraActive();
            }
            else
            {
                SetNPCCameraActive();
            }
        }
    }

    private void SetMainCameraActive()
    {
        mainCamera.enabled = true;
        npcCamera.enabled = false;
        isNpcViewActive = false;

        Debug.Log("�л����������������ӽǣ���");
    }

    private void SetNPCCameraActive()
    {
        mainCamera.enabled = false;
        npcCamera.enabled = true;
        isNpcViewActive = true;

        Debug.Log("�л������������NPC �ӽǣ���");
    }
}
