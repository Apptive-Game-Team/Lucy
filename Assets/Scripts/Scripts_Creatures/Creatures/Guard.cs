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
            actions[status].Start();
        }

        protected override void PatrolStart()
        {
            base.PatrolStart();
            targetPosition.Set(MoveNodes[patrolMoveFlag].X, MoveNodes[patrolMoveFlag].Y, 0);
            SetPathToPosition(targetPosition);
        }

        protected override void PatrolUpdate()
        {
            if (isArrived)
            {
                patrolMoveFlag++;
                if (patrolMoveFlag >= MoveNodes.Count)
                {
                    patrolMoveFlag = 0;
                }
                targetPosition.Set(MoveNodes[patrolMoveFlag].X, MoveNodes[patrolMoveFlag].Y, 0);
                SetPathToPosition(targetPosition);
                isArrived = false;
            }
            detector.SetLookingDirection(direction);
            DetectPlayer();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }
    }
}


