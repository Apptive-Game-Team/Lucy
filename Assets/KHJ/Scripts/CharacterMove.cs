using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    private Rigidbody2D playerRb;
    private Animator myAnim;
    public float playerMoveSpeed = 0.1f;
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        myAnim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        playerRb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * playerMoveSpeed * Time.deltaTime;

        myAnim.SetFloat("MoveX", playerRb.velocity.x);
        myAnim.SetFloat("MoveY", playerRb.velocity.y);
    }
}
