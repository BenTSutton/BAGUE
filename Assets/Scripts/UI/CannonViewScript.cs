using Unity.VisualScripting;
using UnityEngine;

public class CannonViewScript : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    void OnEnable()
    {
        Cursor.visible = false;
        ButtonUI.ShotsFired += FireCannon;
    }

    private void OnDisable()
    {
        ButtonUI.ShotsFired -= FireCannon;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        crosshair.position = mousePos;
    }

    public void FireCannon()
    {
        Debug.Log("Bang!");
        GameManager.Instance.ChangeState(GameState.Combat);
        this.gameObject.SetActive(false);
        Cursor.visible = true;
    }
}
