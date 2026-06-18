using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShieldStation : EnemyShipStation
{
    [SerializeField] private ShieldColourGradient shieldGradient;
    private GameObject shieldVisuals;
    private Image shieldImage;
    private float shieldAlpha = 0.3f;

    protected override void Awake()
    {
        // Finds the ship component on this object or any parent
        enemyShip = GetComponentInParent<EnemyShip>();

        if (enemyShip != null)
            {
                enemyShip.setShieldStationStatus(true);
                shieldVisuals = enemyShip.transform.Find("ShieldVisuals")?.gameObject;

                if (shieldVisuals != null)
                {
                    shieldImage = shieldVisuals.GetComponent<Image>();
                }
                else
                {
                    Debug.LogError($"EnemyShieldStation could not find 'ShieldVisuals' relative to {enemyShip.name}!");
                }
            }
    }

    private void Start()
    {
        if (enemyShip != null && shieldVisuals != null)
        {
            shieldImage.enabled = true;
            
            shieldImage.UpdateShieldColour(enemyShip.GetShieldHealth, enemyShip.GetShieldMaxHealth);
        }
    }

    private void ReactToShieldDamaged(float shieldHealth, float shieldMaxHealth)
    {
        shieldImage.UpdateShieldColour(shieldHealth, shieldMaxHealth);
    }

    // private void UpdateShieldColour(float shieldHealth, float shieldMaxHealth)
    // {
    //     if (shieldImage == null || shieldGradient == null) return;
        
    //     Color calculatedColor = shieldGradient.GetColor(shieldHealth, shieldMaxHealth);
    //     calculatedColor.a = shieldAlpha;
    //     shieldImage.color = calculatedColor;;
    // }


    private void OnEnable()
    {
        enemyShip.OnEnemyShieldBreak += DisableShield;
        EnemyShip.OnEnemyShieldDamaged += ReactToShieldDamaged;
        EnemyShip.OnEnemyShieldRepaired += EnableShield;
    }

    private void OnDisable()
    {
        enemyShip.OnEnemyShieldBreak -= DisableShield;
        EnemyShip.OnEnemyShieldDamaged -= ReactToShieldDamaged;
        EnemyShip.OnEnemyShieldRepaired -= EnableShield;     
    }

    private void HandleFixedStation() // Will likely need to be changed to an override at some point
    {
        EnableShield(enemyShip.GetShieldHealth, enemyShip.GetShieldMaxHealth);
        enemyShip.setShieldStationStatus(true);
    }

    public override void HandleBrokenStation()
    {
        DisableShield();
        enemyShip.setShieldStationStatus(false);
    }

    private void EnableShield(float shieldHealth, float shieldMaxHealth)
    {
        shieldImage.enabled = true;
        shieldImage.UpdateShieldColour(shieldHealth, shieldMaxHealth);
    }
    private void DisableShield()
    {
        shieldImage.enabled = false;
    }
}
