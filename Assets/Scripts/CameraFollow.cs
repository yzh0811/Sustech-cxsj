using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // 玩家 Transform
    public float smoothSpeed = 0.125f; // 平滑跟随的速度
    public Vector3 offset; // 相机与玩家之间的偏移量
    public CinemachineVirtualCamera virtualCamera; // Cinemachine Virtual Camera 引用

    void Start()
    {
        // 如果没有手动设置玩家对象，可以在这里查找
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player object not found! Make sure the player has the 'Player' tag.");
            }
        }

        // 如果没有手动设置CinemachineVirtualCamera，则通过Find查找
        if (virtualCamera == null)
        {
            virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineVirtualCamera not found in the scene!");
            }
        }

        // 设置虚拟相机的Follow为玩家
        if (virtualCamera != null && player != null)
        {
            virtualCamera.Follow = player;
            virtualCamera.LookAt = player;
        }

        // 监听场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void LateUpdate()
    {
        if (player == null) return;  // 如果玩家对象为空，则跳过更新

        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 更新相机位置
        transform.position = smoothedPosition;
    }

    // 场景加载完成后，更新相机的Follow目标
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查 player 是否丢失
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player object lost after scene load!");
                return; // 如果找不到 player，退出
            }
        }

        // 确保设置虚拟相机的 Follow 和 LookAt
        if (virtualCamera != null && player != null)
        {
            virtualCamera.Follow = player;
            virtualCamera.LookAt = player;
            Debug.Log("Cinemachine Camera Follow updated!");
        }
    }

    // 确保取消场景加载事件的监听
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
