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
        private ActorSoundController soundController;
    
        void Awake()
        {
            soundController = transform.Find("FootsoundController").GetComponent<ActorSoundController>();
            playerRb = GetComponent<Rigidbody2D>();
            Anim = GetComponent<Animator>();
        }

        void FixedUpdate()
        {
            Vector3 direction = InputManager.Instance.GetMoveVector();
            bool isMoving = direction.magnitude > 0;
            if (Input.GetKey(KeyCode.LeftShift) && CharacterStat.instance.curStamina!=0 && CharacterStat.instance.canRun && isMoving)
            {
                playerRb.velocity = direction.normalized * playerMoveSpeed * Time.deltaTime * 1.5f;    
                Anim.speed = 1.5f;
                CharacterStat.instance.isRun = true;
                if(CharacterStat.instance.curStamina >= 0)
                {
                    CharacterStat.instance.ChangeStamina(-10);
                }
            }
            else
            {
                playerRb.velocity = direction.normalized * playerMoveSpeed * Time.deltaTime;
                Anim.speed = 1f;
                CharacterStat.instance.isRun = false;
                CharacterStat.instance.ChangeStamina(+5);
            }

            if (isMoving)
            {
                soundController.StartFootstepSoundPlay(CharacterStat.instance.isRun);
            } else
            {
                soundController.StopFootstepSoundPlay();
            }
            

            if(Math.Abs(direction.y) == 1 || Math.Abs(direction.x) == 1)
            {
                Anim.SetFloat("LastMoveX", direction.x);
                Anim.SetFloat("LastMoveY", direction.y);
            }
            
            Anim.SetFloat("MoveX", playerRb.velocity.x);
            Anim.SetFloat("MoveY", playerRb.velocity.y);
        }
        public void StopMovement()
        {
            playerMoveSpeed = 0f; 
            Anim.SetFloat("MoveX", 0);
            Anim.SetFloat("MoveY", 0);
        }

        public void ResumeMovement()
        {
            playerMoveSpeed = 150f;
        }
    }
}


