using UnityEngine;

public class LineScroller : MonoBehaviour
{
    public float speed = 0.5f;

    private Material mat;
    private Vector2 offset;

    void Start()
    {
        mat = GetComponent<LineRenderer>().material;
    }

    void Update()
    {
        offset.x -= speed * Time.deltaTime;
        mat.mainTextureOffset = offset;
    }
}