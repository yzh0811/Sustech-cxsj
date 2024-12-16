using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCameraManager : MonoBehaviour
{
    public Camera mainCamera;         // ��ǰ���������
    public Camera newSceneCamera;     // �³��������

    public string currentSceneName;   // ��ǰ��������
    public string targetSceneName;    // Ŀ�곡������

    private GameObject[] currentSceneRootObjects;
    private GameObject[] newSceneRootObjects;

    private bool isViewingNewScene = false;

    private void Start()
    {
        // ��ȡ��ǰ�����ĸ�����
        Scene currentScene = SceneManager.GetActiveScene();
        currentSceneName = currentScene.name;
        currentSceneRootObjects = currentScene.GetRootGameObjects();

        // �첽����Ŀ�곡��
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);

        // �ȴ������������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // ��ȡĿ�곡���ĸ�����
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (targetScene.IsValid())
        {
            newSceneRootObjects = targetScene.GetRootGameObjects();

            // Ĭ������Ŀ�곡�������и�����
            foreach (GameObject rootObj in newSceneRootObjects)
            {
                rootObj.SetActive(false);
            }

            Debug.Log($"Ŀ�곡�� {targetSceneName} �Ѽ��ز����ء�");
        }
    }

    private void Update()
    {
        // �����л�����
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
        // ���õ�ǰ����������������³��������
        mainCamera.enabled = true;
        newSceneCamera.enabled = false;

        // ��ʾ��ǰ�����������³���
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
        Debug.Log("�л�����ǰ�����ӽǡ�");
    }

    private void SetNewSceneActive()
    {
        // �����³�������������õ�ǰ���������
        mainCamera.enabled = false;
        newSceneCamera.enabled = true;

        // ��ʾ�³��������ص�ǰ����
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
        Debug.Log("�л����³����ӽǡ�");
    }
}
