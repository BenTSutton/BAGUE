using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance != null)
        {
            gameObject.SetActive(GameManager.Instance.debug);
        }
        else
        {
            // Test scene, likely no GameManager
            gameObject.SetActive(true);
        }
    }
}