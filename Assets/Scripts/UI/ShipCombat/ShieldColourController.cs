using UnityEngine;
using UnityEngine.UI;

public static class ShieldColourController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private static ShieldColourGradient _globalGradient;

    // Loads once automatically to grab the ShieldGradient
    static ShieldColourController()
    {
        // Loads the asset named "MasterShieldPalette" from the Resources folder
        _globalGradient = Resources.Load<ShieldColourGradient>("UI/Palettes/MasterShieldPalette");

        if (_globalGradient == null)
        {
            Debug.LogError("ShieldEffects: Could not find 'MasterShieldPalette' in the Resources folder!");
        }
    }

    public static void UpdateShieldColour(this Image shieldImage, float shieldHealth, float shieldMaxHealth, float shieldAlpha = 0.3f)
    {
        if (shieldImage == null || _globalGradient == null) return;
        
        Color calculatedColor = _globalGradient.GetColor(shieldHealth, shieldMaxHealth);
        calculatedColor.a = shieldAlpha;
        shieldImage.color = calculatedColor;
    }
}
