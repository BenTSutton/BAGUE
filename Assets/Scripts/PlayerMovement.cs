using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public float moveInput;

    public Transform groundCheck;
    public float groundCheckRadius= 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        //Checks if the GroundCheck object overlaps with ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        //Moves the player horizontally
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
       
        //Checks if player tries to jump and is grounded, and jumps if so
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
   }
}
