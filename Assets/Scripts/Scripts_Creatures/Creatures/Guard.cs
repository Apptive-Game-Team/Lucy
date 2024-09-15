using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Guard : Creature
    {

        [SerializeField] List<Node> MoveNodes = new List<Node>();
        private int patrolMoveFlag = 0;

        protected override void Start()
        {
            base.Start();
            minSpeed = 1;
            maxSpeed = 2;
            actions[(int)status].Play();
            StartCoroutine(MoveOnPath());
        }

        public override IEnumerator PatrolAction()
        {
            speed = minSpeed;
            if (transform.position.x == MoveNodes[patrolMoveFlag].X && transform.position.y == MoveNodes[patrolMoveFlag].Y){
                patrolMoveFlag++;
                if (patrolMoveFlag >= MoveNodes.Count)
                {
                    patrolMoveFlag = 0;
                }
            }
            targetPosition.Set(MoveNodes[patrolMoveFlag].X, MoveNodes[patrolMoveFlag].Y, 0);
            GetPathToPosition(targetPosition);

            DetectPlayer();
            yield return new WaitForSeconds(0.1f);
            actions[(int)status].Play();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
}


