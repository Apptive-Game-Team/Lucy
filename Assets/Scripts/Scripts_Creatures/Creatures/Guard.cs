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
            if (!enabled) return;
            minSpeed = 1;
            maxSpeed = 2;
            actions[status].Start();
        }

        protected override void PatrolStart()
        {
            base.PatrolStart();
            if (MoveNodes.Count == 0) { SetRandomPath(); return; }
            targetPosition.Set(MoveNodes[patrolMoveFlag].X, MoveNodes[patrolMoveFlag].Y, 0);
            SetPathToPosition(targetPosition);
        }

        protected override void PatrolUpdate()
        {
            if (MoveNodes.Count == 0) { base.PatrolUpdate(); return; }
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

        public void StopPatrol()
        {
            isPatrolling = false;
            StopAllCoroutines();
            if (soundController != null) soundController.StopFootstepSoundPlay();
        }

        public void StartPatrol()
        {
            if (!isPatrolling && enabled)
            {
                isPatrolling = true;
                status = CreatureStatus.PATROL;
                isArrived = false;
                actions[status].Start();
                // StopPatrol also stopped sensing and state updates, not just movement.
                StartCoroutine(CreatureUpdate());
                StartCoroutine(MoveOnPath());
            }
        }
    }
}


