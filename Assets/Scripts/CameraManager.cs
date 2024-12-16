using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;     // 主摄像机
    public Camera npcCamera;      // 副摄像机（NPC 视角摄像机）

    private bool isNpcViewActive = false;

    private void Start()
    {
        // 确保主摄像机激活，副摄像机禁用
        SetMainCameraActive();
    }

    private void Update()
    {
        // 按键切换摄像机视角（例如 "C" 键）
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

        Debug.Log("切换到主摄像机（玩家视角）。");
    }

    private void SetNPCCameraActive()
    {
        mainCamera.enabled = false;
        npcCamera.enabled = true;
        isNpcViewActive = true;

        Debug.Log("切换到副摄像机（NPC 视角）。");
    }
}
