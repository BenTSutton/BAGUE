using System;
using UnityEngine;

public class EnemyLaunchBayStation : EnemyShipStation
{
    public static event Action<EnemyLaunchBayStation> LaunchBayReady;
    public static event Action FutureBoardingWavesStopped;

    public bool CanLaunchWaves => !stationIsBroken;

    public override string BrokenMessage =>
        "LAUNCH BAY DESTROYED — NO FURTHER BOARDING WAVES";

    private void Start()
    {
        if (CanLaunchWaves)
        {
            LaunchBayReady?.Invoke(this);
        }
    }

    public override void HandleBrokenStation()
    {
        Debug.Log("[EnemyLaunchBayStation] Future boarding waves stopped.", this);

        FutureBoardingWavesStopped?.Invoke();
    }
}