using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCamera
{
    public class CharacterMove : MonoBehaviour
    {
        private Rigidbody2D playerRb;
        private Animator Anim;
        public float playerMoveSpeed = 150f;
        
        void Awake()
        {
            playerRb = GetComponent<Rigidbody2D>();
            Anim = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            playerRb.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * playerMoveSpeed * Time.deltaTime;

            if(Math.Abs(Input.GetAxisRaw("Horizontal")) == 1 || Math.Abs(Input.GetAxisRaw("Vertical")) == 1)
            {
                Anim.SetFloat("LastMoveX", Input.GetAxisRaw("Horizontal"));
                Anim.SetFloat("LastMoveY", Input.GetAxisRaw("Vertical"));
            }
            
            Anim.SetFloat("MoveX", playerRb.velocity.x);
            Anim.SetFloat("MoveY", playerRb.velocity.y);
        }
    }
}


