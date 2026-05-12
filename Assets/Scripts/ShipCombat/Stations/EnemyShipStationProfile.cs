using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShipStationProfile", menuName = "EnemyShipSystem/StationProfile")]
public class EnemyShipStationProfile : ScriptableObject
{
    public string stationTypeName;
    public Sprite icon;
}