using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShipStationProfile", menuName = "EnemyShipSystem/Stations/StationProfile")]
public class EnemyShipStationProfile : ScriptableObject
{
    public string stationTypeName;
    public Sprite icon;
}