using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCamera
{
    public abstract class Character
    {
        protected Animator animator;
        protected float CharacterMoveSpeed = 5f;

        public void Initialize(Animator anim)
        {
            animator = anim;
        }

        public abstract void Move();
    }

    public class Stop : Character
    {
        public override void Move()
        {
            animator.SetFloat("MoveX",0);
            animator.SetFloat("MoveY",0);
        }
    }

    public class Up : Character
    {
        public override void Move()
        {
            animator.transform.Translate(Vector2.up * CharacterMoveSpeed * Time.deltaTime);
            animator.SetFloat("MoveY", 1f);
            animator.SetFloat("LastMoveY",1f);
            animator.SetFloat("LastMoveX",0);
        }
    }

    public class Down : Character
    {
        public override void Move()
        {
            animator.transform.Translate(Vector2.down * CharacterMoveSpeed *  Time.deltaTime);
            animator.SetFloat("MoveY", -1f);
            animator.SetFloat("LastMoveY",-1f);
            animator.SetFloat("LastMoveX",0);
        }
    }

    public class Right : Character
    {
        public override void Move()
        {
            animator.transform.Translate(Vector2.right * CharacterMoveSpeed *  Time.deltaTime);
            animator.SetFloat("MoveX", 1f);
            animator.SetFloat("LastMoveX",1f);
            animator.SetFloat("LastMoveY",0);
        }
    }

    public class Left : Character
    {
        public override void Move()
        {
            animator.transform.Translate(Vector2.left * CharacterMoveSpeed *  Time.deltaTime);
            animator.SetFloat("MoveX", -1f);
            animator.SetFloat("LastMoveX",-1f);
            animator.SetFloat("LastMoveY",0);
        }
    }



    public class CharacterMove2 : MonoBehaviour
    {
        private Character characterMovement;
        private Animator animator;
        

        void Start()
        {
            animator = GetComponent<Animator>();

            characterMovement = new Up();
            characterMovement.Initialize(animator);
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                characterMovement = new Up();
                characterMovement.Initialize(animator);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                characterMovement = new Down();
                characterMovement.Initialize(animator);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                characterMovement = new Left();
                characterMovement.Initialize(animator);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                characterMovement = new Right();
                characterMovement.Initialize(animator);
            }
            else
            {
                characterMovement = new Stop();
                characterMovement.Initialize(animator);
            }

            if (characterMovement != null)
            {
                characterMovement.Move();
            }
        }
    }
}
