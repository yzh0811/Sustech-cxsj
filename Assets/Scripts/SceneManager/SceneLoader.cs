using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    private GameObject playerPrefab;  // 不再使用 public 变量

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject); // 保持该对象在场景切换时不被销毁

        // 使用 Resources.Load 动态加载玩家预制体
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player"); // 假设玩家预制体位于 Resources/Prefabs 文件夹下
       
        if (playerPrefab == null)
        {
            Debug.LogError("未找到玩家预制体，请确保已将预制体放入 Resources 文件夹中并使用正确的路径！");
        }
        else
        {
            Debug.Log("已找到玩家预制体");
        }
    }

    // 切换场景函数
    public void TransitionToScene(string sceneName, Vector3 playerPosition, GameObject objToTeleport)
    {

        DontDestroyOnLoad(objToTeleport);
        // 检查目标场景是否已经加载
        if (IsSceneLoaded(sceneName))
        {
            Debug.Log($"场景 {sceneName} 已经加载，直接移动对象。");

            // 直接将对象移动到目标位置
            Scene targetScene = SceneManager.GetSceneByName(sceneName);
            Scene currentScene = SceneManager.GetActiveScene();  // 当前活动场景
            SceneManager.MoveGameObjectToScene(objToTeleport, targetScene);
            SceneManager.SetActiveScene(targetScene);
            // 隐藏当前场景中的所有根对象
            foreach (GameObject rootObj in currentScene.GetRootGameObjects())
            {
                rootObj.SetActive(false);
                Debug.Log($"隐藏当前场景对象：{rootObj.name}");
            }

            // 激活目标场景的所有根对象
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                rootObj.SetActive(true);
                Debug.Log($"重现目标场景对象：{rootObj.name}");
            }
        }
        else
        {
            // 启动协程加载场景并完成传送
            StartCoroutine(TransitionCoroutine(sceneName, playerPosition, objToTeleport));
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        // 遍历当前已加载的场景，检查是否已加载
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }
   
    // 切换场景协程
    public IEnumerator TransitionCoroutine(string newSceneName, Vector3 targetPosition, GameObject objToTeleport)
    {
        Scene currentScene = SceneManager.GetActiveScene();  // 当前活动场景
        Scene targetScene = SceneManager.GetSceneByName(newSceneName);  // 目标场景

        // 如果目标场景还没有加载，则加载目标场景
        if (!targetScene.isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            targetScene = SceneManager.GetSceneByName(newSceneName);  // 确保目标场景加载完成
        }

        // 判断传送的对象类型（Player或NPC）
        if (objToTeleport.CompareTag("Player"))
        {
            // 玩家传送逻辑
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // 如果找不到玩家对象，则实例化一个新的玩家对象
                if (playerPrefab != null)
                {
                    SceneManager.MoveGameObjectToScene(player, targetScene);
                    player.transform.position = targetPosition;
                    Debug.Log("玩家对象已实例化到位置：" + targetPosition);
                }
                else
                {
                    Debug.LogError("玩家预制体加载失败！");
                }
            }
            else
            {
                // 如果玩家已经存在，直接移动玩家对象到目标位置
                player.transform.position = targetPosition;
                Debug.Log("玩家对象已移动到新位置：" + targetPosition);
            }
        }
        else
        {
            Debug.LogError("无法识别传送对象的标签：" + objToTeleport.tag);
        }
        SceneManager.SetActiveScene(targetScene);
        // 隐藏当前场景中的所有根对象
        foreach (GameObject rootObj in currentScene.GetRootGameObjects())
        {
            rootObj.SetActive(false);
            Debug.Log($"隐藏当前场景对象：{rootObj.name}");
        }

        // 激活目标场景的所有根对象
        foreach (GameObject rootObj in targetScene.GetRootGameObjects())
        {
            rootObj.SetActive(true);
            Debug.Log($"重现目标场景对象：{rootObj.name}");
        }
    }

    // 更新 NPC 状态的方法
    private void UpdateNPCState(GameObject npc)
    {
        string npcID = npc.name; // 假设 NPC 的名称是唯一标识
        Vector3 position = npc.transform.position;

        NPCmanager.Instance.SetNPCPosition(npcID, position);
        Debug.Log($"NPC 状态已更新：{npcID}, 位置：{position}");
    }

    public void SendNPCToNewScene(string newSceneName, Vector3 targetPosition, GameObject npc)
    {
        // 确保 NPC 不被销毁
        DontDestroyOnLoad(npc);

        // 更新 NPC 的状态
        UpdateNPCState(npc);

        // 异步加载目标场景
        StartCoroutine(SendNPCCoroutine(newSceneName, targetPosition, npc));
    }
    private IEnumerator SendNPCCoroutine(string newSceneName, Vector3 targetPosition, GameObject npc)
    {
        // 异步加载目标场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 获取目标场景
        Scene targetScene = SceneManager.GetSceneByName(newSceneName);
        if (targetScene.IsValid())
        {
            // 将 NPC 移动到目标场景
            SceneManager.MoveGameObjectToScene(npc, targetScene);
            npc.transform.position = targetPosition;
            Debug.Log($"NPC 已传送到新场景：{newSceneName}, 位置：{targetPosition}");

            // 隐藏目标场景的所有根对象
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                rootObj.SetActive(false);
                Debug.Log($"隐藏目标场景对象：{rootObj.name}");
            }
        }
    }



}
