using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShieldStation : EnemyShipStation
{
    [SerializeField] private GameObject shieldVisuals;
    [SerializeField] private ShieldColourGradient shieldGradient;
    private Image shieldImage;
    private float shieldAlpha = 0.3f;

    protected override void Awake()
    {
        // Finds the ship component on this object or any parent
        enemyShip = GetComponentInParent<EnemyShip>();
        if (shieldVisuals != null)
        {
            shieldImage = shieldVisuals.GetComponent<Image>();
        }

        if (enemyShip != null)
        {
            enemyShip.setShieldStationStatus(true);
        }
    }

    private void Start()
    {
        if (enemyShip != null && shieldVisuals != null)
        {
            shieldImage.enabled = true;
            
            UpdateShieldColour(enemyShip.GetShieldHealth, enemyShip.GetShieldMaxHealth);
        }
    }

    private void UpdateShieldColour(float shieldHealth, float shieldMaxHealth)
    {
        if (shieldImage == null || shieldGradient == null) return;
        
        Color calculatedColor = shieldGradient.GetColor(shieldHealth, shieldMaxHealth);
        calculatedColor.a = shieldAlpha;
        shieldImage.color = calculatedColor;;
    }

    private void OnEnable()
    {
        enemyShip.OnEnemyShieldBreak += DisableShield;
        EnemyShip.OnEnemyShieldDamaged += UpdateShieldColour;
        EnemyShip.OnEnemyShieldRepaired += EnableShield;
    }

    private void OnDisable()
    {
        enemyShip.OnEnemyShieldBreak -= DisableShield;
        EnemyShip.OnEnemyShieldDamaged -= UpdateShieldColour;
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
        UpdateShieldColour(shieldHealth, shieldMaxHealth);
    }
    private void DisableShield()
    {
        shieldImage.enabled = false;
    }



}
