using System.Collections.Generic;
using Scripts_Creatures.Util;
using UnityEngine;

namespace Scripts_Creatures.Creatures
{
    public class Guard : Creature
    {

        [SerializeField] List<Node> MoveNodes = new List<Node>();
        private int patrolMoveFlag = 0;
        private bool isPatrolling = true;

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
            if (MoveNodes.Count == 0)
            {
                return;
            }
            targetPosition.Set(MoveNodes[patrolMoveFlag].X, MoveNodes[patrolMoveFlag].Y, 0);
            SetPathToPosition(targetPosition);
        }

        protected override void PatrolUpdate()
        {
            if (isArrived && MoveNodes.Count > 0)
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

        public void StopPatrol()
        {
            if (!isPatrolling)
            {
                return;
            }
            isPatrolling = false;
            // StopAllCoroutines also killed CreatureUpdate, which StartPatrol never restarted,
            // leaving the guard brain-dead after the NPC event.
            if (moveOnPathCoroutine != null)
            {
                StopCoroutine(moveOnPathCoroutine);
                moveOnPathCoroutine = null;
            }
            path = null;
        }

        public void StartPatrol()
        {
            if (!isPatrolling)
            {
                isPatrolling = true;
                moveOnPathCoroutine = StartCoroutine(MoveOnPath());
            }
        }
    }
}


