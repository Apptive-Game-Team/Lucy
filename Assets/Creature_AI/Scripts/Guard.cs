using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Creature
{
    public class Guard : Creature
    {

        [SerializeField] List<Node> MoveNodes = new List<Node>();
        private int patrolMoveFlag = 0;

        void Start()
        {
            base.Start();
        }

        public override IEnumerator PatrolAction()
        {
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

    }
}


