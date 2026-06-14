using UnityEngine;

[CreateAssetMenu(fileName = "NewShieldPalette", menuName = "UI-Elements/Shield Palette")]
public class ShieldColourGradient : ScriptableObject
{
    public Gradient colorGradient;

    public Color GetColor(float current, float max)
    {
        if (max <= 0) return Color.blue;

        float percentage = Mathf.Clamp01(current / max);

        
        float evaluatedPosition = current switch
        {
            // If current HP is exactly 1, set colour to red (or bottom of gradient)
            1f => 0.0f,

            // Evaluate what range the gradient in and sets evaluatedPosition to a specific part of the gradient
            _ when percentage <= 0.33f => 0.0f, // Critical / Low (Red)
            _ when percentage <= 0.80f => 0.34f, // Warning / Medium (Yellow)
            _ => 1.0f                          // Healthy (Blue)
        };

        return colorGradient.Evaluate(evaluatedPosition);
    }
}