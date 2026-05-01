using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject shipUIObj;
    private bool invActive = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I) && 
        GameManager.Instance.currentState == GameState.Navigation)
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        invActive = !invActive;
        shipUIObj.SetActive(invActive);
    }
}
