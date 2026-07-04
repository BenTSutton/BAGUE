using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Float Logo")]
    [SerializeField] private RectTransform logo;
    [SerializeField] private float floatDistance = 10f;
    [SerializeField] private float floatSpeed = 0.55f;
    [SerializeField] private float tiltAmount = 0.75f;

    private Vector2 logoRestingPosition;

    private void Start()
    {
        logoRestingPosition = logo.anchoredPosition;
    }

    private void Update()
    {
        //Move the logo!
        float time = Time.unscaledTime * floatSpeed * Mathf.PI * 2f;
        float verticalWave = Mathf.Sin(time);
        float tiltWave = Mathf.Sin(time * 0.5f);

        logo.anchoredPosition =
            logoRestingPosition + Vector2.up * verticalWave * floatDistance;

        logo.localRotation =
            Quaternion.Euler(0f, 0f, tiltWave * tiltAmount);
    }
}