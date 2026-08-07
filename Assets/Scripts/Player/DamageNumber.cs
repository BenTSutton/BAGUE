using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float lifetime = 0.65f;
    [SerializeField] private float riseSpeed = 20f;

    private float elapsed;

    public void Initialize(int damage, bool killed)
    {
        damageText.text = damage.ToString();
        damageText.color = killed
            ? new Color(1f, 0.75f, 0.15f)
            : Color.white;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        float progress = Mathf.Clamp01(elapsed / lifetime);
        damageText.alpha = 1f - progress;

        if (elapsed >= lifetime)
            Destroy(gameObject);
    }
}