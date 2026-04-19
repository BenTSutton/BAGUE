using UnityEngine;

public class MapShip : MonoBehaviour
{
    public static MapShip Instance;

    void Awake()
    {
        Instance = this;
    }

}
