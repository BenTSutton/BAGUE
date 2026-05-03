using Unity.VisualScripting;
using UnityEngine;

public class CannonViewScript : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    void OnEnable()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        crosshair.position = mousePos;

        if (GameManager.Instance.currentState == GameState.Aiming)
        {
            if (Input.GetMouseButtonDown(0)) // 0 is Left Click
            {
                FireCannon();
            }
        }
    }

    void FireCannon()
    {
        Debug.Log("Bang!");
        GameManager.Instance.ChangeState(GameState.Combat);
        this.gameObject.SetActive(false);
        Cursor.visible = true;
    }

    
}
