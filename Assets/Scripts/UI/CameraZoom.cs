using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera cam;

    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public float smoothSpeed = 5f;

    private float targetZoom;

    void Start()
    {
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        targetZoom -= scroll * zoomSpeed;

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            Time.deltaTime * smoothSpeed
        );
    }
}