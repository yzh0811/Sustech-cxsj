using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    private GameObject playerPrefab;  // ����ʹ�� public ����

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

        DontDestroyOnLoad(gameObject); // ���ָö����ڳ����л�ʱ��������

        // ʹ�� Resources.Load ��̬�������Ԥ����
        playerPrefab = Resources.Load<GameObject>("Prefabs/Player"); // �������Ԥ����λ�� Resources/Prefabs �ļ�����
       
        if (playerPrefab == null)
        {
            Debug.LogError("δ�ҵ����Ԥ���壬��ȷ���ѽ�Ԥ������� Resources �ļ����в�ʹ����ȷ��·����");
        }
        else
        {
            Debug.Log("���ҵ����Ԥ����");
        }
    }

    // �л���������
    public void TransitionToScene(string sceneName, Vector3 playerPosition, GameObject objToTeleport)
    {

        DontDestroyOnLoad(objToTeleport);
        // ���Ŀ�곡���Ƿ��Ѿ�����
        if (IsSceneLoaded(sceneName))
        {
            Debug.Log($"���� {sceneName} �Ѿ����أ�ֱ���ƶ�����");

            // ֱ�ӽ������ƶ���Ŀ��λ��
            Scene targetScene = SceneManager.GetSceneByName(sceneName);
            Scene currentScene = SceneManager.GetActiveScene();  // ��ǰ�����
            SceneManager.MoveGameObjectToScene(objToTeleport, targetScene);
            SceneManager.SetActiveScene(targetScene);
            // ���ص�ǰ�����е����и�����
            foreach (GameObject rootObj in currentScene.GetRootGameObjects())
            {
                rootObj.SetActive(false);
                Debug.Log($"���ص�ǰ��������{rootObj.name}");
            }

            // ����Ŀ�곡�������и�����
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                rootObj.SetActive(true);
                Debug.Log($"����Ŀ�곡������{rootObj.name}");
            }
        }
        else
        {
            // ����Э�̼��س�������ɴ���
            StartCoroutine(TransitionCoroutine(sceneName, playerPosition, objToTeleport));
        }
    }

    private bool IsSceneLoaded(string sceneName)
    {
        // ������ǰ�Ѽ��صĳ���������Ƿ��Ѽ���
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
   
    // �л�����Э��
    public IEnumerator TransitionCoroutine(string newSceneName, Vector3 targetPosition, GameObject objToTeleport)
    {
        Scene currentScene = SceneManager.GetActiveScene();  // ��ǰ�����
        Scene targetScene = SceneManager.GetSceneByName(newSceneName);  // Ŀ�곡��

        // ���Ŀ�곡����û�м��أ������Ŀ�곡��
        if (!targetScene.isLoaded)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            targetScene = SceneManager.GetSceneByName(newSceneName);  // ȷ��Ŀ�곡���������
        }

        // �жϴ��͵Ķ������ͣ�Player��NPC��
        if (objToTeleport.CompareTag("Player"))
        {
            // ��Ҵ����߼�
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // ����Ҳ�����Ҷ�����ʵ����һ���µ���Ҷ���
                if (playerPrefab != null)
                {
                    SceneManager.MoveGameObjectToScene(player, targetScene);
                    player.transform.position = targetPosition;
                    Debug.Log("��Ҷ�����ʵ������λ�ã�" + targetPosition);
                }
                else
                {
                    Debug.LogError("���Ԥ�������ʧ�ܣ�");
                }
            }
            else
            {
                // �������Ѿ����ڣ�ֱ���ƶ���Ҷ���Ŀ��λ��
                player.transform.position = targetPosition;
                Debug.Log("��Ҷ������ƶ�����λ�ã�" + targetPosition);
            }
        }
        else
        {
            Debug.LogError("�޷�ʶ���Ͷ���ı�ǩ��" + objToTeleport.tag);
        }
        SceneManager.SetActiveScene(targetScene);
        // ���ص�ǰ�����е����и�����
        foreach (GameObject rootObj in currentScene.GetRootGameObjects())
        {
            rootObj.SetActive(false);
            Debug.Log($"���ص�ǰ��������{rootObj.name}");
        }

        // ����Ŀ�곡�������и�����
        foreach (GameObject rootObj in targetScene.GetRootGameObjects())
        {
            rootObj.SetActive(true);
            Debug.Log($"����Ŀ�곡������{rootObj.name}");
        }
    }

    // ���� NPC ״̬�ķ���
    private void UpdateNPCState(GameObject npc)
    {
        string npcID = npc.name; // ���� NPC ��������Ψһ��ʶ
        Vector3 position = npc.transform.position;

        NPCmanager.Instance.SetNPCPosition(npcID, position);
        Debug.Log($"NPC ״̬�Ѹ��£�{npcID}, λ�ã�{position}");
    }

    public void SendNPCToNewScene(string newSceneName, Vector3 targetPosition, GameObject npc)
    {
        // ȷ�� NPC ��������
        DontDestroyOnLoad(npc);

        // ���� NPC ��״̬
        UpdateNPCState(npc);

        // �첽����Ŀ�곡��
        StartCoroutine(SendNPCCoroutine(newSceneName, targetPosition, npc));
    }
    private IEnumerator SendNPCCoroutine(string newSceneName, Vector3 targetPosition, GameObject npc)
    {
        // �첽����Ŀ�곡��
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);

        // �ȴ������������
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // ��ȡĿ�곡��
        Scene targetScene = SceneManager.GetSceneByName(newSceneName);
        if (targetScene.IsValid())
        {
            // �� NPC �ƶ���Ŀ�곡��
            SceneManager.MoveGameObjectToScene(npc, targetScene);
            npc.transform.position = targetPosition;
            Debug.Log($"NPC �Ѵ��͵��³�����{newSceneName}, λ�ã�{targetPosition}");

            // ����Ŀ�곡�������и�����
            foreach (GameObject rootObj in targetScene.GetRootGameObjects())
            {
                rootObj.SetActive(false);
                Debug.Log($"����Ŀ�곡������{rootObj.name}");
            }
        }
    }



}
