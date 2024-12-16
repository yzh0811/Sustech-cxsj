using UnityEngine;
using UnityEngine.SceneManagement; // ���볡������

public class SceneExit : MonoBehaviour
{
    public string newSceneName;         // Ŀ�곡����
    public Vector3 targetPosition;      // Ŀ��λ��

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ��ȡ��ǰ��������
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ���Ŀ�곡���͵�ǰ������ͬ����ִ�д����߼�
        if (newSceneName == currentSceneName)
        {
            Debug.Log("Ŀ�곡���뵱ǰ������ͬ�����ͱ����ԡ�");
            return;
        }
        
        // �������ҽ��봫���ţ��л�����
        if (other.CompareTag("Player"))
        {
            // ��ҽ��봫���ţ��л�����
            SceneLoader.Instance.TransitionToScene(newSceneName, targetPosition, other.gameObject);
        }
        // ����� NPC ���봫���ţ����л�������ֻ�� NPC �ƶ����³���
        else if (other.CompareTag("NPC"))
        {
            // NPC ���봫���ţ����л�������ֻ�� NPC �ƶ����³���
            SceneLoader.Instance.SendNPCToNewScene(newSceneName, targetPosition, other.gameObject);
        }
    }
}
