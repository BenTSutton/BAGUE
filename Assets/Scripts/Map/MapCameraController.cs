using UnityEngine;

//This class lets you scroll up and down the map using scroll wheel
public class MapCameraController : MonoBehaviour
{
    public float scrollSpeed = 10f;
    public float minY = 0f;
    public float maxY = 25f;

    public float smoothSpeed = 5f;

    Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        targetPos.y += scroll * scrollSpeed * 10f;

        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smoothSpeed * Time.deltaTime
        );
    }
}