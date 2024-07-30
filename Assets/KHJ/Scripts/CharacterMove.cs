using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{
    private Rigidbody2D playerRb;
    private Animator Anim;
    public float playerMoveSpeed = 150f;
    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        playerRb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * playerMoveSpeed * Time.deltaTime;

        Anim.SetFloat("MoveX", playerRb.velocity.x);
        Anim.SetFloat("MoveY", playerRb.velocity.y);
    }
}
