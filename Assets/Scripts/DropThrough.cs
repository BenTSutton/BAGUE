using UnityEngine;
using System.Collections;

public class DropThrough : MonoBehaviour
{

    public Collider2D toDropCollider;
    public Collider2D platformCollider;

    void Start()
    {
        toDropCollider = GameObject.Find("Player").GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartCoroutine(DoDrop());
        }
    }

    IEnumerator DoDrop()
    {
        Debug.Log("Do Drop");
        Physics2D.IgnoreCollision(toDropCollider, platformCollider, true);
        //platformEffector.rotationalOffset = 180f;
        yield return new WaitForSeconds(1.0f);
        Physics2D.IgnoreCollision(toDropCollider, platformCollider, false);
        //platformEffector.rotationalOffset = 0;
    }
}
