using UnityEngine;
using UnityEngine.UI;

public class StationStatusMirror : MonoBehaviour
{
    private Image mirrorImage;
    private EnemyShipStationUI targetStationUI;

    public void InitializeMirror(EnemyShipStationUI targetUI)
    {
        mirrorImage = GetComponent<Image>();
        targetStationUI = targetUI;

        if (targetStationUI != null && mirrorImage != null)
        {
            // Give this global display icon the exact same graphic profile the station uses
            mirrorImage.sprite = targetStationUI.GetStationSprite;
        }
    }

    private void Update()
    {
        // Simple frame sync: copy the exact color (White/Red) from the station UI
        if (targetStationUI != null && mirrorImage != null)
        {
            // If you change the code above to match your existing logic, 
            // you can expose a way to get the image color, or check the station status directly:
            mirrorImage.color = targetStationUI.GetComponentInChildren<Image>().color;
        }
    }
}
