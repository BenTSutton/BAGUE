using UnityEngine;

public class Twinkle : MonoBehaviour
{

    private float speed;
    public float twinkleSpeedMin = 1f;
    public float twinkleSpeedMax = 3f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    private float offset;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        offset = Random.Range(0f, 100f);
        speed = Random.Range(twinkleSpeedMin, twinkleSpeedMax);
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed + offset) + 1f) / 2f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}
