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
        Debug.Log("[CannonViewScript] Bang!");
        // This check is in place to prevent it switching back to combat after defeating a enemy ship. May need changed if we change victory logic
        if (RunManager.Instance.activeEnemyShip != null)
        {
            GameManager.Instance.ChangeState(GameState.Combat);
        }
        else
        {
            Debug.Log("[CannonViewScript] Skipping switch back to combat as the enemy ship was destroyed");
        }
        thisCanvas.enabled = false; 
        Cursor.visible = true;
    }
}
