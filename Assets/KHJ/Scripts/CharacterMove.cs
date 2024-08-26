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
            Vector3 direction = InputManager.Instance.GetMoveVector();
            if (Input.GetKey(KeyCode.LeftShift))
            {
                playerRb.velocity = direction.normalized * playerMoveSpeed * Time.deltaTime * 1.5f;    
                Anim.speed = 1.5f;
            }
            else
            {
                playerRb.velocity = direction.normalized * playerMoveSpeed * Time.deltaTime;
                Anim.speed = 1f;
            }
            

            if(Math.Abs(direction.y) == 1 || Math.Abs(direction.x) == 1)
            {
                Anim.SetFloat("LastMoveX", direction.x);
                Anim.SetFloat("LastMoveY", direction.y);
            }
            
            Anim.SetFloat("MoveX", playerRb.velocity.x);
            Anim.SetFloat("MoveY", playerRb.velocity.y);
        }
    }
}


