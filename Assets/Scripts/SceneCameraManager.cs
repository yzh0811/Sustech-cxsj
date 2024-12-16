using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCameraManager : MonoBehaviour
{
    public Camera mainCamera;         // 当前场景摄像机
    public Camera newSceneCamera;     // 新场景摄像机

    public string currentSceneName;   // 当前场景名称
    public string targetSceneName;    // 目标场景名称

    private GameObject[] currentSceneRootObjects;
    private GameObject[] newSceneRootObjects;

    private bool isViewingNewScene = false;

    private void Start()
    {
        // 获取当前场景的根对象
        Scene currentScene = SceneManager.GetActiveScene();
        currentSceneName = currentScene.name;
        currentSceneRootObjects = currentScene.GetRootGameObjects();

        // 异步加载目标场景
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 获取目标场景的根对象
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (targetScene.IsValid())
        {
            newSceneRootObjects = targetScene.GetRootGameObjects();

            // 默认隐藏目标场景的所有根对象
            foreach (GameObject rootObj in newSceneRootObjects)
            {
                rootObj.SetActive(false);
            }

            Debug.Log($"目标场景 {targetSceneName} 已加载并隐藏。");
        }
    }

    private void Update()
    {
        // 按键切换场景
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isViewingNewScene)
            {
                SetCurrentSceneActive();
            }
            else
            {
                SetNewSceneActive();
            }
        }
    }

    private void SetCurrentSceneActive()
    {
        // 启用当前场景摄像机，禁用新场景摄像机
        mainCamera.enabled = true;
        newSceneCamera.enabled = false;

        // 显示当前场景，隐藏新场景
        if (currentSceneRootObjects != null)
        {
            foreach (GameObject rootObj in currentSceneRootObjects)
            {
                rootObj.SetActive(true);
            }
        }
        if (newSceneRootObjects != null)
        {
            foreach (GameObject rootObj in newSceneRootObjects)
            {
                rootObj.SetActive(false);
            }
        }

        isViewingNewScene = false;
        Debug.Log("切换到当前场景视角。");
    }

    private void SetNewSceneActive()
    {
        // 启用新场景摄像机，禁用当前场景摄像机
        mainCamera.enabled = false;
        newSceneCamera.enabled = true;

        // 显示新场景，隐藏当前场景
        if (currentSceneRootObjects != null)
        {
            foreach (GameObject rootObj in currentSceneRootObjects)
            {
                rootObj.SetActive(false);
            }
        }
        if (newSceneRootObjects != null)
        {
            foreach (GameObject rootObj in newSceneRootObjects)
            {
                rootObj.SetActive(true);
            }
        }

        isViewingNewScene = true;
        Debug.Log("切换到新场景视角。");
    }
}
