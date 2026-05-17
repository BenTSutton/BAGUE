using Unity.VisualScripting;
using UnityEngine;

public class CannonViewScript : MonoBehaviour
{
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Canvas thisCanvas;
    void OnEnable()
    {
        //Cursor.visible = false;
        StationHitByCannon.ShotsFired += FireCannon;
    }

    private void OnDisable()
    {
        StationHitByCannon.ShotsFired -= FireCannon;
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
        thisCanvas.enabled = false; 
        Cursor.visible = true;
    }
}
