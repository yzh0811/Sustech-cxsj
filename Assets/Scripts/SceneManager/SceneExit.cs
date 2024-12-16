using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理

public class SceneExit : MonoBehaviour
{
    public string newSceneName;         // 目标场景名
    public Vector3 targetPosition;      // 目标位置

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 获取当前场景名称
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 如果目标场景和当前场景相同，则不执行传送逻辑
        if (newSceneName == currentSceneName)
        {
            Debug.Log("目标场景与当前场景相同，传送被忽略。");
            return;
        }
        
        // 如果是玩家进入传送门，切换场景
        if (other.CompareTag("Player"))
        {
            // 玩家进入传送门，切换场景
            SceneLoader.Instance.TransitionToScene(newSceneName, targetPosition, other.gameObject);
        }
        // 如果是 NPC 进入传送门，不切换场景，只将 NPC 移动到新场景
        else if (other.CompareTag("NPC"))
        {
            // NPC 进入传送门，不切换场景，只将 NPC 移动到新场景
            SceneLoader.Instance.SendNPCToNewScene(newSceneName, targetPosition, other.gameObject);
        }
    }
}
