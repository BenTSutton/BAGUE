using System.Collections;
using UnityEngine;

public class ShootingStar : MonoBehaviour
{
    [SerializeField] private Vector2 direction = new(1f, -0.4f);
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private TrailRenderer trailRenderer;
    private Material trailMaterial;
    private Color originalTrailMaterialColor;
    private int trailColorProperty;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        trailRenderer = GetComponentInChildren<TrailRenderer>();

        trailMaterial = trailRenderer.material;

        trailColorProperty = trailMaterial.HasProperty("_BaseColor")? Shader.PropertyToID("_BaseColor") : Shader.PropertyToID("_Color");

        originalTrailMaterialColor = trailMaterial.GetColor(trailColorProperty);

        StartCoroutine(FadeOutCoroutine());
    }

    public void Initialise(Vector2 newDirection, float newSpeed)
    {
        direction = newDirection.normalized;
        speed = newSpeed;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private IEnumerator FadeOutCoroutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, lifetime - fadeDuration));

        Color originalSprite = spriteRenderer.color;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / fadeDuration);

            Color sprite = originalSprite;
            sprite.a = originalSprite.a * fade;
            spriteRenderer.color = sprite;

            Color trailColour = originalTrailMaterialColor;
            trailColour.a = originalTrailMaterialColor.a * fade;
            trailMaterial.SetColor(trailColorProperty, trailColour);

            yield return null;
        }

        Destroy(gameObject);
    }
}